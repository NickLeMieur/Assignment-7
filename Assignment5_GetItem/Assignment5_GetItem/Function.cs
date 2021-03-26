using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Assignment5_GetItem
{
    public class Items
    {
        public string itemId;
        public string description;
        public int rating;
        public string type;
        public string company;
        public string lastInstanceOfWord;
    }
    public class Function
    {
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private string tableName = "Assignment5";
        public async Task<Items> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            string itemId = "";
            Dictionary<string, string> dict = (Dictionary<string, string>)input.QueryStringParameters;
            dict.TryGetValue("itemId", out itemId);
            GetItemResponse res = await client.GetItemAsync(tableName, new Dictionary<string, AttributeValue>
                {
                    {"itemId", new AttributeValue { S = itemId } }
                }
            );
            Document myDoc = Document.FromAttributeMap(res.Item);
            Items myItem = JsonConvert.DeserializeObject<Items>(myDoc.ToJson());
            return myItem;
        }
    }
}
