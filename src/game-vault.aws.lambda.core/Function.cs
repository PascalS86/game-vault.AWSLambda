using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace game_vault.aws.lambda.core
{
    public class Functions
    {
        private readonly string ACCESS_CHECK = "ACCESS_CHECK";
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
        }


        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Request\n");
            //TODO: Add Client Authentication
            if(request.HttpMethod.ToLower() == "get"){
                return await GetCall(request, context);
            }
            else{
                return await PostCall(request, context);
            }
            
        }

        public async Task<APIGatewayProxyResponse> GetCall(APIGatewayProxyRequest request, ILambdaContext context){
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body =  JsonConvert.SerializeObject(await CsandraGame.GetGames(ACCESS_CHECK)),
                Headers = new Dictionary<string, string> { 
                    { "Content-Type", "application/json" } ,
                    { "Access-Control-Allow-Origin", "*" }
                }
            };

            return response;
        }
        
        /// <summary>
        /// A Lambda function to respond to HTTP POST methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public async Task<APIGatewayProxyResponse> PostCall(APIGatewayProxyRequest request, ILambdaContext context)
        {
             
            context.Logger.LogLine("POST Request\n");
            string result = "";
            if(request.Path.Contains("access")){
                var data = JsonConvert.DeserializeObject<ClientClass>(request.Body);
                result = ACCESS_CHECK.Split(',').Count(c=> c.ToLower() == data.Client.ToLower()) > 0?"":"No changes allowed";
            }
            else{
                var data = JsonConvert.DeserializeObject<DataClass>(request.Body);
                result = await CsandraGame.SaveGame(data.Data, data.Client);
            }
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)(string.IsNullOrWhiteSpace(result)?HttpStatusCode.OK:HttpStatusCode.BadRequest),
                Body =  result,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" } }
            };

            return response;
        }
    }
}
