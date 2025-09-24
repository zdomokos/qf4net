using qf4net;

namespace DiningPhilosophersLegacy;

public class DPPSignal : QSignals
{
    public static readonly Signal Hungry  = new(nameof(Hungry));
    public static readonly Signal Done    = new(nameof(Done));
    public static readonly Signal Eat     = new(nameof(Eat));
    public static readonly Signal Timeout = new(nameof(Timeout));
};
