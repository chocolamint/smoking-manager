using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly TimeZoneInfo _jst = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.Id == "Tokyo Standard Time") ?? TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");

        public async Task FunctionHandler(Input input, ILambdaContext context)
        {
            var secretsJson = await GetSecretAsync("arn:aws:secretsmanager:ap-northeast-1:729870111298:secret:line/SmokingManager/Values-S7sLCm");
            var secrets = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(secretsJson);
            using var line = new LineMessagingClient(secrets["ChannelAccessToken"]);

            var recipient = secrets[$"{input.Environment}:Recipient"];
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, _jst);
            var timeString = now.TimeOfDay.Hours switch 
            {
                >= 4 and < 9 => "寝起きの",
                >= 9 and < 12 => "朝の",
                >= 12 and < 15 => "昼食後の",
                >= 15 and < 18 => "夕方の",
                >= 18 and < 21 => "夜の",
                _ => "寝る前の",
            };
            var messages = new[]
            {
                input.Initial ?
                    new TextMessage("こんにちは！タバコの時間が来たら僕が知らせるね。それまでタバコは吸っちゃダメだよ😝\r\n\r\nお助け1本に頼りたいときは一言その旨を教えてね。どうしても苦しいとき以外は基本的に頼らないようにしよう！") as ISendMessage :
                    new TemplateMessage(
                        altText: $"{timeString}喫煙タイムです⏰よく我慢したね💕この時間から1本だけタバコを吸ってもいいよ🥰",
                        template: new ButtonsTemplate(
                            text: $"{timeString}喫煙タイムです⏰よく我慢したね💕この時間から1本だけタバコを吸ってもいいよ🥰",
                            actions: new[]
                            {
                                new MessageTemplateAction("吸い始める", "吸い始めたよ🚬")
                            }
                        )
                    )
            };
            await line.PushMessageAsync(recipient, messages);
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

            public bool Initial { get; set; }
        }
    }
}
