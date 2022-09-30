using System;
using System.Text.Json;
using Ngb.Geographic;
using Ngb.Geographic.Json;
using NUnit.Framework;

namespace Tests;

public class JsonTest {
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = {new GeoCoordinateJsonConverter()}
    };

    [SetUp]
    public void Setup() {
        // JsonOptions.Converters.Add(new GeoCoordinateJsonConverter());
    }

    [Test]
    [TestCase(106.709437, -6.329094, true)]
    [TestCase(106.709437, -6.329094, false)]
    public void SerializationTest(double lon, double lat, bool array) {
        GeoCoordinateJsonConverter.UseArrayRepresentation(array);
        var coord = new GeoCoordinate(lon, lat);
        var data = new TestRecord("Test", coord);
        var jsonText = JsonSerializer.Serialize(data, JsonOptions);
        Console.WriteLine(jsonText);
    }

    [Test]
    [TestCase(@"{""pointName"":""Test"",""coordinate"":{""longitude"":106.709437,""latitude"":-6.329094}}")]
    [TestCase(@"{""pointName"":""Test"",""coordinate"":[106.709437,-6.329094]}")]
    public void DeserializationTest(string rawJson) {
        var obj = JsonSerializer.Deserialize<TestRecord>(rawJson, JsonOptions);
        Console.WriteLine(obj);
    }
}

internal record TestRecord(string PointName, GeoCoordinate Coordinate);
