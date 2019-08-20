// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.5.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Constants = Microsoft.Recognizers.Text.DataTypes.TimexExpression.Constants;
using System;

namespace AQMD.Dialogs
{
    public class DateResolverDialog : CancelAndHelpDialog
    {
        private const string PromptMsgText = "For which date you want to check weather condition?";
        private static string RepromptMsgText = "I'm sorry, It seems you have asked info for invalid date. To check weather please enter a valid date from above list.";
        public static string start = "";
        public static string end = "";
        public static int AttemptCount = 0;
        public static bool isStart = false;
        public static bool isEnd = false;

        public DateResolverDialog(string id = null)
            : base(id ?? nameof(DateResolverDialog))
        {
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), DateTimePromptValidator));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        public static bool IsDateRange(string distinctTimexExpressions)
        {
            bool hasDate = false;
            string[] inputText = distinctTimexExpressions.Split(",");//split on a whitespace

            foreach (string text in inputText)
            {
                if (DateTime.TryParse(text, out DateTime Temp) == true)
                {
                    hasDate = true;
                }
                else
                {
                    hasDate = false;
                    break;
                }

            }
            if (hasDate)
            {
                return ((Convert.ToDateTime(inputText[1]) - Convert.ToDateTime(inputText[0])).TotalDays > 7) ? false : true;
            }
            else
            {
                return hasDate;
            }
        }

        public static string DateRange(string Date)
        {
            // Run the recognizer.
            var results = DateTimeRecognizer.RecognizeDateTime(Date, Culture.English);
            var distinctTimexExpressions = new HashSet<string>();
            // We should find a single result in this example.
            foreach (var result in results)
            {
                // The resolution includes a single value because there is no ambiguity.
                var values = (List<Dictionary<string, string>>)result.Resolution["values"];
                foreach (var value in values)
                {
                    if (value.TryGetValue("type", out var type))
                    {
                        if (type == "date" && value.TryGetValue("value", out var val))
                        {
                            distinctTimexExpressions.Add(val + ',' + val);
                        }
                        if (type == "daterange" && value.TryGetValue("start", out var start) && value.TryGetValue("end", out var end))
                        {

                            distinctTimexExpressions.Add(start + ',' + end);
                        }
                    }
                    break;
                }
            }
            return string.Join(',', distinctTimexExpressions);
        }
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var timex = (string)stepContext.Options;
            var time = DateRange(timex);
            var promptMessage = MessageFactory.Text(PromptMsgText, PromptMsgText, InputHints.ExpectingInput);
            var repromptMessage = MessageFactory.Text(RepromptMsgText, RepromptMsgText, InputHints.ExpectingInput);
            // We have a Date we just need to check it is unambiguous.
            var timexProperty = new TimexProperty(time);
            if (!IsDateRange(time))
            {

                var getFeedback = stepContext.Context.Activity.CreateReply();
                var feedbackChoices = new HeroCard
                {
                   // Text = "We could not identify the valid date . Please select any of the below.",
                    Buttons = new List<CardAction>
                    {
                        new CardAction() { Title = "Next week", Type = ActionTypes.ImBack, Value = "next week"},
                        new CardAction() { Title = "Next weekend", Type = ActionTypes.ImBack, Value = "next weekend"},
                        new CardAction() { Title = "Tomorrow", Type = ActionTypes.ImBack, Value = "tomorrow"},
                        new CardAction() { Title = "Today", Type = ActionTypes.ImBack, Value = "today"},
                        new CardAction() { Title = "Last week", Type = ActionTypes.ImBack, Value = "last week"},
                        new CardAction() { Title = "Last weekend", Type = ActionTypes.ImBack, Value = "last weekend"},
                        new CardAction() { Title = "Yesterday", Type = ActionTypes.ImBack, Value = "yesterday"}
                    },
                };

                // Add the card to our reply to user.
                getFeedback.Attachments = new List<Attachment>() { feedbackChoices.ToAttachment() };

                await stepContext.Context.SendActivityAsync(getFeedback, cancellationToken);
                return await stepContext.PromptAsync(nameof(DateTimePrompt),
                    new PromptOptions
                    {
                        Prompt = repromptMessage,
                    }, cancellationToken);

            }
            return await stepContext.NextAsync(new List<DateTimeResolution> { new DateTimeResolution { Timex = time } }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string time = null;
            var timex = ((List<DateTimeResolution>)stepContext.Result)[0].Timex;
            var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);
            if (isDefinite)
            {
                time = timex + "," + timex;
            }
            else if (isStart && isEnd)
            {
                time = start + "," + end;
                isStart = false;
                isEnd = false;
            }
            else if (IsDateRange(timex))
            {
                time = timex;
            }

            return await stepContext.EndDialogAsync(time, cancellationToken);
        }

        private static Task<bool> DateTimePromptValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                AttemptCount = promptContext.AttemptCount;
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                var timex = promptContext.Recognized.Value[0].Timex.Split('T')[0];

                // If this is a definite Date including year, month and day we are good otherwise reprompt.
                // A better solution might be to let the user know what part is actually missing.
                var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);
                if (!isDefinite)
                {
                    start = promptContext.Recognized.Value[0].Start;
                    end = promptContext.Recognized.Value[0].End;
                    isStart = new TimexProperty(start).Types.Contains(Constants.TimexTypes.Definite);
                    isEnd = new TimexProperty(end).Types.Contains(Constants.TimexTypes.Definite);
                    if (isStart && isEnd && ((Convert.ToDateTime(end) - Convert.ToDateTime(start)).TotalDays > 7))
                        return Task.FromResult(false);
                    else
                        return Task.FromResult(true);

                }

                return Task.FromResult(isDefinite);
            }

            return Task.FromResult(false);
        }
    }
}
