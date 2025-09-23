using qf4net;

namespace OrthogonalComponentHsm;

public static class AlarmClockSignals
{
    public static readonly Signal Time       = new("Time");
    public static readonly Signal Start      = new("Start");
    public static readonly Signal Alarm      = new("Alarm");
    public static readonly Signal AlarmOn    = new("AlarmOn");
    public static readonly Signal AlarmOff   = new("AlarmOff");
    public static readonly Signal Mode12Hour = new("Mode12Hour");
    public static readonly Signal Mode24Hour = new("Mode24Hour");
    public static readonly Signal Terminate  = new("Terminate");
} //AlarmClockSignals
//namespace OrthogonalComponentHsm
