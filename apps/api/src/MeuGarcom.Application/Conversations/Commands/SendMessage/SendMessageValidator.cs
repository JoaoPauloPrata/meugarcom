using FluentValidation;
using MeuGarcom.Application.Common.Messages;
using MeuGarcom.Domain.Conversations;

namespace MeuGarcom.Application.Conversations.Commands.SendMessage;

public sealed class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage(UserMessages.ForCode("conversation.not_found"));
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(UserMessages.ForCode("conversation.message_empty"))
            .MaximumLength(Conversation.MaxCustomerMessageLength).WithMessage(UserMessages.ForCode("conversation.message_too_long"));
    }
}
