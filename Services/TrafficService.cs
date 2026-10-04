using AirlyATC.Models;

namespace AirlyATC.Services;

public sealed class TrafficService
{
    private readonly Random _random = new(42);

    public List<AircraftTrack> Aircraft { get; } =
    [
        new() { Callsign = "RYR123", AircraftType = "B738", From = "EIDW", To = "EGLL", X = .30, Y = .40, Heading = 78, Altitude = 21000, GroundSpeed = 420, Squawk = "4210", Route = "NIMAT UL975 DVR" },
        new() { Callsign = "EIN456", AircraftType = "A320", From = "EGLL", To = "EIDW", X = .67, Y = .54, Heading = 258, Altitude = 17000, GroundSpeed = 390, Squawk = "2341", Route = "OCK UL975 NIMAT" },
        new() { Callsign = "BAW72", AircraftType = "A20N", From = "KJFK", To = "EGLL", X = .18, Y = .72, Heading = 65, Altitude = 35000, GroundSpeed = 455, Squawk = "1002", Route = "MERIT NATS" },
        new() { Callsign = "DAL401", AircraftType = "B763", From = "EGLL", To = "KJFK", X = .80, Y = .25, Heading = 280, Altitude = 33000, GroundSpeed = 470, Squawk = "6021", Route = "SUNOT NAT" },
        new() { Callsign = "SAS72", AircraftType = "A20N", From = "ESSA", To = "EIDW", X = .52, Y = .20, Heading = 205, Altitude = 24000, GroundSpeed = 410, Squawk = "4512", Route = "RUMAR DCT" }
    ];

    public void Tick()
    {
        foreach (var aircraft in Aircraft)
        {
            var distance = aircraft.GroundSpeed / 900000d;
            var radians = aircraft.Heading * Math.PI / 180d;
            aircraft.X += Math.Sin(radians) * distance;
            aircraft.Y -= Math.Cos(radians) * distance;

            if (aircraft.X < .04 || aircraft.X > .96)
                aircraft.Heading = 360 - aircraft.Heading;
            if (aircraft.Y < .04 || aircraft.Y > .96)
                aircraft.Heading = 180 - aircraft.Heading;

            aircraft.Altitude = Math.Max(500, aircraft.Altitude + _random.Next(-80, 81));
        }
    }
}
