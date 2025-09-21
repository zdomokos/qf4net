// -----------------------------------------------------------------------------
// Run to Completion Example for Rainer Hessmer's C# port of
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

namespace RunToCompletionHsm;

/// <summary>
/// Used instead of the delegate implementation shown in the other file
/// to keep track of the states in the event of an abort.
/// </summary>
public enum LastCompletedStep
{
    None,          //no steps run yet
    CantInterrupt, //step 0 (very first work to be done -- can't interrupt it)
    SlowOne,       //step 1 -- part 1 of long op that user may want to cancel
    SlowTwo,       //step 2 -- part 2 of long op that user may want to cancel
}

/// <summary>
/// RTC state machine example for Rainer Hessmer's C# port of HQSM
///
/// The purpose of this example is to answer the question:
/// "Is it possible to implement a run to completion statechart that
/// allows the user to abort long actions prior to their completion?"
///
/// In this example we assume the operations must be done in sequence, but the user
/// may want to stop temporarily to do other work and then resume this sequence
/// without going all the way back to the beginning.
///
/// This example should be compatible with static transitions, but that feature wasn't tested.
/// Several shortcuts are taken with regard to communication between threads and
/// the threading immplementation in general.
/// </summary>
public sealed class RunToCompletion : QHsm
{
    //communication with main form is via this event:
    public delegate void RtcDisplayHandler(object sender, RtcDisplayEventArgs e);

    public event RtcDisplayHandler DisplayState;

    public bool IsHandled { get; set; } //IsHandled

    private int _bigValue = 100000000;

    public int BigValue
    {
        set => _bigValue = value;
    }

    private LastCompletedStep _lastCompleted = LastCompletedStep.None;

    private readonly QState _dispatching;
    private readonly QState _cantInterrupt; //step 0 (very first work to be done -- can't interrupt it)
    private readonly QState _interruptible; //superstate for SlowOne & SlowTwo (doesn't include much functionality in this example)
    private readonly QState _slowOne;       //step 1 -- part 1 of long op that user may want to cancel
    private readonly QState _slowTwo;       //step 2 -- part 2 of long op that user may want to cancel
    private readonly QState _completed;     //all finished with parts 0, 1 and 2
    private          QState _final;

    private QState DoDispatching(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("Dispatching State");
            //lastCompleted = LastCompletedStep.None;
            return null;
        }

        if (qevent.QSignal == RtcSignals.Start)
        {
            switch (_lastCompleted)
            {
                case LastCompletedStep.None:
                case LastCompletedStep.SlowTwo:
                    TransitionTo(_cantInterrupt);
                    break;
                case LastCompletedStep.CantInterrupt:
                    TransitionTo(_slowOne);
                    break;
                case LastCompletedStep.SlowOne:
                    TransitionTo(_slowTwo);
                    break;
            }

            return null;
        }

        if (qevent.QSignal >= (int)QSignals.UserSig)
        {
            IsHandled = false;
        }

        return TopState;
    }

    private QState DoCantInterrupt(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("CantInterrupt");
            return null;
        }

        if (qevent.QSignal == RtcSignals.Start)
        {
            var completedOk = DoSomeUninterruptibleWork();
            if (completedOk)
            {
                _lastCompleted = LastCompletedStep.CantInterrupt;
            }

            TransitionTo(_slowOne);
            return null;
        }

        return TopState;
    }

    private QState DoSlowOne(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("SlowOne");
            return null;
        }

        if (qevent.QSignal == RtcSignals.Start)
        {
            var completedOk1 = DoSomeInterruptibleWork1();
            if (completedOk1)
            {
                _lastCompleted = LastCompletedStep.SlowOne;
                TransitionTo(_slowTwo);
            }
            else
            {
                TransitionTo(_dispatching);
            }

            return null;
        }

        return _interruptible;
    }

    private QState DoSlowTwo(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("SlowTwo");
            return null;
        }

        if (qevent.QSignal == RtcSignals.Start)
        {
            var completedOk2 = DoSomeInterruptibleWork2();
            if (completedOk2)
            {
                _lastCompleted = LastCompletedStep.SlowTwo; //not really needed
                TransitionTo(_completed);
            }
            else
            {
                TransitionTo(_dispatching);
            }

            return null;
        }

        return _interruptible;
    }

    private QState DoInterruptible(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("Interruptible");
            return null;
        }

        if (qevent.QSignal == RtcSignals.Abort)
        {
            SendAbortSignal();
            TransitionTo(_dispatching);
            return null;
        }

        return TopState;
    }

    private QState DoCompleted(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
        {
            OnDisplayState("Completed");
            _lastCompleted = LastCompletedStep.None;
            return null;
        }

        if (qevent.QSignal == RtcSignals.Start)
        {
            TransitionTo(_cantInterrupt);
            return null;
        }

        return TopState;
    }

    //UNDONE: revise this code
    private QState DoFinal(IQEvent qevent)
    {
        if (qevent.QSignal == QSignals.Entry)
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
            DisplayState(this, new RtcDisplayEventArgs(stateInfo));
        }
    } //OnDisplayState

    private bool DoSomeUninterruptibleWork()
    {
        // do lots of looping
        for (var i = 0; i < _bigValue; ++i)
        {
            double y = i + i;
        }

        return true;
    } //DoSomeUninterruptibleWork

    private bool DoSomeInterruptibleWork1()
    {
        var isAborted = Abort.Status;

        for (var i = 0; i < _bigValue; ++i)
        {
            if (Abort.Status)
            {
                isAborted = true;
                OnDisplayState("<<Aborted>>");
                break;
            }

            double y = i * i * i;
        }

        return !isAborted;
    } //DoSomeInterruptibleWork1

    private bool DoSomeInterruptibleWork2()
    {
        var isAborted = Abort.Status;

        for (var i = 0; i < _bigValue; ++i)
        {
            if (Abort.Status)
            {
                isAborted = true;
                OnDisplayState("<<Aborted>>");
                break;
            }

            double y = i * i * i;
        }

        return !isAborted;
    } //DoSomeInterruptibleWork2

    private void SendAbortSignal() //not used
    {
        Abort.Status = true;
    } //SendAbortSignal

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        InitializeState(_dispatching); // initial transition
    }

    private RunToCompletion()
    {
        _dispatching   = DoDispatching;
        _cantInterrupt = DoCantInterrupt;
        _interruptible = DoInterruptible;
        _slowOne       = DoSlowOne;
        _slowTwo       = DoSlowTwo;
        _completed     = DoCompleted;
        _final         = DoFinal;
    }

    //
    //Thread-safe implementation of singleton as a property
    //
    private static volatile RunToCompletion _singleton;
    private static readonly object          Sync = new(); //for static lock

    public static RunToCompletion Instance
    {
        get
        {
            if (_singleton == null)
            {
                lock (Sync)
                {
                    if (_singleton == null)
                    {
                        _singleton = new RunToCompletion();
                        _singleton.Init();
                    }
                }
            }

            return _singleton;
        }
    }
}

public class RtcDisplayEventArgs : EventArgs
{
    public string Message { get; }

    public RtcDisplayEventArgs(string message)
    {
        Message = message;
    }
}
