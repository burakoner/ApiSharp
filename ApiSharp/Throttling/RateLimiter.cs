namespace ApiSharp.Throttling;

/// <summary>
/// Limits the amount of requests to a certain constraint
/// </summary>
public class RateLimiter : IRateLimiter
{
    private readonly ConcurrentBag<Limiter> _limiters = new();

    /// <summary>
    /// Create a new RateLimiter. Configure the rate limiter by calling <see cref="AddTotalRateLimit"/>, 
    /// <see cref="AddEndpointLimit(string, int, TimeSpan, HttpMethod?, bool)"/>, <see cref="AddPartialEndpointLimit(string, int, TimeSpan, HttpMethod?, bool, bool)"/> or <see cref="AddApiKeyLimit"/>.
    /// </summary>
    public RateLimiter()
    {
    }

    /// <summary>
    /// Add a rate limit for the total amount of requests per time period
    /// </summary>
    /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
    /// <param name="period">The time period the limit is for</param>
    /// <param name="ignoreOtherRateLimits">If set to true it ignores other rate limits</param>
    public RateLimiter AddTotalRateLimit(int limit, TimeSpan period, bool ignoreOtherRateLimits = false)
    {
        _limiters.Add(new TotalRateLimiter(limit, period, null, ignoreOtherRateLimits));

        return this;
    }

    /// <summary>
    /// Add a rate lmit for the amount of requests per time for an endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint the limit is for</param>
    /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
    /// <param name="period">The time period the limit is for</param>
    /// <param name="method">The HttpMethod the limit is for, null for all</param>
    /// <param name="ignoreOtherRateLimits">If set to true it ignores other rate limits</param>
    public RateLimiter AddEndpointLimit(string endpoint, int limit, TimeSpan period, HttpMethod method = null, bool ignoreOtherRateLimits = false)
    {
        _limiters.Add(new EndpointRateLimiter(new[] { endpoint }, limit, period, method, ignoreOtherRateLimits));

        return this;
    }

    /// <summary>
    /// Add a rate lmit for the amount of requests per time for an endpoint
    /// </summary>
    /// <param name="endpoints">The endpoints the limit is for</param>
    /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
    /// <param name="period">The time period the limit is for</param>
    /// <param name="method">The HttpMethod the limit is for, null for all</param>
    /// <param name="ignoreOtherRateLimits">If set to true it ignores other rate limits</param>
    public RateLimiter AddEndpointLimit(IEnumerable<string> endpoints, int limit, TimeSpan period, HttpMethod method = null, bool ignoreOtherRateLimits = false)
    {
        _limiters.Add(new EndpointRateLimiter(endpoints.ToArray(), limit, period, method, ignoreOtherRateLimits));

        return this;
    }

    /// <summary>
    /// Add a rate lmit for the amount of requests per time for an endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint the limit is for</param>
    /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
    /// <param name="period">The time period the limit is for</param>
    /// <param name="method">The HttpMethod the limit is for, null for all</param>
    /// <param name="countPerEndpoint">Whether all requests for this partial endpoint are bound to the same limit or each individual endpoint has its own limit</param>
    /// <param name="ignoreOtherRateLimits">If set to true it ignores other rate limits</param>
    public RateLimiter AddPartialEndpointLimit(string endpoint, int limit, TimeSpan period, HttpMethod method = null, bool countPerEndpoint = false, bool ignoreOtherRateLimits = false)
    {
        _limiters.Add(new PartialEndpointRateLimiter(new[] { endpoint }, limit, period, countPerEndpoint, method, ignoreOtherRateLimits));

        return this;
    }

    /// <summary>
    /// Add a rate limit for the amount of requests per Api key
    /// </summary>
    /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
    /// <param name="period">The time period the limit is for</param>
    /// <param name="onlyForSignedRequests">Only include calls that are signed in this limiter</param>
    /// <param name="ignoreOtherRateLimits">If set to true it ignores other rate limits</param>
    public RateLimiter AddApiKeyLimit(int limit, TimeSpan period, bool onlyForSignedRequests, bool ignoreOtherRateLimits = false)
    {
        _limiters.Add(new ApiKeyRateLimiter(limit, period, onlyForSignedRequests, null, ignoreOtherRateLimits));

        return this;
    }

