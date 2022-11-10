using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

using IMiddleware = Microsoft.Bot.Builder.IMiddleware;

namespace NetCoreConf2022.Bot.Adapters;

/// <summary>
/// Bot adapters with custom error handling that implements the Bot Framework Protocol and can
/// be hosted in different cloud environmens, both public and private.
/// </summary>
internal sealed class CloudAdapterWithErrorHandler : CloudAdapter
{
    private readonly CloudAdapterWithErrorHandlerOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudAdapterWithErrorHandler"/> class.
    /// </summary>
    /// <param name="adapterOptions">Options for this bot adapter.</param>
    public CloudAdapterWithErrorHandler(CloudAdapterWithErrorHandlerOptions adapterOptions)
        : base(adapterOptions?.BotFrameworkAuthentication ?? BotFrameworkAuthenticationFactory.Create(), adapterOptions?.Logger)
    {
        options = adapterOptions;

        OnTurnError = ErrorHandlerAsync;

        InitializeWithDefaultMiddlewares(options?.Middlewares);
    }

    /// <summary>
    /// An error handler that can catch exceptions in the middleware or application.
    /// </summary>
    /// <param name="turnContext">The current turn context.</param>
    /// <param name="exception">The catched excetion.</param>
    /// <returns>A task that represents the asynchronous error handling operation.</returns>
    public async Task ErrorHandlerAsync(ITurnContext turnContext, Exception exception)
    {
        // Send a message to the user
        var errorMessageText = $@"The bot encountered an error or bug. Please provide the following identifer to track the error: {turnContext.Activity.Id}";
        var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
        await turnContext.SendActivityAsync(errorMessage);

        if (options != null)
        {
            // Send the exception telemetry.
            options.BotTelemetryClient.TrackException(exception, new Dictionary<string, string> { { @"Bot exception caught in", $"{GetType().Name} - {nameof(OnTurnError)}" } });

            // Log any leaked exception from the application.
            options.Logger.LogError(exception, @"Unhandled error in activity '{ActivityId}': {ErrorMessage}", turnContext.Activity.Id, exception.Message);

            var conversationState = options.BotStates.OfType<ConversationState>().SingleOrDefault();

            if (conversationState != null)
            {
                try
                {
                    // Delete the conversationState for the current conversation to prevent the
                    // bot from getting stuck in a error-loop caused by being in a bad state.
                    await conversationState.DeleteAsync(turnContext);
                }
                catch (Exception e)
                {
                    options.Logger.LogError(e, @"Exception caught on attempting to delete the ConversationState. Error message: {ErrorMessage}", e.Message);
                }
            }
        }

        // Send a trace activity. This information could be displayed in the Bot Framework Emulator for example.
        await turnContext.TraceActivityAsync($@"{nameof(OnTurnError)} Trace", exception.ToString(), "https://www.botframework.com/schemas/error", $@"{nameof(OnTurnError)} - Exception");
    }

    private void InitializeWithDefaultMiddlewares(IEnumerable<IMiddleware> middlewares)
    {
        if (middlewares?.Any() ?? false)
        {
            var dicMiddlewares = middlewares.ToDictionary(i => i.GetType(), i => i);

            if (dicMiddlewares.TryGetValue(typeof(TelemetryInitializerMiddleware), out var telemetryInitializerMiddleware))
            {
                Use(telemetryInitializerMiddleware);

                if (dicMiddlewares.TryGetValue(typeof(TelemetryLoggerMiddleware), out var telemetryLoggerMiddleware))
                {
                    Use(telemetryLoggerMiddleware);
                }
            }

            if (dicMiddlewares.TryGetValue(typeof(TranscriptLoggerMiddleware), out var transcriptLoggerMiddleware))
            {
                Use(transcriptLoggerMiddleware);
            }

            if (dicMiddlewares.TryGetValue(typeof(ShowTypingMiddleware), out var showTypingMiddleware))
            {
                Use(showTypingMiddleware);
            }

            if (dicMiddlewares.TryGetValue(typeof(AutoSaveStateMiddleware), out var autoSaveStateMiddleware))
            {
                Use(autoSaveStateMiddleware);
            }
        }
    }
}
