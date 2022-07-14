# Coordinate and Geohash Converter for .NET

## Usage
Create a new coordinate from longitude and latitude
```c#
var coord = new GeoCoordinate(106.709437, -6.329094);
```
Get integer representation of geohash with default precision (52 bit)
```c#
long hash = coord.GetHash();
```
Get geohash from coordinate with default precision (11 characters)
```c#
string geohash = coord.ToGeohash();
```
Create a new coordinate from hash
```c#
var coord = GeoCoordinate.FromHash(3195111357704980);
```
Create a new coordinate from geohash with any precision between 1 and 12
```c#
var coord = GeoCoordinate.FromGeohash("qqggupz6q5");
```
Use tuple deconstruction method to get longitude and latitude
```c#
var (lon, lat) = coord;
```
Calculate distance between two coordinates
```c#
var coord1 = new GeoCoordinate(106.709437, -6.329094);
var coord2 = new GeoCoordinate(106.723845, -6.328453);
double distance = coord1.DistanceTo(coord2);
```
