using System;
using System.Threading;
using qf4net;

namespace DiningPhilosophers;

/// <summary>
/// The active object that represents the table
/// </summary>
public class Philosopher : QActive
{
    public Philosopher(IQEventBroker eventBroker, int philosopherId) : base(eventBroker)
    {
        _philosopherId = philosopherId;
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

        Subscribe(this, DPPSignal.Eat);
        InitializeState(Thinking); // initial transition
    }

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
            TransitionTo(Hungry);
            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage($"Philosopher {_philosopherId} is exiting thinking state.");
            return null;
        }

        return TopState;
    }

    private QState Hungry(IQEvent qEvent)
    {
        if (qEvent.IsSignal(QSignals.Entry))
        {
            LogMessage($"Philosopher {_philosopherId} is hungry.");
            var tableEvent = new TableEvent(DPPSignal.Hungry, _philosopherId);
            LogMessage($"Philosopher {_philosopherId} publishes Hungry event.");
            Publish(tableEvent);
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Eat))
        {
            if (((TableEvent)qEvent).PhilosopherId == _philosopherId)
            {
                LogMessage($"Philosopher {_philosopherId} receives eat signal.");
                TransitionTo(Eating);
            }

            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage($"Philosopher {_philosopherId} is exiting hungry state.");
            return null;
        }

        return TopState;
    }

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
            TransitionTo(Thinking);
            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage($"Philosopher {_philosopherId} is exiting eating state.");
            var tableEvent = new TableEvent(DPPSignal.Done, _philosopherId);
            LogMessage($"Philosopher {_philosopherId} publishes Done event.");
            Publish(tableEvent);
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
        Console.WriteLine("\t" + Thread.CurrentThread.Name + ": " + message);
    }

    protected override void HsmUnhandledException(Exception e)
    {
    }

    private readonly TimeSpan _thinkTime = new(0, 0, 7); // last parameter represents seconds
    private readonly TimeSpan _eatTime   = new(0, 0, 5); // last parameter represents seconds

    private readonly QTimer _timer;
    private readonly int    _philosopherId;
}
