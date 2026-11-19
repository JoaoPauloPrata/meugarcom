using Mediator;
using MeuGarcom.Api.Http;
using MeuGarcom.Application.Conversations.Commands.SendMessage;
using MeuGarcom.Application.Conversations.Commands.StartConversation;
using MeuGarcom.Application.Conversations.Queries.GetPublicMenu;

namespace MeuGarcom.Api.Endpoints;

/// <summary>Customer chat at the table (QR code / NFC). No login.</summary>
public sealed class PublicChatEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public").WithTags("Public chat").AllowAnonymous();

        group.MapPost("/conversations", async (StartRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new StartConversationCommand(body.AccessCode), ct)).ToHttp());

        group.MapPost("/conversations/{conversationId:guid}/messages", async (Guid conversationId, MessageRequest body, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SendMessageCommand(conversationId, body.Content), ct)).ToHttp());

        group.MapGet("/menus/{accessCode}", async (string accessCode, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetPublicMenuQuery(accessCode), ct)).ToHttp());
    }

    private sealed record StartRequest(string AccessCode);

    private sealed record MessageRequest(string Content);
}
