namespace ApiSharp.Throttling.Structs;

internal struct LimitEntry
{
    public DateTime Timestamp { get; set; }
    public int Weight { get; set; }

    public LimitEntry(DateTime timestamp, int weight)
    {
        Timestamp = timestamp;
        Weight = weight;
    }
}
