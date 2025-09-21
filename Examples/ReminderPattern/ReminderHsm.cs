// -----------------------------------------------------------------------------
// Reminder Pattern Example for Rainer Hessmer's C# port of
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

namespace ReminderHsm;

/// <summary>
/// Reminder pattern example for Rainer Hessmer's C# port of HQSM. This code is adapted from
/// Samek's reminder pattern example in section 5.2.
///
/// </summary>
public sealed class Reminder : QHsmQ
{
    //communication with main form is via these events:
    public delegate void                ReminderDisplayHandler(object sender, ReminderDisplayEventArgs e);
    public event ReminderDisplayHandler DisplayState; //name of state
    public event ReminderDisplayHandler DisplayPoll;  //polling counter
    public event ReminderDisplayHandler DisplayProc;  //processing counter

    public  bool IsHandled { get; set; } //IsHandled

    /// <summary>
    /// Determines how often DataReady events are posted
    /// </summary>
    public int Frequency
    {
        set => frequency = value;
    }
    private int frequency = 3; //every Nth tick

    private int myPollCtr;
    private int myProcCtr;

    private readonly QState Polling;
    private readonly QState Processing;
    private readonly QState Idle;
    private readonly QState Busy;
    private readonly QState Final;

    private QState DoPolling(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            MainForm.Instance.SetTimer();
            OnDisplayState("Polling");
            return null;
        }

        if (qevent.QSignal == QSignals.Init)
        {
            InitializeState(Processing);
            return null;
        }

        if (qevent.QSignal == ReminderSignals.TimerTick)
        {
            OnDisplayPoll(++myPollCtr);
            if ((myPollCtr & frequency) == 0) //using Samek's C-style technique
            {
                Enqueue(new ReminderEvent(ReminderSignals.DataReady));
            }

            return null;
        }

        if (qevent.QSignal == ReminderSignals.Terminate)
        {
            TransitionTo(Final);
            return null;
        }

        if (qevent.QSignal == QSignals.Exit)
        {
            MainForm.Instance.KillTimer();
            return null;
        }

        if (qevent.QSignal >= (int)QSignals.UserSig)
        {
            IsHandled = false;
        }
        return TopState;
    }

    private QState DoProcessing(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("Processing");
            return null;
        }

        if (qevent.QSignal == QSignals.Init)
        {
            InitializeState(Idle);
            return null;
        }

        return Polling;
    }

    private QState DoIdle(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("Idle");
            return null;
        }

        if (qevent.QSignal == ReminderSignals.DataReady)
        {
            TransitionTo(Busy);
            return null;
        }

        return Processing;
    }

    private QState DoBusy(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("Busy");
            return null;
        }

        if (qevent.QSignal == ReminderSignals.TimerTick)
        {
            OnDisplayProc(++myProcCtr);
            if ((myPollCtr & 0x1) == 0) //using Samek's C-style technique
            {
                TransitionTo(Idle);
            }

            return null;
        }

        return Processing;
    }

    //UNDONE: revise this code
    private QState DoFinal(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("HSM terminated");
            singleton = null;
            MainForm.Instance.Close();
            System.Windows.Forms.Application.Exit();
            return null;
        }

        return TopState;
    }

    private void OnDisplayState(string stateInfo)
    {
        if (DisplayState != null)
        {
            DisplayState(this, new ReminderDisplayEventArgs(stateInfo));
        }
    } //OnDisplayState

    private void OnDisplayPoll(int pollCtr)
    {
        if (DisplayPoll != null)
        {
            DisplayPoll(this, new ReminderDisplayEventArgs(pollCtr.ToString()));
        }
    } //OnDisplayPoll

    private void OnDisplayProc(int procCtr)
    {
        if (DisplayProc != null)
        {
            DisplayProc(this, new ReminderDisplayEventArgs(procCtr.ToString()));
        }
    } //OnDisplayProc

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        InitializeState(Polling); // initial transition
    }

    private Reminder()
    {
        Polling    = DoPolling;
        Processing = DoProcessing;
        Idle       = DoIdle;
        Busy       = DoBusy;
        Final      = DoFinal;
    }

    //
    //Thread-safe implementation of singleton as a property
    //
    private static volatile Reminder singleton;
    private static readonly object   sync      = new(); //for static lock

    public static Reminder Instance
    {
        get
        {
            if (singleton == null)
            {
                lock (sync)
                {
                    if (singleton == null)
                    {
                        singleton = new Reminder();
                        singleton.Init();
                    }
                }
            }

            return singleton;
        }
    }
}

public class ReminderDisplayEventArgs : EventArgs
{
    public           string Message { get; }

    public ReminderDisplayEventArgs(string message)
    {
        Message = message;
    }
}
