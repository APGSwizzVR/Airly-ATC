using AirlyATC.Models;

namespace AirlyATC.Services;

public sealed class AtcPositionService
{
    private readonly List<AtcPosition> _positions =
    [
        new() { Id = "EIDW_DEL", AirportIcao = "EIDW", AirportName = "Dublin Airport", PositionName = "DEL", Frequency = "121.800" },
        new() { Id = "EIDW_GND", AirportIcao = "EIDW", AirportName = "Dublin Airport", PositionName = "GND", Frequency = "121.600" },
        new() { Id = "EIDW_TWR", AirportIcao = "EIDW", AirportName = "Dublin Airport", PositionName = "TWR", Frequency = "118.600" },
        new() { Id = "EIDW_APP", AirportIcao = "EIDW", AirportName = "Dublin Airport", PositionName = "APP", Frequency = "119.550" },
        new() { Id = "EGLL_TWR", AirportIcao = "EGLL", AirportName = "London Heathrow", PositionName = "TWR", Frequency = "118.700" },
        new() { Id = "EGLL_APP", AirportIcao = "EGLL", AirportName = "London Heathrow", PositionName = "APP", Frequency = "119.725" },
        new() { Id = "KJFK_TWR", AirportIcao = "KJFK", AirportName = "John F. Kennedy International", PositionName = "TWR", Frequency = "119.100" },
        new() { Id = "KJFK_APP", AirportIcao = "KJFK", AirportName = "John F. Kennedy International", PositionName = "APP", Frequency = "125.700" }
    ];

    public IReadOnlyList<AtcPosition> GetAll() => _positions;

    public IReadOnlyList<AtcPosition> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _positions;

        query = query.Trim();

        return _positions.Where(p =>
            p.AirportIcao.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.AirportName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.PositionName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Frequency.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public bool TakePosition(AtcPosition position, string controller)
    {
        if (position.IsHumanControlled)
            return false;

        var remaining = position.ControllerQueue
            .Where(x => !string.Equals(x, controller, StringComparison.OrdinalIgnoreCase))
            .ToList();

        position.ControllerQueue.Clear();
        foreach (var item in remaining)
            position.ControllerQueue.Enqueue(item);

        position.HumanController = controller;
        return true;
    }

    public bool JoinQueue(AtcPosition position, string controller)
    {
        if (!position.IsHumanControlled ||
            position.ControllerQueue.Any(x => string.Equals(x, controller, StringComparison.OrdinalIgnoreCase)))
            return false;

        position.ControllerQueue.Enqueue(controller);
        return true;
    }

    public string? ReleasePosition(AtcPosition position)
    {
        position.HumanController = null;

        if (position.ControllerQueue.Count == 0)
            return null;

        position.HumanController = position.ControllerQueue.Dequeue();
        return position.HumanController;
    }
}
