// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.5.0

using System;
using System.Threading;
using System.Threading.Tasks;
using AQMD;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AQMD.Dialogs
{
    public class WeatherDialog : CancelAndHelpDialog
    {

        private const string LocationStepMsgText = "For which location you want to check weather condition?";

        public WeatherDialog()
            : base(nameof(WeatherDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                LocationStepAsync,
                DateStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var weatherDetails = (WeatherDetails)stepContext.Options;
            if (weatherDetails.Location == null)
            {
                var promptMessage = MessageFactory.Text(LocationStepMsgText, LocationStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            //if (weatherDetails.Date != null && IsAmbiguous(weatherDetails.Date))
            //{
            //    return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), weatherDetails.Date, cancellationToken);
            //}
            return await stepContext.NextAsync(weatherDetails.Location, cancellationToken);
        }

        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var weatherDetails = (WeatherDetails)stepContext.Options;

            weatherDetails.Location = (string)stepContext.Result;

            if (weatherDetails.Date != null && IsAmbiguous(weatherDetails.Date))
            {
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), weatherDetails.Date, cancellationToken);
            }

            return await stepContext.NextAsync(weatherDetails, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var weatherDetails = (WeatherDetails)stepContext.Options;
            //weatherDetails.Location = (string)stepContext.Result;
            return await stepContext.EndDialogAsync(weatherDetails, cancellationToken);
        }
        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    //    protected bool CheckDate(string date)
    //    {
    //        try
    //        {
    //            DateTime dt = DateTime.Parse(date);
    //            return true;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }
    }
}
