using Microsoft.Bot.Builder;

namespace NetCoreConf2022.Bot.ActivityHandlers;

internal sealed class BotActivityHandlerOptions
{
    public BotActivityHandlerOptions(ConversationState conversationState, UserState userState, ILogger<BotActivityHandler> logger)
    {
        ConversationState = conversationState;
        UserState = userState;
        Logger = logger;
    }

    /// <summary>
    /// Gets the conversation state of this bot.
    /// </summary>
    public BotState ConversationState { get; init; }

    /// <summary>
    /// Gets the user state of this bot.
    /// </summary>
    public BotState UserState { get; init; }

    /// <summary>
    /// Gets the logger of this bot.
    /// </summary>
    public ILogger Logger { get; init; }
}
