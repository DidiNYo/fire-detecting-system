﻿using ExternalServices.Models;
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

        private List<TagInfo> tagInfoList;

        private static Credentials credentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText("Credentials.json"));


        public APIService()
        {
            username = credentials.Username;
            password = credentials.Password;
            client = new HttpClient();
            graphQLClient = new GraphQLClient("http://aspires.icb.bg//query/api/graphql"); //GraphQL Endpoint
        }


        private async Task DiscoverServerAsync()
        {
            var req = new DiscoveryDocumentRequest
            {
                Address = "http://aspires.icb.bg/identityserver", //Identity Server
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

        public async Task<List<OrganizationItem>> GetOrganizationItems()
        {
            if (organization == null)
            {
                await GetOrganization();
            }

       
            return organization.Items.FindAll(o => o.TypeId == 11 || o.TypeId == 12 || o.TypeId == 13);
        }

        public async Task<List<TagInfo>> GetTagInfoAsync()
        {
            if (organization == null)
            {
                await GetOrganization();
            }

            tagInfoList = new List<TagInfo>();
            for (int i = 0; i < organization.Items.Count(); i++)
            {
                for (int j = 0; j < organization.Items[i].Tags.Count; j++)
                {
                   
                   tagInfoList.Add(organization.Items[i].Tags[j]);
                }
            }

            return tagInfoList;
        }

        // SSEEAA - Temp 20 , RH 50
        // AASSBB - ....
        public async Task<Dictionary<string, List<KeyValuePair<string, string>>>> GetLastValuesFromSenssorsAsync()
        {
            if (tagInfoList == null)
            {
                await GetTagInfoAsync();
            }

            List<KeyValuePair<string, string>> valuesList = new List<KeyValuePair<string, string>>();

            List<TagItemValue> lastData = new List<TagItemValue>();

            foreach (var tagInfo in tagInfoList)
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
                    List<TagItemValue> lastTagItemValue = response.GetDataFieldAs<List<TagItemValue>>("last");
                    if (lastTagItemValue.Count != 0)
                    {
                        lastData.Add(lastTagItemValue[0]);
                    }
                    // valuesList.Add(new KeyValuePair<string, string>(tagInfo.type.Name, lastValue.Value));
                }
            }

            return null;
        }
    }

 
}
