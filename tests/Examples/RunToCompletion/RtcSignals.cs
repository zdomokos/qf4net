using qf4net;

namespace RunToCompletionHsm;

public static class RtcSignals
{
    static RtcSignals()
    {
        // Static constructor ensures all signals are initialized before use
    }

    public static readonly QSignal Start = new();
    public static readonly QSignal Abort = new();
    public static readonly QSignal Quit  = new();
} //RtcSignals
//namespace RunToCompletionHsm
