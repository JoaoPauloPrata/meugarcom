using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Domain.Ai;

namespace MeuGarcom.Infrastructure.Ai;

/// <summary>Main provider. Chat Completions with Structured Outputs (strict JSON schema).</summary>
internal sealed class OpenAiWaiterAssistant(HttpClient http, AiOptions options) : IAiProviderClient
{
    public AiProvider Provider => AiProvider.OpenAi;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.OpenAiApiKey);

    public async Task<string> CompleteAsync(AiModelTarget target, ModelCallOptions call, AiCompletionRequest request, CancellationToken ct)
    {
        var messages = new JsonArray { new JsonObject { ["role"] = "system", ["content"] = request.Instructions } };
        if (request.ContextData is not null)
            messages.Add(new JsonObject { ["role"] = "system", ["content"] = request.ContextData });

        for (var i = 0; i < request.Messages.Count; i++)
        {
            var turn = request.Messages[i];
            var isLast = i == request.Messages.Count - 1;
            messages.Add(new JsonObject
            {
                ["role"] = turn.Role == AiRole.User ? "user" : "assistant",
                ["content"] = isLast && request.Attachment is not null
                    ? ContentWithAttachment(turn.Content, request.Attachment)
                    : turn.Content,
            });
        }

        var body = new JsonObject
        {
            ["model"] = target.Model,
            ["messages"] = messages,
            ["max_completion_tokens"] = call.MaxOutputTokens,
            ["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["name"] = request.SchemaName,
                    ["strict"] = true,
                    ["schema"] = JsonNode.Parse(request.JsonSchema),
                },
            },
        };

        if (!string.IsNullOrWhiteSpace(target.Effort))
            body["reasoning_effort"] = target.Effort;

        // Reasoning models only accept sampling parameters with reasoning disabled.
        if (call.Temperature is { } temperature && (target.Effort is null || target.Effort == "none"))
            body["temperature"] = (double)temperature;

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = JsonContent.Create(body),
        };
        httpRequest.Headers.Authorization = new("Bearer", options.OpenAiApiKey);

        using var response = await http.SendAsync(httpRequest, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new AiProviderException($"OpenAI returned {(int)response.StatusCode}: {Truncate(payload)}");

        using var document = JsonDocument.Parse(payload);
        var choice = document.RootElement.GetProperty("choices")[0];
        var message = choice.GetProperty("message");

        if (message.TryGetProperty("refusal", out var refusal) && refusal.ValueKind == JsonValueKind.String)
            throw new AiProviderException("OpenAI refused the request.");
        if (choice.GetProperty("finish_reason").GetString() == "length")
            throw new AiProviderException("OpenAI output was truncated (max tokens).");

        return message.GetProperty("content").GetString()
            ?? throw new AiProviderException("OpenAI returned an empty message.");
    }

    private static JsonArray ContentWithAttachment(string text, AiAttachment attachment)
    {
        var content = new JsonArray { new JsonObject { ["type"] = "text", ["text"] = text } };
        var dataUrl = $"data:{attachment.MediaType};base64,{Convert.ToBase64String(attachment.Content)}";

        if (AttachmentText.IsImage(attachment))
        {
            content.Add(new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JsonObject { ["url"] = dataUrl, ["detail"] = "high" },
            });
        }
        else if (AttachmentText.IsPdf(attachment))
        {
            content.Add(new JsonObject
            {
                ["type"] = "file",
                ["file"] = new JsonObject { ["filename"] = attachment.FileName, ["file_data"] = dataUrl },
            });
        }
        else
        {
            content.Add(new JsonObject
            {
                ["type"] = "text",
                ["text"] = $"<file name=\"{attachment.FileName}\">\n{AttachmentText.ToText(attachment)}\n</file>",
            });
        }

        return content;
    }

    private static string Truncate(string value) => value.Length > 500 ? value[..500] : value;
}
