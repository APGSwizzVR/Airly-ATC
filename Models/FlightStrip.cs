namespace AirlyATC.Models;

public sealed class FlightStrip
{
    public required string Callsign { get; init; }
    public required string AircraftType { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Altitude { get; init; }
    public required string Squawk { get; init; }
    public required string Route { get; init; }
}
