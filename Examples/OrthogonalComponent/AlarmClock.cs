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
using System.Threading; //for timer
using qf4net;

namespace OrthogonalComponentHsm;

/// <summary>
/// Orthogonal Component pattern example for Rainer Hessmer's C# port of HQSM. This code is adapted from
/// Samek's Orthogonal Component pattern example in section 5.4. The compiler directive "USE_DOTNET_EVENTS"
/// includes code that uses Windows Events to send messages from the component (Alarm) to the container
/// (AlarmClock). Without this directive, the component adds events directly to the container's queue.
///
/// </summary>
public sealed class AlarmClock : QHsmQ //this base class provides an event queue
{
    //communication with main form is via these events:
    public delegate void             AlarmDisplayHandler(object sender, AlarmClockEventArgs e);
    public event AlarmDisplayHandler DisplayTimeOfDay;
    public event AlarmDisplayHandler DisplayAlarmTime;
    public event AlarmDisplayHandler DisplayAlarmAlert;

    private readonly TimerCallback          timerDelegate;
    private          Timer timer;

    public bool IsHandled { get; set; }

    private DateTime myCurrentTime;

    public DateTime AlarmTime
    {
        get => myAlarm.AlarmTime;
        set => myAlarm.AlarmTime = value;
    }

    private readonly Alarm  myAlarm; //state machine component (a singleton)
    private readonly string format12hr    = "hh:mm:ss tt";
    private readonly string format24hr    = "HH:mm:ss";
    private readonly int    tickFrequency = 100; //at 100, the clock runs 10x faster than real time

    private QState TimeKeeping { get; }
    private QState Mode12Hour  { get; }
    private QState Mode24Hour  { get; }
    private QState Final       { get; }

    private QState DoTimeKeeping(IQEvent qevent)
    {
        if (qevent.IsSignal(QSignals.Entry))
        {
            OnDisplayState("TimeKeeping");
            return null;
        }
        //				if (qevent.IsSignal(QSignals.Init)
        //					return null;
        //
        //using explicit start signal instead of init so it is easier to step through the code
        //in this example. These statement could be moved to the Init case if desired, and the
        //start button would not be needed on the GUI.
        if (qevent.IsSignal(AlarmClockSignals.Start))
        {
            // Create a timer that waits 0 seconds, then invokes every (tickFrequency/1000) second.
            timer         = new Timer(timerDelegate, this, 0, tickFrequency);
            myCurrentTime = DateTime.Now;
            OnDisplayAlarmTime(myCurrentTime.AddMinutes(1.0).ToString(format24hr));
            InitializeState(Mode24Hour);
            return null;
        }
        if (qevent.IsSignal(AlarmClockSignals.Mode12Hour))
        {
            TransitionTo(Mode12Hour);
            return null;
        }
        if (qevent.IsSignal(AlarmClockSignals.Mode24Hour))
        {
            TransitionTo(Mode24Hour);
            return null;
        }
        if (qevent.IsSignal(AlarmClockSignals.Alarm))
        {
            OnDisplayAlarmAlert();
            return null;
        }
        if (
                qevent.IsSignal(AlarmClockSignals.AlarmOn)
             || qevent.IsSignal(AlarmClockSignals.AlarmOff)
            )
        {
            myAlarm.Dispatch(qevent); // dispatch event to orthogonal component
            return null;
        }
        if (qevent.IsSignal(QSignals.Exit))
        {
            timer.Dispose();
            return null;
        }
        if (qevent.IsSignal(AlarmClockSignals.Terminate))
        {
            TransitionTo(Final);
            return null;
        }

        //			if (qevent.QSignal >= (int)QSignals.UserSig)
        //			{
        //				isHandled = false;
        //			}
        return TopState;
    }

