using qf4net;

namespace OptimizationBreaker;

public class MyQSignals : QSignals
{
    public static readonly Signal Sig1 = new(nameof(Sig1));
    public static readonly Signal Sig2 = new(nameof(Sig2));
}
