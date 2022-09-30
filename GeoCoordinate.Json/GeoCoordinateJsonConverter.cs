using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ngb.Geographic.Json;

public sealed class GeoCoordinateJsonConverter : JsonConverter<GeoCoordinate> {
    private static bool _arrayRepresentation = true;

    public GeoCoordinateJsonConverter(bool useArray = true) {
        _arrayRepresentation = useArray;
    }

    public static void UseArrayRepresentation(bool value) => _arrayRepresentation = value;

    public override GeoCoordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.StartArray => GetFromArray(ref reader),
            JsonTokenType.StartObject => GetFromObject(ref reader),
            _ => throw new InvalidOperationException("Unexpected JSON token")
        };
    }

    public override void Write(Utf8JsonWriter writer, GeoCoordinate value, JsonSerializerOptions options) {
        if (_arrayRepresentation) {
            WriteAsArray(writer, value);
        } else {
            WriteAsObject(writer, value, options);
        }
    }

    private static bool IsLongitude(string value)
        => string.Equals(value, "longitude", StringComparison.OrdinalIgnoreCase);

    private static bool IsLatitude(string value)
        => string.Equals(value, "latitude", StringComparison.OrdinalIgnoreCase);

    private static KeyValuePair<string, double> GetNextKeyValue(ref Utf8JsonReader reader) {
        var key = GetNextString(ref reader);
        if (key is null) throw new InvalidOperationException("Unexpected null key");
        var value = GetNextDouble(ref reader);
        return new KeyValuePair<string, double>(key, value);
    }

    private static GeoCoordinate GetFromObject(ref Utf8JsonReader reader) {
        reader.Read();
        double lon = 0.0;
        double lat = 0.0;

        while (reader.TokenType != JsonTokenType.EndObject) {
            (string key, double value) = GetNextKeyValue(ref reader);
            switch (key.Length) {
                case 8:
                    if (IsLatitude(key)) lat = value;
                    break;
                case 9:
                    if (IsLongitude(key)) lon = value;
                    break;
            }
        }

        return new GeoCoordinate(lon, lat);
    }

    private static GeoCoordinate GetFromArray(ref Utf8JsonReader reader) {
        reader.Read();
        var lon = GetNextDouble(ref reader);
        var lat = GetNextDouble(ref reader);
        SkipArray(ref reader);
        return new GeoCoordinate(lon, lat);
    }

    private static void SkipArray(ref Utf8JsonReader reader) {
        while (reader.TokenType != JsonTokenType.EndArray) reader.Read();
    }

    private static string? GetNextString(ref Utf8JsonReader reader) {
        var value = reader.GetString();
        reader.Read();
        return value;
    }

    private static double GetNextDouble(ref Utf8JsonReader reader) {
        var value = reader.TokenType switch {
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String => GetDoubleOrDefault(reader.GetString()),
            _ => default
            // throw new InvalidOperationException("Unexpected JSON token type while reading number value")
        };
        reader.Read();
        return value;
    }

    private static double GetDoubleOrDefault(string? value)
        => double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number) ? number : default;

    private static void WriteAsObject(Utf8JsonWriter writer, GeoCoordinate value, JsonSerializerOptions options) {
        (double lon, double lat) = value;
        var useCamelCase = options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase;
        writer.WriteStartObject();
        writer.WriteNumber(useCamelCase ? "longitude" : "Longitude", lon);
        writer.WriteNumber(useCamelCase ? "latitude" : "Latitude", lat);
        writer.WriteEndObject();
    }

    private static void WriteAsArray(Utf8JsonWriter writer, GeoCoordinate value) {
        writer.WriteStartArray();
        (double lon, double lat) = value;
        writer.WriteNumberValue(lon);
        writer.WriteNumberValue(lat);
        writer.WriteEndArray();
    }
}
