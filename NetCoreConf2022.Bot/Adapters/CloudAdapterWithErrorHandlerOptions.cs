using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;

using IMiddleware = Microsoft.Bot.Builder.IMiddleware;

namespace NetCoreConf2022.Bot.Adapters;

internal sealed class CloudAdapterWithErrorHandlerOptions
{
    public CloudAdapterWithErrorHandlerOptions(BotFrameworkAuthentication botFrameworkAuthentication, IBotTelemetryClient botTelemetryClient, IEnumerable<BotState> botStates, IEnumerable<IMiddleware> botMiddlewares, ILogger<CloudAdapterWithErrorHandler> logger)
    {
        BotFrameworkAuthentication = botFrameworkAuthentication;
        BotTelemetryClient = botTelemetryClient;
        BotStates = botStates;
        Middlewares = botMiddlewares;
        Logger = logger;
    }

    public BotFrameworkAuthentication BotFrameworkAuthentication { get; init; }

    public IBotTelemetryClient BotTelemetryClient { get; init; }

    public IEnumerable<BotState> BotStates { get; init; }

    public IEnumerable<IMiddleware> Middlewares { get; init; }

    public ILogger<CloudAdapterWithErrorHandler> Logger { get; init; }
}
