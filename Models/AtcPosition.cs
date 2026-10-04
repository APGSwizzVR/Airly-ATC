namespace AirlyATC.Models;

public sealed class AtcPosition
{
    public required string Id { get; init; }
    public required string AirportIcao { get; init; }
    public required string AirportName { get; init; }
    public required string PositionName { get; init; }
    public required string Frequency { get; init; }
    public string? HumanController { get; set; }
    public Queue<string> ControllerQueue { get; } = new();

    public bool IsHumanControlled => !string.IsNullOrWhiteSpace(HumanController);
    public string ControllerLabel => IsHumanControlled ? HumanController! : "AIRLY AI";
    public string QueueLabel => ControllerQueue.Count == 0 ? "No queue" : $"{ControllerQueue.Count} queued";
}
