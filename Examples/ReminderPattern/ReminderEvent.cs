using qf4net;

namespace ReminderHsm;

public class ReminderEvent : QEvent
{
    public ReminderEvent(Signal signal)
        : base(signal) { }
}                               //ReminderEvent
//namespace ReminderHsm
