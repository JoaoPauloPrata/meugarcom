using MeuGarcom.Domain.Conversations;

namespace MeuGarcom.Domain.Tests;

public class ConversationTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public void Message_longer_than_the_limit_is_rejected()
    {
        var conversation = Conversation.Start(Guid.NewGuid(), null, Now);

        var result = conversation.ReceiveCustomerMessage(new string('a', Conversation.MaxCustomerMessageLength + 1), Now);

        Assert.Equal(ConversationErrors.MessageTooLong, result.Error);
    }

    [Fact]
    public void Conversation_closes_after_the_message_limit()
    {
        var conversation = Conversation.Start(Guid.NewGuid(), null, Now);
        for (var i = 0; i < Conversation.MaxCustomerMessages; i++)
            conversation.ReceiveCustomerMessage("oi", Now);

        var result = conversation.ReceiveCustomerMessage("mais uma", Now);

        Assert.Equal(ConversationErrors.MessageLimitReached, result.Error);
        Assert.Equal(ConversationStatus.Closed, conversation.Status);
    }

    [Fact]
    public void Conversation_expires_after_max_duration()
    {
        var conversation = Conversation.Start(Guid.NewGuid(), null, Now);

        var result = conversation.ReceiveCustomerMessage("oi", Now + Conversation.MaxDuration + TimeSpan.FromMinutes(1));

        Assert.Equal(ConversationErrors.Expired, result.Error);
    }

    [Fact]
    public void Language_is_set_only_by_the_first_message()
    {
        var conversation = Conversation.Start(Guid.NewGuid(), null, Now);

        conversation.DetectLanguage("en");
        conversation.DetectLanguage("es");

        Assert.Equal("en", conversation.Language);
    }

    [Fact]
    public void Missing_language_defaults_to_portuguese()
    {
        var conversation = Conversation.Start(Guid.NewGuid(), null, Now);

        conversation.DetectLanguage(null);

        Assert.Equal(Conversation.DefaultLanguage, conversation.ReplyLanguage);
    }

    [Fact]
    public void Third_abusive_message_closes_the_conversation()
    {
        var conversation = Conversation.Start(Guid.NewGuid(), null, Now);

        Assert.False(conversation.RegisterAbuse());
        Assert.False(conversation.RegisterAbuse());
        Assert.True(conversation.RegisterAbuse());
        Assert.Equal(ConversationStatus.Closed, conversation.Status);
    }
}
