// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.5.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using AQMD;
using AQMD.Shared;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Bot.Builder.Community.Adapters.Alexa;
using AQMD.CognitiveModels;

namespace AQMD.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly WeatherRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        private const string HelpMsgText = "Please drop an email to manas_r@preludesys.com or Contact on +1(123)456-7890";

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(WeatherRecognizer luisRecognizer, WeatherDialog weatherDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(weatherDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("LUIS or QnA is not configured.", inputHint: InputHints.IgnoringInput), cancellationToken);
                Logger.LogInformation("LUIS or QnA is not configured.Update the Luis and QNA setting in Appsetting.json");
                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?\nSay something like \"How is Weather in California?\"";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext,CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the WeatherDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(WeatherDialog), new WeatherDetails(), cancellationToken);
            }
            // Call LUIS and gather any potential Weather details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.Dispatch.RecognizeAsync<WeatherInfo>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case WeatherInfo.Intent.l_Weather:
                    var weatherDetails = new WeatherDetails()
                    {
                        // Get Location and Date from the composite entities arrays.
                        Location = luisResult.Location,
                        Date = luisResult.Date
                    };
                    // Run the Weather Dialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(WeatherDialog), weatherDetails, cancellationToken);
                case WeatherInfo.Intent.q_qna:
                    await ProcessSampleQnAAsync(stepContext.Context, cancellationToken);
                    break;
                default:
                    Logger.LogInformation($"Unrecognized intent: {luisResult.TopIntent().intent}.");
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Sorry, I didn't get that. Please try asking in a different way.For Example Ask Weather in California."), cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task ProcessWeatherAsync(ITurnContext context, WeatherDetails weatherDetails, CancellationToken cancellationToken)
        {
            Logger.LogInformation("ProcessWeatherAsync");

            // Retrieve LUIS results for Weather.
            if (weatherDetails != null)
            {
                // await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessWeather entities were found in the message:\n\n{string.Join("\n\n", result.Entities.Select(i => i.Entity))}"), cancellationToken);
                GetWeatherInPlaceAction action = new GetWeatherInPlaceAction();
                var place = weatherDetails.Location;
                var date = weatherDetails.Date;
                var weatherCard = (AdaptiveCards.AdaptiveCard)await action.FulfillAsync(place, date ,_luisRecognizer.APIXUKey);
                var alexaWeatherCard = (AlexaCardContent)await action.FulfillAsyncAlexa(place, date , _luisRecognizer.APIXUKey);

                if (weatherCard == null)
                {
                    await context.SendActivityAsync(MessageFactory.Text($"Are you sure that's a real City? {place}."), cancellationToken);
                    if (alexaWeatherCard == null)
                    {
                        context.AlexaSetCard(new AlexaCard()
                        {
                            Type = AlexaCardType.Standard,
                            Title = "AQMD Weather",
                            Content = $"Are you sure that's a real City? {place}.",
                        });
                    }
                }
                else
                {
                    // await turnContext.AlexaSendProgressiveResponse("Hold on, I will just check that for you.");
                    var adaptiveCardAttachment = new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = weatherCard,
                    };
                    await context.SendActivityAsync(alexaWeatherCard.text);
                    context.AlexaSetCard(new AlexaCard()
                    {
                        Type = AlexaCardType.Standard,
                        Title = alexaWeatherCard.title,
                        Content = alexaWeatherCard.text,
                        Image = new AlexaCardImage { SmallImageUrl = alexaWeatherCard.smallImageUrl }
                    });
                    await context.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment), cancellationToken);

                }

            }
            else
            {

                await context.SendActivityAsync(MessageFactory.Text($"Please specify a city or location. For Example Ask Weather in California."), cancellationToken);
                context.AlexaSetCard(new AlexaCard()
                {
                    Type = AlexaCardType.Standard,
                    Title = $"Weather App",
                    Content = $"Please specify a city or location. For Example Ask Weather in California.",
                });

            }

        }

        private async Task ProcessSampleQnAAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            Logger.LogInformation("ProcessSampleQnAAsync");

            var results = await _luisRecognizer.SampleQnA.GetAnswersAsync(context);
            if (results.Any())
            {
                switch (results.First().Answer.ToLower())
                {
                    case "help":
                    case "?":
                        await context.SendActivityAsync(MessageFactory.Text(HelpMsgText), cancellationToken);
                        new DialogTurnResult(DialogTurnStatus.Waiting);
                        break;
                    case "cancel":
                        await context.SendActivityAsync(MessageFactory.Text("Cancelling."), cancellationToken);
                        break;
                    case "quit":
                        break;
                    default:
                        await context.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
                        break;

                }
                
            }
            else
            {
                await context.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
            }
        }
        
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("WeatherDialog") was cancelled, the user failed to confirm or if the intent wasn't getting Weather Info
            // the Result here will be null.
            if (stepContext.Result is WeatherDetails result)
            {
                // Now we have all the booking details call the booking service.
                await ProcessWeatherAsync(stepContext.Context, result, cancellationToken);

            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