    private QState DoMode24Hour(IQEvent qevent)
    {
        if (qevent.IsSignal(QSignals.Entry))
        {
            OnDisplayState("Mode24Hour");
            OnDisplayAlarmTime(myAlarm.AlarmTime.ToString(format24hr));
            return null;
        }
        if (qevent.IsSignal(AlarmClockSignals.Time))
        {
            myCurrentTime = myCurrentTime.AddSeconds(1.0);
            myAlarm.Dispatch(new TimeEvent(myCurrentTime, AlarmClockSignals.Time));
            OnDisplayTimeOfDay(myCurrentTime.ToString(format24hr));
            return null;
        }
        return TimeKeeping;
    }

    private QState DoMode12Hour(IQEvent qevent)
    {
        if (qevent.IsSignal(QSignals.Entry))
        {
            OnDisplayState("Mode12Hour");
            OnDisplayAlarmTime(myAlarm.AlarmTime.ToString(format12hr));
            return null;
        }
        if (qevent.IsSignal(AlarmClockSignals.Time))
        {
            myCurrentTime = myCurrentTime.AddSeconds(1.0);
            myAlarm.Dispatch(new TimeEvent(myCurrentTime, AlarmClockSignals.Time));
            OnDisplayTimeOfDay(myCurrentTime.ToString(format12hr));
            return null;
        }
        return TimeKeeping;
    }

    private QState DoFinal(IQEvent qevent)
    {
        if (qevent.IsSignal(QSignals.Entry))
        {
            OnDisplayState("HSM terminated");
            Dispose(false);
            System.Windows.Forms.Application.Exit();
            return null;
        }
        return TopState;
    }

    private static void GenerateTimerTickEvent(object target)
    {
        Instance.DispatchQ(new AlarmInitEvent(AlarmClockSignals.Time));
    }

#if USE_DOTNET_EVENTS
        private void AlarmActivated(object sender, AlarmClockEventArgs e)
        {
            //Posts message to self
            DispatchQ(new AlarmInitEvent(AlarmClockSignals.Alarm));
        }
#endif

    private void OnDisplayState(string s)
    {
        //not used in this example
    }

    private void OnDisplayTimeOfDay(string timeString)
    {
        if (DisplayTimeOfDay != null)
        {
            DisplayTimeOfDay(this, new AlarmClockEventArgs(timeString));
        }
    } //OnDisplayState

    private void OnDisplayAlarmTime(string timeString)
    {
        if (DisplayAlarmTime != null)
        {
            DisplayAlarmTime(this, new AlarmClockEventArgs(timeString));
        }
    } //OnDisplayPoll

    private void OnDisplayAlarmAlert()
    {
        //Microsoft.VisualBasic.Interaction.Beep();

        if (DisplayAlarmAlert != null)
        {
            DisplayAlarmAlert(this, null);
        }
    } //OnDisplayProc

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        InitializeState(TimeKeeping); // initial transition
    }

    private AlarmClock()
    {
        TimeKeeping = DoTimeKeeping;
        Mode12Hour  = DoMode12Hour;
        Mode24Hour  = DoMode24Hour;
        Final       = DoFinal;

        // Create the delegate that invokes methods for the timer.
        timerDelegate = GenerateTimerTickEvent;

        myAlarm = Alarm.Instance; // we could instead just use "Alarm.Instance" and eliminate "myAlarm"
#if USE_DOTNET_EVENTS
            myAlarm.AlarmActivated += new AlarmClock.AlarmDisplayHandler(AlarmActivated);
#endif
    }

    //
    //Thread-safe implementation of singleton as a property
    //
    private static volatile AlarmClock singleton;
    private static readonly object     sync      = new(); //for static lock

    public static AlarmClock Instance
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
                        singleton = new AlarmClock();
                        singleton.Init();
                    }
                }
            }

            return singleton;
        }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    private void Dispose(bool disposing)
    {
        timer.Dispose();
        singleton = null;
    }
}

public class AlarmClockEventArgs : EventArgs
{
    public           string Message { get; }

    public AlarmClockEventArgs(string message)
    {
        Message = message;
    }
}
