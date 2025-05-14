namespace ApiSharp.Throttling.Structs;

internal struct LimitEntry(DateTime timestamp, int weight)
{
    public DateTime Timestamp { get; set; } = timestamp;
    public int Weight { get; set; } = weight;
}
