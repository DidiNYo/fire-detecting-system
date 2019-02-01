using ExternalServices.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
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

        // private Dictionary<string, List<TagInfo>> orgItemNameListTagInfoPair;

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


            return organization.Items.FindAll(o => o.TypeId == 11 || o.TypeId == 12 || o.TypeId == 13);
        }

        public async Task<List<LastMeasurement>> GetLastMeasurementsAsync()
        {
            if (organization == null)
            {
                await GetOrganization();
            }

            List<LastMeasurement> lastMeasurements = new List<LastMeasurement>();
            LastMeasurement currentMeasurement;

            foreach (OrganizationItem organizationItem in organization.Items)
            {
                currentMeasurement = new LastMeasurement();
                if(organizationItem.Name != null)
                {
                    currentMeasurement.OrganizationItemName = organizationItem.Name;
                }
                currentMeasurement.OrganizationItemID = organizationItem.OrgItemId;


                foreach (TagInfo tagInfo in organizationItem.Tags)
                {
                    if (tagInfo.Type != null)
                    {
                        currentMeasurement.MeasurementType = tagInfo.Type.Name;

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
                            if (currentTagItemValue.Count != 0 && currentMeasurement.MeasurementType != null)
                            {
                                currentMeasurement.Values.Add(currentTagItemValue);
                            }
                        }
                    }
                }
                if (currentMeasurement != null)
                {
                    lastMeasurements.Add(currentMeasurement);
                }
            }
            return lastMeasurements;
        }
    }
}
