using AirlyATC.Models;

namespace AirlyATC.Services;

public sealed class AtcPositionService
{
    private readonly List<AtcPosition> _positions =
    [
        new AtcPosition
        {
            Id = "EIDW_DEL",
            AirportIcao = "EIDW",
            AirportName = "Dublin Airport",
            PositionName = "Delivery",
            Frequency = "121.800"
        },
        new AtcPosition
        {
            Id = "EIDW_GND",
            AirportIcao = "EIDW",
            AirportName = "Dublin Airport",
            PositionName = "Ground",
            Frequency = "121.600"
        },
        new AtcPosition
        {
            Id = "EIDW_TWR",
            AirportIcao = "EIDW",
            AirportName = "Dublin Airport",
            PositionName = "Tower",
            Frequency = "118.600"
        },
        new AtcPosition
        {
            Id = "EIDW_APP",
            AirportIcao = "EIDW",
            AirportName = "Dublin Airport",
            PositionName = "Approach",
            Frequency = "119.550"
        },
        new AtcPosition
        {
            Id = "EGLL_TWR",
            AirportIcao = "EGLL",
            AirportName = "London Heathrow",
            PositionName = "Tower",
            Frequency = "118.700"
        },
        new AtcPosition
        {
            Id = "KJFK_TWR",
            AirportIcao = "KJFK",
            AirportName = "John F. Kennedy International",
            PositionName = "Tower",
            Frequency = "119.100"
        }
    ];

    public IReadOnlyList<AtcPosition> GetAll() => _positions;

    public IReadOnlyList<AtcPosition> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _positions;

        query = query.Trim();

        return _positions
            .Where(position =>
                position.AirportIcao.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                position.AirportName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                position.PositionName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool TakePosition(AtcPosition position, string controller)
    {
        if (position.IsHumanControlled)
            return false;

        var queuedController = position.ControllerQueue
            .FirstOrDefault(x => string.Equals(x, controller, StringComparison.OrdinalIgnoreCase));

        if (queuedController is not null)
        {
            var remaining = position.ControllerQueue.Where(x => !string.Equals(x, controller, StringComparison.OrdinalIgnoreCase));
            position.ControllerQueue.Clear();

            foreach (var item in remaining)
                position.ControllerQueue.Enqueue(item);
        }

        position.HumanController = controller;
        return true;
    }

    public bool JoinQueue(AtcPosition position, string controller)
    {
        if (position.IsHumanControlled &&
            !position.ControllerQueue.Any(x => string.Equals(x, controller, StringComparison.OrdinalIgnoreCase)))
        {
            position.ControllerQueue.Enqueue(controller);
            return true;
        }

        return false;
    }

    public string? ReleasePosition(AtcPosition position)
    {
        position.HumanController = null;

        if (position.ControllerQueue.Count == 0)
            return null;

        var next = position.ControllerQueue.Dequeue();
        position.HumanController = next;
        return next;
    }
}
