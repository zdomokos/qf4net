using System;
using System.Threading;
using qf4net;

namespace DiningPhilosophersLegacy;

/// <summary>
/// The active object that represents the table
/// </summary>
public class Philosopher : QActiveLegacy
{
    public Philosopher(int philosopherId)
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
        QEventBrokerSingleton.Instance.Subscribe(this, DPPSignal.Eat);
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
            TransitionTo(Hungry, ref s_Tran_Thinking_Hungry);
            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage($"Philosopher {_philosopherId} is exiting thinking state.");
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
            QEventBrokerSingleton.Instance.Publish(tableEvent);
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Eat))
        {
            if (((TableEvent)qEvent).PhilosopherId == _philosopherId)
            {
                LogMessage($"Philosopher {_philosopherId} receives eat signal.");
                TransitionTo(Eating, ref s_Tran_Hungry_Eating);
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
            TransitionTo(Thinking, ref s_Tran_Eating_Thinking);
            return null;
        }

        if (qEvent.IsSignal(QSignals.Exit))
        {
            LogMessage($"Philosopher {_philosopherId} is exiting eating state.");
            var tableEvent = new TableEvent(DPPSignal.Done, _philosopherId);
            LogMessage($"Philosopher {_philosopherId} publishes Done event.");
            QEventBrokerSingleton.Instance.Publish(tableEvent);
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

    private static TransitionChain s_Tran_Thinking_Hungry;

    private readonly TimeSpan _thinkTime = new(0, 0, 7); // last parameter represents seconds
    private readonly TimeSpan _eatTime   = new(0, 0, 5); // last parameter represents seconds

    private readonly QTimer _timer;
    private readonly int    _philosopherId;

}
