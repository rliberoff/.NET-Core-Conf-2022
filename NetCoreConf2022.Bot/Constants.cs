using NetCoreConf2022.Bot.Dialogs;

namespace NetCoreConf2022.Bot;

/// <summary>
/// Constants.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Expected connection string names.
    /// </summary>
    internal static class ConnectionStrings
    {
        /// <summary>
        /// The name of the <c>BlobStorage</c> connection string configuration parameter.
        /// </summary>
        internal const string BlobStorage = @"BlobStorage";

        /// <summary>
        /// The name of the <c>TableStorage</c> connection string configuration parameter.
        /// </summary>
        internal const string TableStorage = @"TableStorage";
    }

    /// <summary>
    /// Custom events sent for the bot, for example from the webchat.
    /// </summary>
    internal static class BotEvents
    {
        /// <summary>
        /// Event from the web chat to greet a new member.
        /// </summary>
        internal const string WebChatGreetings = @"webchat/greetings";
    }

    /// <summary>
    /// Intents supported by this bot, usually those that cannot be calculated.
    /// </summary>
    internal static class Intents
    {
        /// <summary>
        /// The name of the 'Greetings' intent, which is typically used to greet any new user when
        /// starting a conversation with the bot.
        /// </summary>
        internal const string GreetingsIntent = @"Greetings";

        /// <summary>
        /// The name of the 'Confused' intent, which is typically used to inform that the bot was
        /// unable to understand the user's message.
        /// </summary>
        internal const string ConfusedIntent = @"Confused";
    }

    /// <summary>
    /// Settings values, usually found in configurations like the <c>appsettings.json</c> file.
    /// </summary>
    internal static class Settings
    {
        /// <summary>
        /// The name of the <c>DefaultCulture</c> configuration parameter. It corresponds to the
        /// default culture (combination of ISO 639-1 for language and ISO 3166-2 for country) to use.
        /// </summary>
        /// <seealso href="https://en.wikipedia.org/wiki/ISO_639-1"/>
        /// <seealso href="https://en.wikipedia.org/wiki/ISO_3166-2"/>
        internal const string DefaultCulture = @"DefaultCulture";

        /// <summary>
        /// The name of the default intent prediction service.
        /// </summary>
        internal const string DefaultIntentPredictionServiceName = @"DefaultIntentPredictionServiceName";

        /// <summary>
        /// The name of the <c>KnowledgeBaseName</c> configuration parameter with the name of the
        /// Custom Question Answering (CQA) project to use.
        /// </summary>
        internal const string KnowledgeBaseName = @"KnowledgeBaseName";

        /// <summary>
        /// The name of the <c>KnowledgeDeploymentSlot</c> configuration parameter. The retrieved value
        /// should be either '<c>production</c>' or '<c>test</c>'.
        /// </summary>
        internal const string KnowledgeDeploymentSlot = @"KnowledgeDeploymentSlot";

        /// <summary>
        /// The name of the <c>KnowledgeKeyCredential</c> configuration parameter containing the
        /// key credential required to send requests to a Custom Question Answering (CQA) project.
        /// </summary>
        internal const string KnowledgeKeyCredential = @"KnowledgeKeyCredential";

        /// <summary>
        /// The name of the <c>KnowledgeUri</c> configuration parameter with the URI (usually an URL)
        /// to the Azure Cognitive Service for Language resource that has the Custom Question Answering (CQA)
        /// projects.
        /// </summary>
        internal const string KnowledgeUri = @"KnowledgeUri";

        /// <summary>
        /// The name of the <c>QuestionAnsweringConfidenceScoreThreshold</c> configuration parameter.
        /// It corresponds to the minimum threshold score for answers, value ranges from 0 to 1.
        /// </summary>
        internal const string QuestionAnsweringConfidenceScoreThreshold = @"QuestionAnsweringConfidenceScoreThreshold";

        /// <summary>
        /// The name of the <c>SupportedCultures</c> configuration parameter. It corresponds
        /// to languages supported by this bot that does not require any special treatment
        /// like language detection or text translation.
        /// </summary>
        internal const string SupportedCultures = @"SupportedCultures";

        /// <summary>
        /// The name of the <c>Verbose</c> configuration parameter. It corresponds
        /// to a flag indicating whether a service (like the <see cref="QuestionAnsweringDialog"> question answering dialog</see>)
        /// should generate traces or include additional verbose information.
        /// </summary>
        internal const string Verbose = @"Verbose";

        /// <summary>
        /// Specific settings values for memory cache.
        /// </summary>
        internal static class MemoryCache
        {
            /// <summary>
            /// The name of the <c>GeneralAbsoluteExpirationSeconds</c> configuration parameter.
            /// It corresponds to the absolute cache expiration time, relative to 'now' in seconds.
            /// </summary>
            internal const string GeneralAbsoluteExpirationSeconds = @"MemoryCache:GeneralAbsoluteExpirationSeconds";
        }
    }
}
