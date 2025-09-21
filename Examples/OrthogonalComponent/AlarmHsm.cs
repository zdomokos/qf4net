// -----------------------------------------------------------------------------
// Orthogonal Component Pattern Example for Rainer Hessmer's C# port of
// Samek's Quantum Hierarchical State Machine.
//
// Author: David Shields (david@shields.net)
//
// References:
// Practical Statecharts in C/C++; Quantum Programming for Embedded Systems
// Author: Miro Samek, Ph.D.
// http://www.quantum-leaps.com/book.htm
//
// Rainer Hessmer, Ph.D. (rainer@hessmer.org)
// http://www.hessmer.org/dev/qhsm/
// -----------------------------------------------------------------------------

using System;
using qf4net;

namespace OrthogonalComponentHsm
{
    /// <summary>
    /// Orthogonal Component pattern example for Rainer Hessmer's C# port of HQSM. This code is adapted from
    /// Samek's Orthogonal Component pattern example in section 5.4. The compiler directive "USE_DOTNET_EVENTS
    /// includes code that uses Windows Events to send messages from the component (Alarm) to the container
    /// (AlarmClock). Without this directive, the component adds events directly to the container's queue.
    ///
    /// </summary>
    public sealed class Alarm : QHsm //The C++ example uses an FSM. However, currently I only have a C# HSM available.
    {
#if USE_DOTNET_EVENTS
        //communication with AlarmClock is via these events:
        public event AlarmClock.AlarmDisplayHandler AlarmActivated;
#endif
        public DateTime AlarmTime
        {
            get { return myAlarmTime; }
            set { myAlarmTime = value; }
        } //AlarmTime
        private DateTime myAlarmTime;

        //We signal the alarm if the time comparison is within a fraction of a second
        private const long timeErrorMargin = (long)(TimeSpan.TicksPerSecond * 0.75);

        private QState On;
        private QState Off;

        private QState DoOn(IQEvent qevent)
        {
            if (qevent.IsSignal(AlarmClockSignals.Time))
            {
                TimeSpan t = (((TimeEvent)qevent).CurrentTime - myAlarmTime);
                if (Math.Abs(t.Ticks) < timeErrorMargin)
                {
                    OnAlarmActivated();
                }
                return null;
            }
            if (qevent.IsSignal(AlarmClockSignals.AlarmOff))
            {
                TransitionTo(Off);
                return null;
            }
            return this.TopState;
        }

        private QState DoOff(IQEvent qevent)
        {
            if (qevent.IsSignal(AlarmClockSignals.AlarmOn))
            {
                TransitionTo(On);
                return null;
            }
            return this.TopState;
        }

        private void OnAlarmActivated()
        {
#if USE_DOTNET_EVENTS
            if (AlarmActivated != null)
            {
                AlarmActivated(this, new AlarmClockEventArgs(string.Empty));
            }

#else
            AlarmClock.Instance.Enqueue(new AlarmInitEvent(AlarmClockSignals.Alarm));
#endif
        } //OnDisplayState

        /// <summary>
        /// Is called inside of the function Init to give the deriving class a chance to
        /// initialize the state machine.
        /// </summary>
        protected override void InitializeStateMachine()
        {
            InitializeState(Off); // initial transition
        } //init

        private Alarm()
        {
            On = new QState(this.DoOn);
            Off = new QState(this.DoOff);
        } //ctor

        //
        //Thread-safe implementation of singleton as a property
        //
        private static volatile Alarm singleton = null;
        private static object sync = new object(); //for static lock

        public static Alarm Instance
        {
            [System.Diagnostics.DebuggerStepThrough()]
            get
            {
                if (singleton == null)
                {
                    lock (sync)
                    {
                        if (singleton == null)
                        {
                            singleton = new Alarm();
                            singleton.Init();
                        }
                    }
                }

                return singleton;
            }
        } //Instance
    } //class Alarm
} //namespace OrthogonalComponentHsm
