using qf4net;

namespace RunToCompletionHsm;

public static class RtcSignals
{
    static RtcSignals()
    {
        // Static constructor ensures all signals are initialized before use
    }

    public static readonly Signal Start = new("Start");
    public static readonly Signal Abort = new("Abort");
    public static readonly Signal Quit  = new("Quit");
} //RtcSignals
//namespace RunToCompletionHsm