    public async Task<CallResult<int>> LimitRequestAsync(ILogger logger, string endpoint, HttpMethod method, bool signed, SensitiveString apikey, RateLimitingBehavior limitBehaviour, int requestWeight, CancellationToken ct)
    {
        var totalWaitTime = 0;

        if (_limiters.OfType<EndpointRateLimiter>().Any(x => x.IgnoreOtherRateLimits)) goto EndpointRateLimiter;
        if (_limiters.OfType<PartialEndpointRateLimiter>().Any(x => x.IgnoreOtherRateLimits)) goto PartialEndpointRateLimiter;
        if (_limiters.OfType<ApiKeyRateLimiter>().Any(x => x.IgnoreOtherRateLimits)) goto ApiKeyRateLimiter;
        if (_limiters.OfType<TotalRateLimiter>().Any(x => x.IgnoreOtherRateLimits)) goto TotalRateLimiter;

    EndpointRateLimiter:
        var endpointLimit = _limiters.OfType<EndpointRateLimiter>().SingleOrDefault(h => h.Endpoints.Contains(endpoint) && (h.Method == null || h.Method == method));
        if (endpointLimit != null)
        {
            var waitResult = await ProcessTopic(logger, endpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
            if (!waitResult) return waitResult;
            totalWaitTime += waitResult.Data;
        }
        if (endpointLimit?.IgnoreOtherRateLimits == true)
        {
            return new CallResult<int>(totalWaitTime);
        }

    PartialEndpointRateLimiter:
        var partialEndpointLimits = _limiters.OfType<PartialEndpointRateLimiter>().Where(h => h.PartialEndpoints.Any(h => endpoint.Contains(h)) && (h.Method == null || h.Method == method)).ToList();
        foreach (var partialEndpointLimit in partialEndpointLimits)
        {
            if (partialEndpointLimit.CountPerEndpoint)
            {
                var thisEndpointLimit = _limiters.OfType<SingleTopicRateLimiter>().SingleOrDefault(h => h.Type == RateLimiterType.PartialEndpoint && (string)h.Topic == endpoint);
                if (thisEndpointLimit == null)
                {
                    thisEndpointLimit = new SingleTopicRateLimiter(endpoint, partialEndpointLimit);
                    _limiters.Add(thisEndpointLimit);
                }

                var waitResult = await ProcessTopic(logger, thisEndpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                if (!waitResult) return waitResult;

                totalWaitTime += waitResult.Data;
            }
            else
            {
                var waitResult = await ProcessTopic(logger, partialEndpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                if (!waitResult) return waitResult;

                totalWaitTime += waitResult.Data;
            }
        }
        if (partialEndpointLimits.Any(p => p.IgnoreOtherRateLimits))
        {
            return new CallResult<int>(totalWaitTime);
        }

    ApiKeyRateLimiter:
        var apiLimit = _limiters.OfType<ApiKeyRateLimiter>().SingleOrDefault(h => h.Type == RateLimiterType.ApiKey);
        if (apiLimit != null)
        {
            if (apikey == null)
            {
                if (!apiLimit.OnlyForSignedRequests)
                {
                    var waitResult = await ProcessTopic(logger, apiLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                    if (!waitResult) return waitResult;

                    totalWaitTime += waitResult.Data;
                }
            }
            else if (signed || !apiLimit.OnlyForSignedRequests)
            {
                var thisApiLimit = _limiters.OfType<SingleTopicRateLimiter>().SingleOrDefault(h => h.Type == RateLimiterType.ApiKey && ((SensitiveString)h.Topic).IsEqualTo(apikey));
                if (thisApiLimit == null)
                {
                    thisApiLimit = new SingleTopicRateLimiter(apikey, apiLimit);
                    _limiters.Add(thisApiLimit);
                }

                var waitResult = await ProcessTopic(logger, thisApiLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                if (!waitResult) return waitResult;

                totalWaitTime += waitResult.Data;
            }
        }
        if ((signed || apiLimit?.OnlyForSignedRequests == false) && apiLimit?.IgnoreOtherRateLimits == true)
        {
            return new CallResult<int>(totalWaitTime);
        }

    TotalRateLimiter:
        var totalLimit = _limiters.OfType<TotalRateLimiter>().SingleOrDefault();
        if (totalLimit != null)
        {
            var waitResult = await ProcessTopic(logger, totalLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
            if (!waitResult) return waitResult;
            totalWaitTime += waitResult.Data;
        }
        if (totalLimit?.IgnoreOtherRateLimits == true)
        {
            return new CallResult<int>(totalWaitTime);
        }

        return new CallResult<int>(totalWaitTime);
    }

    private static async Task<CallResult<int>> ProcessTopic(ILogger logger, Limiter historyTopic, string endpoint, int requestWeight, RateLimitingBehavior limitBehaviour, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await historyTopic.Semaphore.WaitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return new CallResult<int>(new CancellationRequestedError());
        }
        sw.Stop();

        var totalWaitTime = 0;
        while (true)
        {
            // Remove requests no longer in time period from the history
            var checkTime = DateTime.UtcNow;
            for (var i = 0; i < historyTopic.Entries.Count; i++)
            {
                if (historyTopic.Entries[i].Timestamp < checkTime - historyTopic.Period)
                {
                    historyTopic.Entries.Remove(historyTopic.Entries[i]);
                    i--;
                }
                else break;
            }

            var currentWeight = !historyTopic.Entries.Any() ? 0 : historyTopic.Entries.Sum(h => h.Weight);
            if (currentWeight + requestWeight > historyTopic.Limit)
            {
                if (currentWeight == 0)
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {requestWeight}, Ratelimit: {historyTopic.Limit}");

                // Wait until the next entry should be removed from the history
                var thisWaitTime = (int)Math.Round((historyTopic.Entries.First().Timestamp - (checkTime - historyTopic.Period)).TotalMilliseconds);
                if (thisWaitTime > 0)
                {
                    if (limitBehaviour == RateLimitingBehavior.Fail)
                    {
                        historyTopic.Semaphore.Release();
                        var msg = $"Request to {endpoint} failed because of rate limit `{historyTopic.Type}`. Current weight: {currentWeight}/{historyTopic.Limit}, request weight: {requestWeight}";
                            logger.Log(LogLevel.Warning, msg);
                        return new CallResult<int>(new RateLimitError(msg));
                    }

                        logger.Log(LogLevel.Information, $"Request to {endpoint} waiting {thisWaitTime}ms for rate limit `{historyTopic.Type}`. Current weight: {currentWeight}/{historyTopic.Limit}, request weight: {requestWeight}");
                    try
                    {
                        await Task.Delay(thisWaitTime, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return new CallResult<int>(new CancellationRequestedError());
                    }
                    totalWaitTime += thisWaitTime;
                }
            }
            else
            {
                break;
            }
        }

        var newTime = DateTime.UtcNow;
        historyTopic.Entries.Add(new LimitEntry(newTime, requestWeight));
        historyTopic.Semaphore.Release();

        return new CallResult<int>(totalWaitTime);
    }

}