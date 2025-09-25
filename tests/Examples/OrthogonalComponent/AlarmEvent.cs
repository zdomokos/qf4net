using System;
using qf4net;

namespace OrthogonalComponentHsm;

public class AlarmInitEvent : QEvent
{
    public AlarmInitEvent(QSignal signal)
        : base(signal) { }
}                          //AlarmInitEvent

public class TimeEvent : QEvent
{
    public DateTime CurrentTime { get; set; }

    public TimeEvent(DateTime currentTime, QSignal signal)
        : base(signal)
    {
        CurrentTime = currentTime;
    }
}     //TimeEvent
//namespace OrthogonalComponentHsm
