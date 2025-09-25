using qf4net;

namespace RunToCompletionHsm;

public class RtcEvent : QEvent
{
    public RtcEvent(QSignal signal)
        : base(signal) { }
}                          //RtcEvent
//namespace RunToCompletionHsm
