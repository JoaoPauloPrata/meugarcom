using Mediator;
using MeuGarcom.Domain.Common;

namespace MeuGarcom.Application.Conversations.Commands.StartConversation;

/// <summary>Opens a new chat session. Called every time the customer opens the chat link.</summary>
public sealed record StartConversationCommand(string AccessCode) : ICommand<Result<ConversationStartedDto>>;

public sealed record ConversationStartedDto(Guid ConversationId, string RestaurantName, string UnitName, int? TableNumber);
