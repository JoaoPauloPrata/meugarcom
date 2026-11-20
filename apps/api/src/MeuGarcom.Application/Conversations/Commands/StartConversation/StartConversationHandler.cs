using Mediator;
using MeuGarcom.Application.Accounts.Queries.GetWorkspace;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Common;
using MeuGarcom.Domain.Conversations;

namespace MeuGarcom.Application.Conversations.Commands.StartConversation;

public sealed class StartConversationHandler(IAppDbContext db, TimeProvider clock)
    : ICommandHandler<StartConversationCommand, Result<ConversationStartedDto>>
{
    public async ValueTask<Result<ConversationStartedDto>> Handle(StartConversationCommand command, CancellationToken ct)
    {
        var access = await db.ResolveAsync(command.AccessCode, ct);
        if (access is null)
            return ConversationErrors.AccessCodeInvalid;

        var conversation = Conversation.Start(access.UnitId, access.TableId, clock.GetUtcNow());
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);

        return new ConversationStartedDto(
            conversation.Id,
            access.RestaurantName,
            access.UnitName,
            access.TableNumber,
            access.ChatSuggestions.Select(s => new ChatSuggestionDto(s.Text, s.Icon)).ToList());
    }
}
