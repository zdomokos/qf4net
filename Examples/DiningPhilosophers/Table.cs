using System;
using System.Diagnostics;
using System.Threading;
using qf4net;

namespace DiningPhilosophers;

/// <summary>
/// The active object that represents the table
/// </summary>
public class Table : QActive
{
    public Table(int numberOfPhilosophers)
    {
        _numberOfPhilosophers = numberOfPhilosophers;
        _forkIsUsed = new bool[_numberOfPhilosophers];
        _philosopherIsHungry = new bool[_numberOfPhilosophers];

        for (var i = 0; i < _numberOfPhilosophers; i++)
        {
            _forkIsUsed[i] = false;
            _philosopherIsHungry[i] = false;
        }

        _stateServing = Serving;
    }

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        Thread.CurrentThread.Name = ToString();
        // Subscribe for the relevant events raised by philosophers
        QfEventBroadcaster.Instance.Subscribe(this, DPPSignal.Hungry);
        QfEventBroadcaster.Instance.Subscribe(this, DPPSignal.Done);

        InitializeState(_stateServing); // initial transition
    }

    private QState Serving(IQEvent qEvent)
    {
        int philosopherId;

        if (qEvent.IsSignal(DPPSignal.Entry))
        {
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Hungry))
        {
            philosopherId = GetPhilosopherId(qEvent);
            Debug.Assert(
                !_philosopherIsHungry[philosopherId],
                "Philosopher must not be already hungry"
            );

            Console.WriteLine($"Philosopher {philosopherId} is hungry.");

            if (ForksFree(philosopherId))
            {
                LetPhilosopherEat(philosopherId);
            }
            else
            {
                // The philosopher has to wait for free forks
                _philosopherIsHungry[philosopherId] = true; // mark philosopher as hungry
                Console.WriteLine($"Philosopher {philosopherId} has to wait for forks.");
            }
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Done))
        {
            philosopherId = GetPhilosopherId(qEvent);
            Console.WriteLine($"Philosopher {philosopherId} is done eating.");
            _philosopherIsHungry[philosopherId] = false;

            // free up the philosopher's forks
            FreeForks(philosopherId);

            // Can the left philosopher eat?
            var neighborPhilosopher = LeftIndex(philosopherId);
            if (_philosopherIsHungry[neighborPhilosopher] && ForksFree(neighborPhilosopher))
            {
                LetPhilosopherEat(neighborPhilosopher);
                // The left philosopher could eat; mark philosopher as no longer hungry
                _philosopherIsHungry[neighborPhilosopher] = false;
            }

            // Can the right philosopher eat?
            neighborPhilosopher = RightIndex(philosopherId);
            if (_philosopherIsHungry[neighborPhilosopher] && ForksFree(neighborPhilosopher))
            {
                LetPhilosopherEat(neighborPhilosopher);
                // The left philosopher could eat; mark philosopher as no longer hungry
                _philosopherIsHungry[neighborPhilosopher] = false;
            }

            return null;
        }
        return TopState;
    }

    private int GetPhilosopherId(IQEvent qEvent)
    {
        var philosopherId = ((TableEvent)qEvent).PhilosopherId;
        Debug.Assert(
            philosopherId < _numberOfPhilosophers,
            "Philosopher id must be < number of philosophers"
        );
        return philosopherId;
    }

    private void LetPhilosopherEat(int philosopherId)
    {
        UseForks(philosopherId);
        var tableEvent = new TableEvent(DPPSignal.Eat, philosopherId);
        Debug.WriteLine($"Table publishes Eat event for Philosopher {philosopherId}.");

        QfEventBroadcaster.Instance.Publish(tableEvent);
        Console.WriteLine($"Philosopher {philosopherId} is eating.");
    }

    private int LeftIndex(int index)
    {
        return (index + 1) % _numberOfPhilosophers;
    }

    private int RightIndex(int index)
    {
        return (index - 1 + _numberOfPhilosophers) % _numberOfPhilosophers;
    }

    private bool ForksFree(int philosopherId)
    {
        return _forkIsUsed[philosopherId] == false
            && _forkIsUsed[LeftIndex(philosopherId)] == false;
    }

    private void UseForks(int philosopherId)
    {
        _forkIsUsed[philosopherId] = true;
        _forkIsUsed[LeftIndex(philosopherId)] = true;
    }

    private void FreeForks(int philosopherId)
    {
        _forkIsUsed[philosopherId] = false;
        _forkIsUsed[LeftIndex(philosopherId)] = false;
    }

    public override string ToString()
    {
        return "Table";
    }

    protected override void HsmUnhandledException(Exception e)
    {
        throw new NotImplementedException();
    }

    private readonly QState _stateServing;
    private readonly int _numberOfPhilosophers;
    private readonly bool[] _forkIsUsed;
    private readonly bool[] _philosopherIsHungry;
}
