using System.Globalization;
using System.Text;

using Azure;
using Azure.AI.Language.QuestionAnswering;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

using NetCoreConf2022.Bot.Infrastructure;

using Newtonsoft.Json.Linq;

namespace NetCoreConf2022.Bot.Dialogs;

internal sealed class QuestionAnsweringDialog : Dialog
{
    private const string VerboseActivityValueParameter = @"x-encamina-questionanswering-verbose";

    private readonly bool withVerbose;
    private readonly QuestionAnsweringClient client;
    private readonly QuestionAnsweringProject project;

    private readonly IMetadataHandler metadataHandler;

    public QuestionAnsweringDialog(IMetadataHandler metadataHandler, IConfiguration configuration)
    {
        this.metadataHandler = metadataHandler;

        withVerbose = configuration.GetValue<bool>(Constants.Settings.Verbose, false);

        client = new QuestionAnsweringClient(configuration.GetValue<Uri>(Constants.Settings.KnowledgeUri), new AzureKeyCredential(configuration.GetValue<string>(Constants.Settings.KnowledgeKeyCredential)));
        project = new QuestionAnsweringProject(configuration.GetValue<string>(Constants.Settings.KnowledgeBaseName), configuration.GetValue<string>(Constants.Settings.KnowledgeDeploymentSlot));
    }

    /// <inheritdoc />
    public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
    {
        var answerOptions = new AnswersOptions()
        {
            ConfidenceThreshold = 0.5,
            Filters = new QueryFilters()
            {
                LogicalOperation = LogicalOperationKind.Or,
                MetadataFilter = new MetadataFilter()
                {
                    LogicalOperation = LogicalOperationKind.Or,
                },
            },
            RankerKind = RankerKind.Default,
            Size = 3,
        };

        var message = dc.Context.Activity.Text;

        // Normalize symbols and diacritics...
        message = NormalizeDiacritics(message);

        // Detect metadata to filter...
        (await metadataHandler.HandleMessageAsync(message, cancellationToken)).ToList().ForEach(m => answerOptions.Filters.MetadataFilter.Metadata.Add(new MetadataRecord(m.Key, m.Value)));

        var response = await client.GetAnswersAsync(message, project, answerOptions, cancellationToken);

        var answerText = response.Value.Answers[0].Answer; // Get the first answer, which should be the one with highest confidence score or the default answers...
        var answerActivity = MessageFactory.Text(answerText, answerText);

        if (withVerbose || VerboseFromTurnContextValue(dc.Context))
        {
            answerActivity.Properties = BuildVerboseInformation(response.Value.Answers);
        }

        var resourceResponse = await dc.Context.SendActivityAsync(answerActivity, cancellationToken);

        return await dc.EndDialogAsync(resourceResponse, cancellationToken);
    }

    private static JObject BuildVerboseInformation(IEnumerable<KnowledgeBaseAnswer> answers)
    {
        return JObject.FromObject(new
        {
            Verbose = new
            {
                Answers = answers.Select(a => new
                {
                    Id = a.QnaId,
                    a.Answer,
                    AssociatedQuestions = a.Questions,
                    ConfidenceSocre = a.Confidence,
                    a.Metadata,
                    a.Source,
                }),
            },
        });
    }

    private static string NormalizeDiacritics(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var removeCharacters = new char[] { '¿', '?', '¡', '!' };
        var replaceCharacters = new Dictionary<char, char> { { '̃', 'ñ' } }; // replace ̃  with ñ

        var builder = new StringBuilder(value.Length);
        var normalized = string.Join(string.Empty, value.ToCharArray().Where(c => !removeCharacters.Contains(c))).Normalize(NormalizationForm.FormD);

        foreach (var character in normalized)
        {
            if (replaceCharacters.TryGetValue(character, out var replaceCharacter))
            {
                builder.Length--;
                builder.Append(replaceCharacter);
            }
            else if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
            else
            {
                /* Previous conditions are required to be mutually exclusive */
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).Trim();
    }

    private static bool VerboseFromTurnContextValue(ITurnContext turnContext)
    {
        return turnContext.Activity.Value is JObject activityValue && (activityValue[VerboseActivityValueParameter]?.Value<bool>() ?? false);
    }
}
