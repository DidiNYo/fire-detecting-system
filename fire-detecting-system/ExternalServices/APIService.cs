using ExternalServices.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ExternalServices
{
    public class APIService
    {
        private readonly string username;

        private readonly string password;

        private readonly HttpClient client;

        private DiscoveryResponse discovered; //Base URL of the server. 

        private string token;

        private readonly GraphQLClient graphQLClient;

        private Organization organization;

        enum Type { Sensor = 11, Camera = 12, WeatherStation = 13 }

        //  private Dictionary<string, LastMeasurement> organisationItemLastMeasurements;

        public APIService()
        {
            username = GetConfiguration.ConfigurationInstance.ConfigurationData.Username;
            password = GetConfiguration.ConfigurationInstance.ConfigurationData.Password;
            client = new HttpClient();
            graphQLClient = new GraphQLClient(GetConfiguration.ConfigurationInstance.ConfigurationData.GraphQLClient); //GraphQL Endpoint
        }


        private async Task DiscoverServerAsync()
        {
            var req = new DiscoveryDocumentRequest
            {
                Address = GetConfiguration.ConfigurationInstance.ConfigurationData.IdentityServer, //Identity Server
                Policy =
                {
                    RequireHttps = false
                }
            };

            discovered = await client.GetDiscoveryDocumentAsync(req);

            if (discovered.IsError)
            {
                throw new Exception(discovered.Error);
            }
        }

        private async Task GetTokenAsync()
        {
            if (discovered == null)
            {
                await DiscoverServerAsync();
            }

            ClientCredentialsTokenRequest request = new ClientCredentialsTokenRequest
            {
                Address = discovered.TokenEndpoint,
                ClientId = username,
                ClientSecret = password,
                Scope = "graphqlApi queryApi"
            };

            TokenResponse tokenRequest = await client.RequestClientCredentialsTokenAsync(request);

            if (tokenRequest.IsError)
            {
                throw new Exception(tokenRequest.Error);
            }

            //Extract the token from the JSON 
            token = tokenRequest.Json.TryGetString("access_token");

            //Authorizing the GraphQL client.
            graphQLClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        }


        private async Task GetOrganization()
        {
            if (string.IsNullOrEmpty(token))
            {
                await GetTokenAsync();
            }

            GraphQLRequest request = new GraphQLRequest
            {
                Query = @"{
                    org(orgid: 6) {
   			                name,
                            items {
                                name,
          	                    typeid,
                                orgitemid,
                              	tags {
                                  tagid,
                                  type{
                                    name
                                  }
                                },
                                properties {
                                  type,
                                  value
                              }
                            }
                        }
                    }"
            };


            GraphQLResponse graphQLResponse = await graphQLClient.PostAsync(request);

            organization = graphQLResponse.GetDataFieldAs<Organization>("org");

        }

        public async Task<List<OrganizationItem>> GetOrganizationItemsAsync()
        {
            if (organization == null)
            {
                await GetOrganization();
            }

            return organization.Items.FindAll(o =>
            o.TypeId == (int)Type.Sensor ||
            o.TypeId == (int)Type.Camera ||
            o.TypeId == (int)Type.WeatherStation);
        }

        public async Task GetLastImages()
        {
            if (organization == null)
            {
                await GetOrganization();
            }

            try
            {
                foreach (OrganizationItem organizationItem in organization.Items)
                {
                    if (organizationItem.TypeId == 12)
                    {
                        foreach (var tag in organizationItem.Tags)
                        {
                            if (tag.Type == null)
                            {
                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($@"http://aspires.icb.bg/files/api/files/file?TagID={tag.TagId}");
                                request.Method = "Get";
                                request.Headers.Add("Authorization", $"Bearer {token}");

                                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                                using (Stream output = File.Open($@"..\..\Assets\{organizationItem.Name}_{tag.TagId}.jpg", FileMode.Create))
                                using (Stream input = response.GetResponseStream())
                                {
                                    await input.CopyToAsync(output);
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException exception) // catch the internal server error from the API
            {
                Debug.Print(exception.Message);
            }
        }

        public async Task<Dictionary<string, LastMeasurement>> GetLastMeasurementsAsync()
        {
            Debug.Print("STARTED GetLastMeasurementsAsync at {0}", DateTime.Now);

            if (organization == null)
            {
                await GetOrganization();
            }

            Dictionary<string, LastMeasurement> organisationItemLastMeasurements = new Dictionary<string, LastMeasurement>();
            LastMeasurement currentMeasurement;

            foreach (OrganizationItem organizationItem in organization.Items)
            {
                currentMeasurement = new LastMeasurement();
                if (organizationItem.Name != null)
                {
                    currentMeasurement.OrganizationItemName = organizationItem.Name.Trim();
                }
                currentMeasurement.OrganizationItemID = organizationItem.OrgItemId;


                foreach (TagInfo tagInfo in organizationItem.Tags)
                {
                    if (tagInfo.Type != null)
                    {

                        if (tagInfo != null)
                        {
                            int tagId = tagInfo.TagId;
                            GraphQLRequest requesForTagIds = new GraphQLRequest
                            {

                                Query = $@"{{
                                            last(tagid: {tagId}) {{
                                                date,
                                                value
                                                    }}
                                                }}"
                            };

                            GraphQLResponse response = await graphQLClient.PostAsync(requesForTagIds);
                            List<TagItemValue> currentTagItemValue = response.GetDataFieldAs<List<TagItemValue>>("last");
                            MeasurementValue currentMeasurementValue = new MeasurementValue(tagInfo.Type.Name, currentTagItemValue);
                            if (currentTagItemValue.Count != 0)
                            {
                                currentMeasurement.Values.Add(currentMeasurementValue);
                            }
                        }
                    }
                }
                if (currentMeasurement != null)
                {
                    organisationItemLastMeasurements.Add(organizationItem.Name, currentMeasurement);
                }
            }

            Debug.Print("ENDED GetLastMeasurementsAsync at {0}", DateTime.Now);

            return organisationItemLastMeasurements;
        }
    }
}
