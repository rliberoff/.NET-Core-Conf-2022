using System.Reflection;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace NetCoreConf2022.Bot.Dialogs;

internal sealed class GreetingsDialog : Dialog
{
    private readonly IMessageActivity greetingsMessageActvity;

    public GreetingsDialog()
    {
        greetingsMessageActvity = MessageFactory.Attachment(new HeroCard()
        {
            Images = new List<CardImage>()
            {
                new CardImage()
                {
                    Url = LoadGreetingsImageFromAssemblyEmbeddedResource(),
                },
            },
            Title = @"¡Bienvenidos a la NetCoreConf 2022!",
            Text = @"Recetas de La Abuela para bases de conocimiento en Azure Cognitive Services for Language",
        }.ToAttachment());
    }

    public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
    {
        var result = await dc.Context.SendActivityAsync(greetingsMessageActvity, cancellationToken);

        return await dc.EndDialogAsync(result, cancellationToken);
    }

    private static string LoadGreetingsImageFromAssemblyEmbeddedResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(@"NetCoreConf2022.Bot.Resources.Greetings.png");
        var count = (int)stream.Length;
        var data = new byte[count];
        stream.Read(data, 0, count);
        return $@"data:image/png;base64,{Convert.ToBase64String(data)}";
    }
}
