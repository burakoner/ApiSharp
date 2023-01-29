namespace ApiSharp.Stream;

public class StreamFactory
{
    public StreamClient CreateStreamClient(Log log, StreamParameters parameters)
    {
        return new StreamClient(log, parameters);
    }
}
