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
        set => _frequency = value;
    }

    private QState DoPolling(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            MainForm.Instance.SetTimer();
            OnDisplayState("DoPolling");
            return null;
        }

        if (qevent.Signal == QSignals.Init)
        {
            InitializeState(DoProcessing);
            return null;
        }

        if (qevent.Signal == ReminderSignals.TimerTick)
        {
            OnDisplayPoll(++_myPollCtr);
            if ((_myPollCtr & _frequency) == 0) //using Samek's C-style technique
            {
                Enqueue(new ReminderEvent(ReminderSignals.DataReady));
            }

            return null;
        }

        if (qevent.Signal == QSignals.Terminate)
        {
            TransitionTo(DoFinal);
            return null;
        }

        if (qevent.Signal == QSignals.Exit)
        {
            MainForm.Instance.KillTimer();
            return null;
        }

        // if (qevent.Signal >= (int)QSignals.UserSig)
        // {
        //     IsHandled = false;
        // }
        return TopState;
    }

    private QState DoProcessing(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("DoProcessing");
            return null;
        }

        if (qevent.Signal == QSignals.Init)
        {
            InitializeState(DoIdle);
            return null;
        }

        return DoPolling;
    }

    private QState DoIdle(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("DoIdle");
            return null;
        }

        if (qevent.Signal == ReminderSignals.DataReady)
        {
            TransitionTo(DoBusy);
            return null;
        }

        return DoProcessing;
    }

    private QState DoBusy(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("DoBusy");
            return null;
        }

        if (qevent.Signal == ReminderSignals.TimerTick)
        {
            OnDisplayProc(++_myProcCtr);
            if ((_myPollCtr & 0x1) == 0) //using Samek's C-style technique
            {
                TransitionTo(DoIdle);
            }

            return null;
        }

        return DoProcessing;
    }

    //UNDONE: revise this code
    private QState DoFinal(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("HSM terminated");
            _singleton = null;
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
        InitializeState(DoPolling); // initial transition
    }

    private Reminder()
    {
    }

    //
    //Thread-safe implementation of singleton as a property
    //
    private static volatile Reminder _singleton;
    private static readonly object   Sync      = new(); //for static lock

    public static Reminder Instance
    {
        get
        {
            if (_singleton == null)
            {
                lock (Sync)
                {
                    if (_singleton == null)
                    {
                        _singleton = new Reminder();
                        _singleton.Init();
                    }
                }
            }

            return _singleton;
        }
    }

    private int _frequency = 3; //every Nth tick
    private int _myPollCtr;
    private int _myProcCtr;
}

public class ReminderDisplayEventArgs : EventArgs
{
    public           string Message { get; }

    public ReminderDisplayEventArgs(string message)
    {
        Message = message;
    }
}
