using qf4net;

namespace RunToCompletionHsm;

public class RtcSignals : QSignals
{
    public static readonly Signal Start = new("Start");
    public static readonly Signal Abort = new("Abort");
    public static readonly Signal Quit  = new("Quit");
} //RtcSignals
//namespace RunToCompletionHsm
