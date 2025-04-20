namespace ApiSharp.Throttling.Limiters;

internal class SingleTopicRateLimiter(object topic, Limiter limiter) : Limiter(limiter.Type, limiter.Limit, limiter.Period, limiter.Method, limiter.IgnoreOtherRateLimits)
{
    public object Topic { get; set; } = topic;
}
