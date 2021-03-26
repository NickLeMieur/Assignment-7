using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Assignment5_AddItem
{
    class Item
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
        public async Task<string> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            Table items = Table.LoadTable(client, tableName);
            Item newItem = JsonConvert.DeserializeObject<Item>(input.Body);

            if (newItem.company.ToUpper() == "B")
            {
                if (0 < newItem.rating && newItem.rating < 11)
                {
                    int rate = newItem.rating;
                    if(newItem.rating == 1)
                    {

                    }
                    else
                    {
                        newItem.rating = (rate / 2);
                    }
                    
                }
                else
                {
                    return "Error: Rating for Company B is not between 1 and 10";
                }
            }
            else if (newItem.company.ToUpper() == "A")
            {
                if (0 < newItem.rating && newItem.rating < 6)
                {

                }
                else
                {
                    return "Error: Rating for Company A is not between 1 and 5";
                }
            }
            else
            {
                return "Error: Company does not match A or B";
            }

            List<string> result = newItem.description.Split(' ').ToList();

            string lastInstanceOfO = result.FindLast(x => x.ToUpper().Contains("O"));
            int lastO = result.FindLastIndex(x => x.ToUpper().Contains("O"));

            string lastInstanceOfE = result.FindLast(x => x.ToUpper().Contains("E"));
            int lastE = result.FindLastIndex(x => x.ToUpper().Contains("E"));

            if(newItem.description == "")
            {
                newItem.description = "Not Available";
                newItem.lastInstanceOfWord = "Not Available";
            }
            else if (lastInstanceOfE == null && lastInstanceOfO == null)
            {
                newItem.lastInstanceOfWord = "Not Available";
            }
            else if(lastE > lastO)
            {
                newItem.lastInstanceOfWord = lastInstanceOfE;
            }
            else if (lastO > lastE)
            {
                newItem.lastInstanceOfWord = lastInstanceOfO;
            }
            else
            {
                newItem.lastInstanceOfWord = lastInstanceOfO;
            }

            PutItemOperationConfig config = new PutItemOperationConfig();
            config.ReturnValues = ReturnValues.AllOldAttributes;

            //Document myDoc = await items.PutItemAsync(Document.FromJson(JsonConvert.SerializeObject(newItem)), config);
            //return myDoc.ToJson();

            await items.PutItemAsync(Document.FromJson(JsonConvert.SerializeObject(newItem)), config);
            return input.Body;
        }
    }
}
