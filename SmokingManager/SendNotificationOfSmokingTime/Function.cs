using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendNotificationOfSmokingTime
{
    public class Function
    {
        public string FunctionHandler(Input input, ILambdaContext context)
        {
            return input.Environment;
        }

        public class Input
        {
            [JsonPropertyName("Env")]
            public string Environment { get; set; }
        }
    }
}
