using qf4net;

namespace OptimizationBreaker;

public static class MyQSignals
{
    public static readonly QSignal Sig1 = new(nameof(Sig1));
    public static readonly QSignal Sig2 = new(nameof(Sig2));
}
