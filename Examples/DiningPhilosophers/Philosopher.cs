using System;
using System.Diagnostics;
using System.Threading;
using qf4net;

namespace DiningPhilosophers;

/// <summary>
/// The active object that represents the table
/// </summary>
public class Philosopher : QActive
{
    private readonly TimeSpan _thinkTime = new(0, 0, 7); // last parameter represents seconds
    private readonly TimeSpan _eatTime   = new(0, 0, 5); // last parameter represents seconds

    private readonly QTimer _timer;
    private readonly int    _philosopherId;

    private readonly QState _stateThinking;
    private readonly QState _stateHungry;
    private readonly QState _stateEating;

    public Philosopher(int philosopherId)
    {
        _philosopherId = philosopherId;

        _stateThinking = Thinking;
        _stateHungry   = Hungry;
        _stateEating   = Eating;

        _timer = new QTimer(this);
    }

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        Thread.CurrentThread.Name = ToString();
        LogMessage($"Initializing philosopher {_philosopherId}");
        QfEventBroadcaster.Instance.Subscribe(this, DPPSignal.Eat);
        InitializeState(_stateThinking); // initial transition
    }

    private static TransitionChain s_Tran_Thinking_Hungry;

    private QState Thinking(IQEvent qEvent)
    {
        if (qEvent.IsSignal(QSignals.Entry))
        {
            LogMessage($"Philosopher {_philosopherId} is thinking.");
            _timer.FireIn(_thinkTime, new PhilosopherEvent(DPPSignal.Timeout));
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Timeout))
        {
            TransitionTo(_stateHungry, ref s_Tran_Thinking_Hungry);
            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage(
                       $"Philosopher {_philosopherId} is exiting thinking state."
                      );
            return null;
        }
        return TopState;
    }

    private static TransitionChain s_Tran_Hungry_Eating;

    private QState Hungry(IQEvent qEvent)
    {
        if (qEvent.IsSignal(QSignals.Entry))
        {
            LogMessage($"Philosopher {_philosopherId} is hungry.");
            var tableEvent = new TableEvent(DPPSignal.Hungry, _philosopherId);
            LogMessage(
                       $"Philosopher {_philosopherId} publishes Hungry event."
                      );
            QfEventBroadcaster.Instance.Publish(tableEvent);
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Eat))
        {
            if (((TableEvent)qEvent).PhilosopherId == _philosopherId)
            {
                LogMessage(
                           $"Philosopher {_philosopherId} receives eat signal."
                          );
                TransitionTo(_stateEating, ref s_Tran_Hungry_Eating);
            }
            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage(
                       $"Philosopher {_philosopherId} is exiting hungry state."
                      );
            return null;
        }
        return TopState;
    }

    private static TransitionChain s_Tran_Eating_Thinking;

    private QState Eating(IQEvent qEvent)
    {
        if (qEvent.IsSignal(QSignals.Entry))
        {
            LogMessage($"Philosopher {_philosopherId} is eating.");
            _timer.FireIn(_eatTime, new PhilosopherEvent(DPPSignal.Timeout));
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Timeout))
        {
            TransitionTo(_stateThinking, ref s_Tran_Eating_Thinking);
            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage(
                       $"Philosopher {_philosopherId} is exiting eating state."
                      );
            var tableEvent = new TableEvent(DPPSignal.Done, _philosopherId);
            LogMessage($"Philosopher {_philosopherId} publishes Done event.");
            QfEventBroadcaster.Instance.Publish(tableEvent);
            return null;
        }
        return TopState;
    }

    public override string ToString()
    {
        return "Philosopher " + _philosopherId;
    }

    private void LogMessage(string message)
    {
        Debug.WriteLine("\t" + Thread.CurrentThread.Name + ": " + message);
    }

    protected override void HsmUnhandledException(Exception e)
    {
        throw new NotImplementedException();
    }
}
