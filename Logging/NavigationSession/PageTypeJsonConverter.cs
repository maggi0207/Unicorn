using System.Text.Json;
using System.Text.Json.Serialization;

namespace UI.EmployerPortal.Web.Logging.NavigationSession;

/// <summary>
/// Serializes <see cref="PageType"/> as its numeric <see cref="PageType.Value"/> and resolves it back
/// to the canonical static instance on read. The smart-enum has no public parameterless or parameterized
/// constructor, so default System.Text.Json deserialization throws <see cref="System.NotSupportedException"/>;
/// this keeps it round-trippable through session storage.
/// </summary>
internal sealed class PageTypeJsonConverter : JsonConverter<PageType>
{
    /// <inheritdoc />
    public override PageType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return PageType.FromValue(reader.GetInt32());
        }

        // Tolerate the legacy object shape ({ "Value": n, "Name": "..." }) that sessions stored before
        // this converter existed, so stale data resolves instead of throwing.
        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty("Value", out var value)
                ? PageType.FromValue(value.GetInt32())
                : null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PageType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
