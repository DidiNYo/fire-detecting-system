﻿using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using IdentityModel.Client;
using System;
using System.Configuration;
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

        public APIService()
        {
            username = ConfigurationManager.AppSettings["APIService.Username"];
            password = ConfigurationManager.AppSettings["APIService.Password"];
            client = new HttpClient();
        }

        public async Task DiscoverServerAsync()
        {
            discovered = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = "http://aspires.icb.bg/identityserver", //Identity Server
                Policy =
                {
                    RequireHttps = false
                }
            });

            if (discovered.IsError)
            {
                throw new Exception(discovered.Error);
            }
        }

        public async Task GetTokenAsync()
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
        }

        //Using the GraphQL Client Library.
        public async Task GetData()
        {
            if (string.IsNullOrEmpty(token))
            {
                await GetTokenAsync();
            }

            GraphQLRequest request = new GraphQLRequest
            {
                Query = @"{
                        org(orgid: 6) {
                            items {
                                name
                            }
                        }
                    }"
            };

            GraphQLClient graphQLClient = new GraphQLClient("http://aspires.icb.bg//query/api/graphql"); //GraphQL Endpoint
            graphQLClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            GraphQLResponse graphQLResponse = await graphQLClient.PostAsync(request);

            //To be created objects and converted to them.
            Console.WriteLine(graphQLResponse);
        }
    }
}

