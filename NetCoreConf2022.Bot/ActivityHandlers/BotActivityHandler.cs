using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace NetCoreConf2022.Bot.ActivityHandlers;

internal sealed class BotActivityHandler : ActivityHandler
{
    private readonly IEnumerable<Dialog> dialogs;
    private readonly BotActivityHandlerOptions options;

    public BotActivityHandler(IEnumerable<Dialog> dialogs, BotActivityHandlerOptions options)
    {
        this.dialogs = dialogs;
        this.options = options;
    }

    /// <inheritdoc/>
    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);
        await options.ConversationState.SaveChangesAsync(turnContext, force: false, cancellationToken);
        await options.UserState.SaveChangesAsync(turnContext, force: false, cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        await base.OnMessageActivityAsync(turnContext, cancellationToken);

        await dialogs.Single(d => d is Dialogs.QuestionAnsweringDialog)
                     .RunAsync(turnContext, options.ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
    {
        if (turnContext.Activity.Name == Constants.BotEvents.WebChatGreetings)
        {
            await dialogs.Single(d => d is Dialogs.GreetingsDialog)
                         .RunAsync(turnContext, options.ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        await base.OnEventActivityAsync(turnContext, cancellationToken);
    }
}
