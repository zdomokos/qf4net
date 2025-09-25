using qf4net;

namespace ReminderHsm;

public static class ReminderSignals
{
    static ReminderSignals()
    {
        // Static constructor ensures all signals are initialized before use
    }

    public static readonly QSignal DataReady = new(nameof(DataReady));
    public static readonly QSignal TimerTick = new(nameof(TimerTick));
}
