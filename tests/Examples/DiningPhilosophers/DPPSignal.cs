using qf4net;

namespace DiningPhilosophers;

public class DPPSignal : QSignals
{
    public static readonly QSignal Hungry  = new();
    public static readonly QSignal Done    = new();
    public static readonly QSignal Eat     = new();
    public static readonly QSignal Timeout = new();
};
