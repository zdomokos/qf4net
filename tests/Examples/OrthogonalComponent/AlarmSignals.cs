using qf4net;

namespace OrthogonalComponentHsm;

public static class AlarmClockSignals
{
    public static readonly QSignal Time       = new("Time");
    public static readonly QSignal Start      = new("Start");
    public static readonly QSignal Alarm      = new("Alarm");
    public static readonly QSignal AlarmOn    = new("AlarmOn");
    public static readonly QSignal AlarmOff   = new("AlarmOff");
    public static readonly QSignal Mode12Hour = new("Mode12Hour");
    public static readonly QSignal Mode24Hour = new("Mode24Hour");
    public static readonly QSignal Terminate  = new("Terminate");
} //AlarmClockSignals
//namespace OrthogonalComponentHsm
