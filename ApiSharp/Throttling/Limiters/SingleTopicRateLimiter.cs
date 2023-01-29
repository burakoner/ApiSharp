namespace ApiSharp.Throttling.Limiters;

internal class SingleTopicRateLimiter : Limiter
{
    public object Topic { get; set; }

    public SingleTopicRateLimiter(object topic, Limiter limiter)
        : base(limiter.Type, limiter.Limit, limiter.Period, limiter.Method, limiter.IgnoreOtherRateLimits)
    {
        Topic = topic;
    }
}
