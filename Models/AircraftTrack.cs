namespace AirlyATC.Models;

public sealed class AircraftTrack
{
    public required string Callsign { get; init; }
    public required string AircraftType { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Heading { get; set; }
    public int Altitude { get; set; }
    public int GroundSpeed { get; set; }
    public string Squawk { get; set; } = "7000";
    public string Route { get; set; } = "DIRECT";
}
