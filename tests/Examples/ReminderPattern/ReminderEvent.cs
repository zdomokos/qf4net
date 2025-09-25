using qf4net;

namespace ReminderHsm;

public class ReminderEvent : QEvent
{
    public ReminderEvent(QSignal signal)
        : base(signal) { }
}                               //ReminderEvent
//namespace ReminderHsm
