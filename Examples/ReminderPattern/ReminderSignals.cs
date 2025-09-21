using qf4net;

namespace ReminderHsm;

public class ReminderSignals : QSignals
{
    public static readonly Signal DataReady = new(nameof(DataReady));
    public static readonly Signal TimerTick = new(nameof(TimerTick));

}
