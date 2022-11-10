using System.Globalization;

using System.Text;
using System.Text.RegularExpressions;

using Azure;
using Azure.Data.Tables;

using Microsoft.Extensions.Caching.Memory;

namespace NetCoreConf2022.Bot.Infrastructure;

internal sealed class MetadataHandler : IMetadataHandler
{
    private const string CacheKey = @"CacheKey_Metadata";
    private const string RegexFormat = @"(?:^|\W){0}(?:$|\W)|";

    private readonly IConfiguration configuration;
    private readonly IMemoryCache memoryCache;

    public MetadataHandler(IConfiguration configuration, IMemoryCache memoryCache)
    {
        this.configuration = configuration;
        this.memoryCache = memoryCache;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<KeyValuePair<string, string>>> HandleMessageAsync(string message, CancellationToken cancellationToken)
    {
        return (await memoryCache.GetOrCreateAsync(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(configuration.GetValue<double>(Constants.Settings.MemoryCache.GeneralAbsoluteExpirationSeconds, 86400));
            return InitAsync(cancellationToken);
        })).Where(i => i.Value.IsMatch(message)).Select(i => i.Key);
    }

    private static Regex BuildRegex(string terms, string termsSeparatorToken)
    {
        var patternStringBuilder = new StringBuilder();

        var splitedTerms = terms.Split(termsSeparatorToken, StringSplitOptions.RemoveEmptyEntries);

        foreach (var splitedTerm in splitedTerms)
        {
            patternStringBuilder.AppendFormat(CultureInfo.InvariantCulture, RegexFormat, splitedTerm);
        }

        patternStringBuilder.Length--; // Simplest and most efficient way to remove the trailing `|` from the ´RegexFormat´ constant...

        return new Regex(patternStringBuilder.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    private async Task<IDictionary<KeyValuePair<string, string>, Regex>> InitAsync(CancellationToken cancellationToken)
    {
        var tableClient = new TableClient(configuration.GetConnectionString(@"TableStorage"), @"Metadata");

        await tableClient.CreateIfNotExistsAsync(cancellationToken);

        return await tableClient.QueryAsync<MetadataTableEntity>(cancellationToken: cancellationToken)
                                .ToDictionaryAsync(e => KeyValuePair.Create(e.IsComposite ? $@"{e.Label}{e.CompositeToken}{e.Value}" : e.Label, e.Value),
                                                   e => BuildRegex(e.Terms, e.TermsSeparatorToken),
                                                   cancellationToken);
    }

    private sealed class MetadataTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public bool IsComposite { get; set; } = false;

        public string CompositeToken { get; set; } = string.Empty;

        public string Label => PartitionKey;

        public string Value => RowKey;

        public string Terms { get; set; } = string.Empty;

        public string TermsSeparatorToken { get; set; } = string.Empty;
    }
}
