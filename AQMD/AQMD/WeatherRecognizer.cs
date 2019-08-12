// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;


namespace AQMD
{
    public class WeatherRecognizer : IRecognizer
    {
        private readonly LuisRecognizer _recognizer;
        public string APIXUKey { get; private set; }
        public WeatherRecognizer(IConfiguration configuration)
        {
            APIXUKey = configuration["APIXUKey"];
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            Dispatch = new LuisRecognizer(new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com"),
                new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true },
                true);

            SampleQnA = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAEndpointKey"],
                Host = configuration["QnAEndpointHostName"]
            });
        }
        public virtual bool IsConfigured => Dispatch != null && SampleQnA != null;
        public LuisRecognizer Dispatch { get; private set; }
        public QnAMaker SampleQnA { get; private set; }
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            => await _recognizer.RecognizeAsync(turnContext, cancellationToken);

        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
            => await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
    }
}
