using System.Text.Json;

namespace Ngb.Geographic.Json;

public static class JsonSerializerExtension {
    public static void AddGeoCoordinateJsonConverter(this JsonSerializerOptions jsonOptions, bool useArray = true)
        => jsonOptions.Converters.Add(new GeoCoordinateJsonConverter(useArray));
}
