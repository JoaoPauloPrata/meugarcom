using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Ai;

namespace MeuGarcom.Infrastructure.Ai;

/// <summary>Fallback provider, through the official Anthropic SDK with structured outputs.</summary>
internal sealed class AnthropicWaiterAssistant : IAiProviderClient
{
    // Current Claude models think before answering and thinking tokens count against max_tokens,
    // so the ceiling must leave room above the visible answer size configured in the database.
    private const int MinMaxTokens = 4096;

    private readonly AnthropicClient? _client;

    public AnthropicWaiterAssistant(AiOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.AnthropicApiKey))
            _client = new AnthropicClient { ApiKey = options.AnthropicApiKey, MaxRetries = 0 };
    }

    public AiProvider Provider => AiProvider.Anthropic;

    public bool IsConfigured => _client is not null;

    public async Task<string> CompleteAsync(AiModelTarget target, ModelCallOptions call, AiCompletionRequest request, CancellationToken ct)
    {
        if (_client is null)
            throw new AiProviderException("Anthropic API key is not configured.");

        var system = new List<TextBlockParam>
        {
            new() { Text = request.Instructions, CacheControl = new CacheControlEphemeral() },
        };
        if (request.ContextData is not null)
            system.Add(new() { Text = request.ContextData });

        var schema = JsonDocument.Parse(request.JsonSchema).RootElement
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.Clone());

        var format = new JsonOutputFormat { Schema = schema };
        var outputConfig = ParseEffort(target.Effort) is { } effort
            ? new OutputConfig { Format = format, Effort = effort }
            : new OutputConfig { Format = format };

        var parameters = new MessageCreateParams
        {
            Model = target.Model,
            MaxTokens = Math.Max(call.MaxOutputTokens, MinMaxTokens),
            System = system,
            Messages = BuildMessages(request),
            OutputConfig = outputConfig,
        };

        var response = await _client.Messages.Create(parameters, ct);

        if (response.StopReason == "refusal")
            throw new AiProviderException("Anthropic refused the request.");
        if (response.StopReason == "max_tokens")
            throw new AiProviderException("Anthropic output was truncated (max tokens).");

        var text = string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        return string.IsNullOrWhiteSpace(text) ? throw new AiProviderException("Anthropic returned no text.") : text;
    }

    private static List<MessageParam> BuildMessages(AiCompletionRequest request)
    {
        // The Messages API requires the conversation to start with a user turn.
        var source = request.Messages.SkipWhile(m => m.Role != AiRole.User).ToList();
        var messages = new List<MessageParam>(source.Count);

        for (var i = 0; i < source.Count; i++)
        {
            var message = source[i];
            var isLast = i == source.Count - 1;

            if (message.Role == AiRole.Assistant)
                messages.Add(new AssistantMessageParam { Content = message.Content });
            else if (isLast && request.Attachment is not null)
                messages.Add(new UserMessageParam { Content = ContentWithAttachment(message.Content, request.Attachment) });
            else
                messages.Add(new UserMessageParam { Content = message.Content });
        }

        return messages;
    }

    private static List<ContentBlockParam> ContentWithAttachment(string text, AiAttachment attachment)
    {
        var data = Convert.ToBase64String(attachment.Content);
        var blocks = new List<ContentBlockParam>();

        if (AttachmentText.IsImage(attachment))
            blocks.Add(new ImageBlockParam { Source = new Base64ImageSource { Data = data, MediaType = attachment.MediaType } });
        else if (AttachmentText.IsPdf(attachment))
            blocks.Add(new DocumentBlockParam { Source = new Base64PdfSource { Data = data } });
        else
            blocks.Add(new TextBlockParam { Text = $"<file name=\"{attachment.FileName}\">\n{AttachmentText.ToText(attachment)}\n</file>" });

        blocks.Add(new TextBlockParam { Text = text });
        return blocks;
    }

    private static Effort? ParseEffort(string? effort) => effort?.Trim().ToLowerInvariant() switch
    {
        "low" => Effort.Low,
        "medium" => Effort.Medium,
        "high" => Effort.High,
        "max" => Effort.Max,
        _ => null,
    };
}
