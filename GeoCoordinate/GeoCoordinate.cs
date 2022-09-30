using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ngb.Geographic {
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
        // private long _hash;

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
        }

        private GeoCoordinate(bool skipValidation, double longitude, double latitude) {
            Longitude = longitude;
            Latitude = latitude;
        }

        /// <summary>
        /// Creates new coordinate from longitude and latitude.
        /// </summary>
        /// <param name="longitude">longitude</param>
        /// <param name="latitude">latitude</param>
        /// <param name="calculateHash">If true, will calculate hash immediately</param>
        [Obsolete("Please use GeoCoordinate(double longitude, double latitude)")]
        public GeoCoordinate(double longitude, double latitude, bool calculateHash) {
            ValidateLongitude(longitude);
            ValidateLatitude(latitude);
            Longitude = longitude;
            Latitude = latitude;
            // _hash = calculateHash ? CalculateHash(longitude, latitude) : 0;
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
            // var diff = BasePrecision - precision;
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
            return new GeoCoordinate(true, lon, lat);
            // return new GeoCoordinate(lon, lat) {
            //     // _hash = diff > 0 ? hash << diff : hash
            // };
        }


        /// <summary>
        /// Get n-bit integer hash representation of coordinate.
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public long GetHash(int precision = Precision) {
            ValidatePrecision(precision);
            var diff = BasePrecision - precision;
            var hash = CalculateHash(Longitude, Latitude);
            return hash >> diff;
        }

        private static void ValidatePrecision(int precision, [CallerArgumentExpression("precision")] string arg = null!) {
            if (precision > BasePrecision || precision < 0) throw new ArgumentOutOfRangeException(arg);
        }

        private static void ValidateLatitude(double latitude, [CallerArgumentExpression("latitude")] string arg = null!) {
            if (latitude < South || latitude > North) throw new ArgumentOutOfRangeException(arg);
        }

        private static void ValidateLongitude(double longitude, [CallerArgumentExpression("longitude")] string arg = null!) {
            if (longitude < West || longitude > East) throw new ArgumentOutOfRangeException(arg);
        }

        /// <summary>
        /// Display coordinate representation in string.
        /// </summary>
        /// <returns></returns>
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

        private const string Base32Chars = "0123456789bcdefghjkmnpqrstuvwxyz"; //.ToArray();

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
        public string ToGeohash() => ToGeohash(DefaultGeoHashPrecision);

        public bool TryGeohash(Span<char> result, int hashPrecision, out int charsWritten) {
            charsWritten = 0;
            if (result.Length < hashPrecision) return false;
            var shift = BasePrecision - 5;
            var hash = CalculateHash(Longitude, Latitude);
            var charSpan = Base32Chars.AsSpan();
            for (int i = 0; i < hashPrecision; i++) {
                var value = (int) (shift >= 0 ? hash >> shift : hash << -shift);
                var idx = value & 0x1F;
                result[i] = charSpan[idx];
                shift -= 5;
                charsWritten++;
            }

            return true;
        }

        /// <summary>
        /// Converts coordinate to geo hash string.
        /// </summary>
        /// <param name="hashPrecision">Precision of geo hash result</param>
        /// <param name="arg"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">When <c>hashPrecision</c> is not between 1 and 12</exception>
        public string ToGeohash(int hashPrecision, [CallerArgumentExpression("hashPrecision")] string arg = null!) {
            if (hashPrecision < 1 || hashPrecision > 12) {
                throw new ArgumentOutOfRangeException(arg);
            }

            // if (_hash == 0) {
            //     _hash = CalculateHash(Longitude, Latitude);
            // }

            Span<char> result = stackalloc char[hashPrecision];

            return TryGeohash(result, hashPrecision, out _)
                ? result.ToString() //new string(result.ToArray())
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
        public static GeoCoordinate FromGeohash(ReadOnlySpan<char> hash) {
            var length = hash.Length;
            if (length == 0) throw new InvalidOperationException("Geohash contains 0 character");
            long longHash = 0;
            var shift = (BasePrecision - 5);
            var charSpan = Base32Chars.AsSpan();
            for (int i = 0; i < length; i++) {
                Debug.Assert(shift >= 0, "Negative bit shift");
                long value = charSpan.IndexOf(hash[i]); //.IndexOf(hash[i]);
                if (value < 0) throw new InvalidOperationException("Geohash contains invalid character");

                Debug.Assert(value <= 0x1F);
                longHash += value << shift;
                shift -= 5;
            }

            return FromHash(longHash, BasePrecision);
        }

        private const double EarthRadius = 6371000; // meter

        private static double ToRadians(double value) => value * Math.PI / 180;

        /// <summary>
        /// Calculate disntance between two points on earth.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Retunrs disntance in meter.</returns>
        public double DistanceTo(GeoCoordinate other) {
            var lon1 = ToRadians(Longitude);
            var lat1 = ToRadians(Latitude);
            var lon2 = ToRadians(other.Longitude);
            var lat2 = ToRadians(other.Latitude);

            var dlon = lon2 - lon1;
            var dlat = lat2 - lat1;

            var a = Math.Pow(Math.Sin(dlat / 2), 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);

            var c = 2 * Math.Asin(Math.Sqrt(a));
            // var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = EarthRadius * c;
            return d;
        }
    }
}
