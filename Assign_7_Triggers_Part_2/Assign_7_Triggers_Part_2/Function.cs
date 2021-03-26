using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;



// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Assign_7_Triggers_Part_2
{
    public class Item
    {
        public string itemId;
        public string description;
        public int rating;
        public string type;
        public string company;
        public string lastInstanceOfWord;
    }

    public class Stats
    {
        public int count;
        public double averageRating;
    }
    public class Function
    {
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private string tableName = "RatingsByType";
        public async Task<List<Item>> FunctionHandler(DynamoDBEvent input, ILambdaContext context)
        {
            double total = 0;
            int count = 0;
            double currentAverage = 0;
            Table table = Table.LoadTable(client, "RatingsByType");
            List<Item> items = new List<Item>();
            List<DynamoDBEvent.DynamodbStreamRecord> records = (List<DynamoDBEvent.DynamodbStreamRecord>)input.Records;

            if (records.Count > 0)
            {
                DynamoDBEvent.DynamodbStreamRecord record = records[0];
                if (record.EventName.Equals("INSERT"))
                {
                    Document myDoc = Document.FromAttributeMap(record.Dynamodb.NewImage);
                    Item myItem = JsonConvert.DeserializeObject<Item>(myDoc.ToJson());

                    string type = myItem.type;
                    GetItemResponse res = await client.GetItemAsync(tableName, new Dictionary<string, AttributeValue>
                        {
                            {"type", new AttributeValue { S = type } }
                        }
                    );
                    Document myDoc2 = Document.FromAttributeMap(res.Item);
                    Stats myStats = JsonConvert.DeserializeObject<Stats>(myDoc2.ToJson());
                    count = myStats.count;
                    currentAverage = myStats.averageRating;
                    if(count == 0)
                    {
                        total += myItem.rating;
                    }
                    else
                    {

                        if(count >= 2)
                        {
                            total = (myItem.rating + (currentAverage * count)) / (count + 1);
                        }
                        else
                        {
                            total = (myItem.rating + currentAverage) / (count + 1);
                        }
                        
                    }
                    var request = new UpdateItemRequest
                    {
                        TableName = "RatingsByType",
                        Key = new Dictionary<string, AttributeValue>
                    {
                            { "type", new AttributeValue { S = myItem.type } }
                    },
                        AttributeUpdates = new Dictionary<string, AttributeValueUpdate>()
                    {
                        {
                            "count",
                            new AttributeValueUpdate { Action = "ADD", Value = new AttributeValue {N = "1"} }
                        },
                        {                                
                            "averageRating",
                            new AttributeValueUpdate { Action = "PUT", Value = new AttributeValue { N = (total).ToString() } }
                        },
                    },
                    };
                    await client.UpdateItemAsync(request);
                }
            }
            return items;
        }
    }
}
