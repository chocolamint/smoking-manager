using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Line.Messaging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendNotificationOfSmokingTime
{
    public class Function
    {
        public async Task FunctionHandler(Input input, ILambdaContext context)
        {
            var secretsJson = await GetSecretAsync("arn:aws:secretsmanager:ap-northeast-1:729870111298:secret:line/SmokingManager/Values-S7sLCm");
            Console.WriteLine(secretsJson);
            var secrets = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(secretsJson);
            Console.WriteLine(secrets["ChannelAccessToken"]);
            using var line = new LineMessagingClient(secrets["ChannelAccessToken"]);
            Console.WriteLine(secrets[$"{input.Environment}:Recipient"]);
        }

        public static async ValueTask<string> GetSecretAsync(string secretName)
        {
            using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName("ap-northeast-1"));
            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            };
            var response = await client.GetSecretValueAsync(request);
            return response.SecretString;
        }

        public class Input
        {
            [JsonPropertyName("Env")]
            public string Environment { get; set; }
        }
    }
}
