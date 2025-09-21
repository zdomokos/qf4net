using qf4net;

namespace RunToCompletionHsm;

public class RtcEvent : QEvent
{
    public RtcEvent(Signal signal)
        : base(signal) { }
}                          //RtcEvent
//namespace RunToCompletionHsm
