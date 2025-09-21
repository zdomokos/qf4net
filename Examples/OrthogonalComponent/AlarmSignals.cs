using System;
using qf4net;

namespace OrthogonalComponentHsm
{
    public class AlarmClockSignals
    {
        public static readonly Signal Time = new Signal("Time");
        public static readonly Signal Start = new Signal("Start");
        public static readonly Signal Alarm = new Signal("Alarm");
        public static readonly Signal AlarmOn = new Signal("AlarmOn");
        public static readonly Signal AlarmOff = new Signal("AlarmOff");
        public static readonly Signal Mode12Hour = new Signal("Mode12Hour");
        public static readonly Signal Mode24Hour = new Signal("Mode24Hour");
        public static readonly Signal Terminate = new Signal("Terminate");
    } //AlarmClockSignals
} //namespace OrthogonalComponentHsm
