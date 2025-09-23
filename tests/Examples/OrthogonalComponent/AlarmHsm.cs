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

namespace OrthogonalComponentHsm;

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
    public  DateTime AlarmTime { get; set; } //AlarmTime

    //We signal the alarm if the time comparison is within a fraction of a second
    private const long TimeErrorMargin = (long)(TimeSpan.TicksPerSecond * 0.75);


    private QState DoOn(IQEvent qevent)
    {
        if (qevent.IsSignal(AlarmClockSignals.Time))
        {
            var t = (((TimeEvent)qevent).CurrentTime - AlarmTime);
            if (Math.Abs(t.Ticks) < TimeErrorMargin)
            {
                OnAlarmActivated();
            }
            return null;
        }
        if (qevent.IsSignal(AlarmClockSignals.AlarmOff))
        {
            TransitionTo(DoOff);
            return null;
        }
        return TopState;
    }

    private QState DoOff(IQEvent qevent)
    {
        if (qevent.IsSignal(AlarmClockSignals.AlarmOn))
        {
            TransitionTo(DoOn);
            return null;
        }
        return TopState;
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
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        InitializeState(DoOff); // initial transition
    }

    private Alarm()
    {
    }

    //
    //Thread-safe implementation of singleton as a property
    //
    private static volatile Alarm  _singleton;
    private static readonly object Sync      = new(); //for static lock

    public static Alarm Instance
    {
        [System.Diagnostics.DebuggerStepThrough()]
        get
        {
            if (_singleton == null)
            {
                lock (Sync)
                {
                    if (_singleton == null)
                    {
                        _singleton = new Alarm();
                        _singleton.Init();
                    }
                }
            }

            return _singleton;
        }
    }
}
