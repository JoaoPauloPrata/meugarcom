using MeuGarcom.Domain.Ai;
using MeuGarcom.Domain.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuGarcom.Infrastructure.Persistence.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Status).HasConversion(EnumText<ConversationStatus>.Converter);
        builder.HasMany(c => c.Messages).WithOne().HasForeignKey(m => m.ConversationId);
        builder.Navigation(c => c.Messages).HasField("_messages");
    }
}

internal sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Role).HasConversion(EnumText<MessageRole>.Converter);
        builder.Property(m => m.Outcome).HasConversion(EnumText<ReplyOutcome>.Converter);
        builder.Property(m => m.ComboJson).HasColumnName("combo").HasColumnType("jsonb");
        builder.Property(m => m.ReferencedItemIds).HasColumnType("uuid[]");
    }
}

internal sealed class GuardrailEventConfiguration : IEntityTypeConfiguration<GuardrailEvent>
{
    public void Configure(EntityTypeBuilder<GuardrailEvent> builder)
    {
        builder.ToTable("guardrail_events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Kind).HasConversion(EnumText<GuardrailEventKind>.Converter);
    }
}

internal sealed class AiModelSettingsConfiguration : IEntityTypeConfiguration<AiModelSettings>
{
    public void Configure(EntityTypeBuilder<AiModelSettings> builder)
    {
        builder.ToTable("ai_model_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.PrimaryProvider).HasConversion(EnumText<AiProvider>.Converter);
        builder.Property(s => s.FallbackProvider).HasConversion(EnumText<AiProvider>.Converter);
        builder.Property(s => s.ClassifierProvider).HasConversion(EnumText<AiProvider>.Converter);
        builder.Property(s => s.Temperature).HasPrecision(3, 2);
        builder.Ignore(s => s.Primary);
        builder.Ignore(s => s.Fallback);
        builder.Ignore(s => s.Classifier);
    }
}
