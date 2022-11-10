using System.Diagnostics;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

using Microsoft.Extensions.Logging.ApplicationInsights;

using NetCoreConf2022.Bot;
using NetCoreConf2022.Bot.ActivityHandlers;
using NetCoreConf2022.Bot.Adapters;
using NetCoreConf2022.Bot.Dialogs;
using NetCoreConf2022.Bot.Infrastructure;

using IMiddleware = Microsoft.Bot.Builder.IMiddleware;

/**********************/
/* Load Configuration */
/**********************/

var programType = typeof(Program);

var applicationName = programType.Assembly.FullName;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    ApplicationName = applicationName,
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot"),
});

var isDevelopment = builder.Environment.IsDevelopment();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile($@"appsettings.{Environment.UserName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables()
                     ;

/*************************/
/* Logging Configuration */
/*************************/

if (isDevelopment)
{
    builder.Logging.AddConsole();
}

/***************************/
/* Debbuging Configuration */
/***************************/

if (Debugger.IsAttached)
{
    builder.Configuration.AddJsonFile(@"appsettings.debug.json", optional: true, reloadOnChange: true);

    builder.Logging.AddDebug();
}

var applicationInsightsConnectionString = builder.Configuration[@"ApplicationInsights:ConnectionString"];

builder.Logging.AddApplicationInsights((telemetryConfiguration) => telemetryConfiguration.ConnectionString = applicationInsightsConnectionString, (_) => { })
               .AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Trace)
               ;

/**************************/
/* Services Configuration */
/**************************/

// Add application services...
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration)
                .AddHttpClient()
                .AddHttpContextAccessor()
                .AddLocalization(options =>
                {
                    options.ResourcesPath = @"Resources";
                })
                .AddMemoryCache()
                .AddOptions()
                .AddRouting()
                ;

// Add MVC services...
builder.Services.AddControllers(options =>
                {
                    options.RequireHttpsPermanent = true;
                    options.SuppressAsyncSuffixInActionNames = true;
                })
                ;

/******************************/
/* Bot Services Configuration */
/******************************/

// Bot state storage...
builder.Services.AddSingleton<IStorage, MemoryStorage>()
                .AddSingleton<ConversationState>()
                .AddSingleton<UserState>()
                .AddSingleton<BotState, ConversationState>()
                .AddSingleton<BotState, UserState>()
                ;

// Bot telemetry...
builder.Services.AddSingleton((IBotTelemetryClient)new BotTelemetryClient(new TelemetryClient(new TelemetryConfiguration() { ConnectionString = applicationInsightsConnectionString })))
                .AddSingleton<OperationCorrelationTelemetryInitializer>()
                .AddSingleton<TelemetryBotIdInitializer>()
                ;

// Bot authentication and adapter...
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>() // Create the Bot Framework Authentication to be used with the Bot Adapter.
                .AddSingleton<IBotFrameworkHttpAdapter, CloudAdapterWithErrorHandler>() // Create the Bot Adapter with error handling enabled.
                .AddSingleton<CloudAdapterWithErrorHandlerOptions>()
                ;

// Bot middlewares, and their related and required services...
builder.Services.AddSingleton(serviceProvider => new AutoSaveStateMiddleware(serviceProvider.GetServices<BotState>().ToArray()))
                .AddSingleton<TelemetryLoggerMiddleware>()
                .AddSingleton<TelemetryInitializerMiddleware>()
                .AddSingleton<ShowTypingMiddleware>()
                .AddSingleton<TranscriptLoggerMiddleware>()
                .AddSingleton<IMiddleware, TelemetryLoggerMiddleware>()
                .AddSingleton<IMiddleware, TelemetryInitializerMiddleware>()
                .AddSingleton<IMiddleware, AutoSaveStateMiddleware>(serviceProvider => serviceProvider.GetRequiredService<AutoSaveStateMiddleware>())
                .AddSingleton<IMiddleware, ShowTypingMiddleware>()
                .AddSingleton<IMiddleware, TranscriptLoggerMiddleware>()
                .AddSingleton<ITranscriptLogger, MemoryTranscriptStore>()
                ;

// Add dialogs...
builder.Services.AddSingleton<GreetingsDialog>()
                .AddSingleton<Dialog, GreetingsDialog>()
                .AddSingleton<QuestionAnsweringDialog>()
                .AddSingleton<Dialog, QuestionAnsweringDialog>()
                ;

// Add bot handlers...
builder.Services.AddSingleton<IBot, BotActivityHandler>()
                .AddSingleton<BotActivityHandlerOptions>()
                ;

/*****************************************/
/* Infrastructure Services Configuration */
/*****************************************/
builder.Services.AddSingleton<IMetadataHandler, MetadataHandler>()
                ;

/****************************************/
/* Application Middleware Configuration */
/****************************************/

var app = builder.Build();

if (isDevelopment)
{
    app.UseDeveloperExceptionPage();
}

var supportedCultures = app.Configuration.GetSection(Constants.Settings.SupportedCultures).Get<string[]>() ?? Array.Empty<string>();

app.UseHttpsRedirection()
   .UseRequestLocalization(options =>
   {
       options.AddSupportedCultures(supportedCultures)
              .AddSupportedUICultures(supportedCultures)
              .SetDefaultCulture(supportedCultures.FirstOrDefault() ?? string.Empty)
              ;

       options.ApplyCurrentCultureToResponseHeaders = true;
   })
   .UseDefaultFiles()
   .UseStaticFiles()
   .UseRouting()
   .UseAuthentication()
   .UseAuthorization()
   .UseEndpoints(endpoints =>
   {
       endpoints.MapControllers();
   })
   ;

app.Run();