using System;
using Ngb.Geographic;
using NUnit.Framework;

namespace Tests;

public class Tests {
    [SetUp]
    public void Setup() {
        var aa = new Guid();
    }

    [Test]
    [TestCase(106.709437, -6.329094)]
    public void DeconstructionTest(double longitude, double latitude) {
        var coord = new GeoCoordinate(longitude, latitude);
        (double lon, double lat) = coord;
        Assert.IsTrue(AreEqual(lon, longitude, 1e-6));
        Assert.IsTrue(AreEqual(lat, latitude, 1e-6));

        Assert.Pass();
    }

    [Test]
    [TestCase(106.709437, -6.329094, 3195111357704980)]
    [TestCase(106.710205, -6.33012, 3195111357524970)]
    [TestCase(106.800254, 0.177515, 3950182899565084)]
    public void GeohashTest(double longitude, double latitude, long expected) {
        var coord = new GeoCoordinate(longitude, latitude);
        var hash = coord.GetHash();
        // var hash2 = coord.GetHash2();

        Console.WriteLine(hash);
        Assert.AreEqual(hash, expected);
        // Console.WriteLine(hash2);

        Assert.Pass();
    }

    [Test]
    [TestCase(106.709437, -6.329094, "qqggupz6q57")]
    [TestCase(106.710205, -6.33012, "qqggupxurup")]
    [TestCase(106.800254, 0.177515, "w25cn23d471")]
    public void GeoHashStringTest(double longitude, double latitude, string expected) {
        var coord = new GeoCoordinate(longitude, latitude);
        var hash = coord.ToGeohash();
        // var hash2 = coord.GetHash2();

        Console.WriteLine(hash);
        Assert.AreEqual(hash, expected);
        // Console.WriteLine(hash2);

        Assert.Pass();
    }

    [Test]
    [TestCase(106.709437, -6.329094, 3195111357704980)]
    [TestCase(106.710205, -6.33012, 3195111357524970)]
    [TestCase(106.800254, 0.177515, 3950182899565084)]
    public void FromHashTest(double longitude, double latitude, long hash) {
        var coord = GeoCoordinate.FromHash(hash);
        var (lon, lat) = coord;
        Assert.IsTrue(AreEqual(lon, longitude, 1e-5));
        Assert.IsTrue(AreEqual(lat, latitude, 1e-5));
        Console.WriteLine(coord);
        Assert.Pass();
    }

    [Test]
    [TestCase(106.709437, -6.329094, "qqggupz6q57")]
    [TestCase(106.710205, -6.33012, "qqggupxurup")]
    [TestCase(106.800254, 0.177515, "w25cn23d471")]
    public void FromGeohashTest(double longitude, double latitude, string hash) {
        var coord = GeoCoordinate.FromGeohash(hash);
        var (lon, lat) = coord;
        Assert.IsTrue(AreEqual(lon, longitude, 1e-5));
        Assert.IsTrue(AreEqual(lat, latitude, 1e-5));
        Console.WriteLine(coord);
        Assert.Pass();
    }

    public static bool AreEqual(double value1, double value2, double tolerance) {
        return Math.Abs(value1 - value2) < tolerance;
    }
}
