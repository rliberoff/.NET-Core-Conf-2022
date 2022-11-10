namespace NetCoreConf2022.Bot.Infrastructure;

internal interface IMetadataHandler
{
    Task<IEnumerable<KeyValuePair<string, string>>> HandleMessageAsync(string message, CancellationToken cancellationToken);
}
