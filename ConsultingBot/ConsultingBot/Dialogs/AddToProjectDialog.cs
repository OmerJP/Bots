using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsultingBot.Cards;
using ConsultingData.Models;
using ConsultingData.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ConsultingBot.Dialogs
{
    public class AddToProjectDialog : CancelAndHelpDialog
    {
        public AddToProjectDialog(string dialogId) : base(dialogId)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt) + "projectName", ProjectNameValidatorAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ProjectStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        #region Waterfall steps

        // Step 1: Ensure we have a project name
        // Result is the project name from LUIS or from a user prompt
        private async Task<DialogTurnResult> ProjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            List<ConsultingProject> result = await ResolveProject(requestDetails.projectName);

            if (result == null || result.Count < 1)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt) + "projectName", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Which project do you want to add to?"),
                    RetryPrompt = MessageFactory.Text("Sorry I didn't get that, what project was it?"),
                }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(result, cancellationToken);
            }
        }

        // Step 2: Display the Add to Project card
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ?
                stepContext.Options as RequestDetails
                    :
                new RequestDetails();

            List<ConsultingProject> result = stepContext.Result as List<ConsultingProject>;
            if (result == null)
            {
                var projectName = stepContext.Result as string;
                result = await ResolveProject(projectName);
            }
            requestDetails.possibleProjects = result;

            var projectCard = await AddToProjectCard.GetCard(stepContext.Context, requestDetails);
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments.Add(projectCard.ToAttachment());
            await stepContext.Context.SendActivityAsync(reply).ConfigureAwait(false);

            return await stepContext.EndDialogAsync(requestDetails, cancellationToken);
        }
        #endregion

        #region Project name resolution
        private async Task<List<ConsultingProject>> ResolveProject(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                ConsultingDataService dataService = new ConsultingDataService();
                var possibleResults = await dataService.GetProjects(keyword);
                if (possibleResults.Count > 0)
                {
                    // We have a single result, return it
                    return possibleResults;
                }
            }
            return null;
        }

        private async Task<bool> ProjectNameValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            ConsultingDataService dataService = new ConsultingDataService();
            var keyword = promptContext.Recognized.Value;
            var projects = await ResolveProject(keyword);
            return projects != null && projects.Count > 0;
        }
        #endregion
    }
}
