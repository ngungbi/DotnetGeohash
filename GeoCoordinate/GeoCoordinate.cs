﻿using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ngb.Geographic;

public struct GeoCoordinate {
    // private const int West = (int) (-180.0 * 1e7);
    // private const int East = (int) (180.0 * 1e7);
    // private const int North = (int) (90.0 * 1e7);
    // private const int South = (int) (-90.0 * 1e7);

    private const double West = -180.0;
    private const double East = 180.0;
    private const double North = 90.0;
    private const double South = -90.0;

    private const int Precision = 52;
    private const int BasePrecision = 60;

    public readonly double Longitude;
    public readonly double Latitude;
    private long _hash;

    /// <summary>
    /// Creates new coordinate from longitude and latitude.
    /// </summary>
    /// <param name="longitude">longitude</param>
    /// <param name="latitude">latitude</param>
    public GeoCoordinate(double longitude, double latitude) {
        ValidateLongitude(longitude);
        ValidateLatitude(latitude);
        Longitude = longitude;
        Latitude = latitude;
        _hash = 0;
    }

    /// <summary>
    /// Get 52-bit integer hash representation of coordinate.
    /// </summary>
    /// <param name="hash">Integer hash representation of coordinate</param>
    /// <param name="precision">Number of bit precision of hash</param>
    /// <returns></returns>
    public static GeoCoordinate FromHash(long hash, int precision = Precision) {
        ValidatePrecision(precision);
        var i = precision;
        var diff = BasePrecision - precision;
        var east = East;
        var west = West;
        var north = North;
        var south = South;
        while (i > 0) {
            Compare((int) (hash >> --i) & 1, ref west, ref east);
            Compare((int) (hash >> --i) & 1, ref south, ref north);
        }

        var lon = (east + west) / 2;
        var lat = (south + north) / 2;

        return new GeoCoordinate(lon, lat) {
            _hash = diff > 0 ? hash << diff : hash
        };
    }


    /// <summary>
    /// Get n-bit integer hash representation of coordinate.
    /// </summary>
    /// <param name="precision"></param>
    /// <returns></returns>
    public long GetHash(int precision = Precision) {
        ValidatePrecision(precision);
        var diff = BasePrecision - precision;
        if (_hash != 0) return _hash >> diff;
        _hash = CalculateHash(Longitude, Latitude);
        return _hash >> diff;
    }

    private static void ValidatePrecision(int precision, [CallerArgumentExpression("precision")] string? arg = null) {
        if (precision is > BasePrecision or < 0) throw new ArgumentOutOfRangeException(arg);
    }

    private static void ValidateLatitude(double latitude, [CallerArgumentExpression("latitude")] string? arg = null) {
        if (latitude is < South or > North) throw new ArgumentOutOfRangeException(arg);
    }

    private static void ValidateLongitude(double longitude, [CallerArgumentExpression("longitude")] string? arg = null) {
        if (longitude is < West or > East) throw new ArgumentOutOfRangeException(arg);
    }

    public override string ToString() {
        // if (Longitude == 0.0 && Latitude == 0.0) return "(0,0)";
        // char we = Longitude > 0 ? 'E' : 'W';
        // char ns = Latitude > 0 ? 'N' : 'S';
        return $"({Longitude:F7}, {Latitude:F7})";
    }

    public void Deconstruct(out double longitude, out double latitude) {
        longitude = Longitude;
        latitude = Latitude;
    }

    private static long CalculateHash(double longitude, double latitude) {
        var i = BasePrecision;
        long result = 0;

        // int lon = (int) (longitude * 1e7);
        // int lat = (int) (latitude * 1e7);

        var west = West;
        var east = East;
        var south = South;
        var north = North;

        while (i > 0) {
            result |= Search(longitude, ref west, ref east) << --i;
            result |= Search(latitude, ref south, ref north) << --i;
        }

        return result;
    }

    private static long Search(double value, ref double min, ref double max) {
        var mid = (min + max) / 2; // (min / 2 + max / 2); // + (min & max & 1);
        if (value > mid) {
            min = mid;
            return 1;
        } else {
            max = mid;
            return 0;
        }
    }

    private static void Compare(int value, ref double min, ref double max) {
        var mid = (min + max) / 2;
        switch (value) {
            case 1:
                min = mid;
                break;
            case 0:
                max = mid;
                break;
            default: throw new InvalidOperationException();
        }
    }

    private const string Base32Chars = "0123456789bcdefghjkmnpqrstuvwxyz";

    // private static long ToLongHash(ReadOnlySpan<char> hash) {
    //     long result = 0;
    //
    //     var count = hash.Length;
    //     foreach (char c in hash) {
    //         count--;
    //         long value = Base32Chars.IndexOf(c);
    //         result |= value << (count * 5);
    //     }
    //
    //     return result;
    // }

    private const int DefaultGeoHashPrecision = 11;

    /// <summary>
    /// Convert coordinate to geo hash string with default precision.
    /// </summary>
    /// <returns></returns>
    public string ToGeoHash() => ToGeoHash(DefaultGeoHashPrecision);

    public bool TryGeoHash(Span<char> result, int hashPrecision, out int charsWritten) {
        charsWritten = 0;
        if (result.Length < hashPrecision) return false;
        var shift = BasePrecision - 5;
        for (int i = 0; i < hashPrecision; i++) {
            var value = (int) (shift >= 0 ? _hash >> shift : _hash << -shift);
            var idx = value & 0x1F;
            result[i] = Base32Chars[idx];
            shift -= 5;
            charsWritten++;
        }

        return true;
    }

    /// <summary>
    /// Converts coordinate to geo hash string.
    /// </summary>
    /// <param name="hashPrecision">Precision of geo hash result</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">When <see cref="hashPrecision"/> is not between 1 and 12</exception>
    public string ToGeoHash(int hashPrecision) {
        if (hashPrecision is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(hashPrecision));
        if (_hash == 0) {
            _hash = CalculateHash(Longitude, Latitude);
        }

        Span<char> result = stackalloc char[hashPrecision];
        // TryGeoHash(result, hashPrecision, out var count);
        return TryGeoHash(result, hashPrecision, out _)
            ? new string(result)
            : string.Empty;

        // var result = new char[hashPrecision];
        //
        // var shift = BasePrecision - 5;
        // for (int i = 0; i < hashPrecision; i++) {
        //     var value = (int) (shift >= 0 ? _hash >> shift : _hash << -shift);
        //     var idx = value & 0x1F;
        //     result[i] = Base32Chars[idx];
        //     shift -= 5;
        // }
        //
        // return result;
    }

    /// <summary>
    /// Converts geo hash string to coordinate.
    /// </summary>
    /// <param name="hash">String representation of geo hash</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When hash length equals 0</exception>
    public static GeoCoordinate FromGeoHash(ReadOnlySpan<char> hash) {
        var length = hash.Length;
        if (length == 0) throw new InvalidOperationException();
        long longHash = 0;
        var shift = (BasePrecision - 5);
        for (int i = 0; i < length; i++) {
            Debug.Assert(shift >= 0, "Negative bit shift");
            long value = Base32Chars.IndexOf(hash[i]);

            Debug.Assert(value is >= 0 and <= 31);
            longHash += value << shift;
            shift -= 5;
        }

        return FromHash(longHash, BasePrecision);
    }
}