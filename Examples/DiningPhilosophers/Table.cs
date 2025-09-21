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
    private readonly QState m_StateServing;
    private readonly int    m_NumberOfPhilosophers;
    private readonly bool[] m_ForkIsUsed;
    private readonly bool[] m_PhilosopherIsHungry;

    public Table(int numberOfPhilosophers)
    {
        m_NumberOfPhilosophers = numberOfPhilosophers;
        m_ForkIsUsed           = new bool[m_NumberOfPhilosophers];
        m_PhilosopherIsHungry  = new bool[m_NumberOfPhilosophers];

        for (var i = 0; i < m_NumberOfPhilosophers; i++)
        {
            m_ForkIsUsed[i]          = false;
            m_PhilosopherIsHungry[i] = false;
        }

        m_StateServing = Serving;
    }

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        Thread.CurrentThread.Name = ToString();
        // Subscribe for the relevant events raised by philosophers
        Qf.Instance.Subscribe(this, DPPSignal.Hungry);
        Qf.Instance.Subscribe(this, DPPSignal.Done);

        InitializeState(m_StateServing); // initial transition
    }

    private QState Serving(IQEvent qEvent)
    {
        int philosopherId;

        if (qEvent.IsSignal(DPPSignal.Hungry))
        {
            philosopherId = GetPhilosopherId(qEvent);
            Debug.Assert(
                         !m_PhilosopherIsHungry[philosopherId],
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
                m_PhilosopherIsHungry[philosopherId] = true; // mark philosopher as hungry
                Console.WriteLine(
                                  $"Philosopher {philosopherId} has to wait for forks."
                                 );
            }
            return null;
        }

        if (qEvent.IsSignal(DPPSignal.Done))
        {
            philosopherId = GetPhilosopherId(qEvent);
            Console.WriteLine($"Philosopher {philosopherId} is done eating.");
            m_PhilosopherIsHungry[philosopherId] = false;

            // free up the philosopher's forks
            FreeForks(philosopherId);

            // Can the left philosopher eat?
            var neighborPhilosopher = LeftIndex(philosopherId);
            if (m_PhilosopherIsHungry[neighborPhilosopher] && ForksFree(neighborPhilosopher))
            {
                LetPhilosopherEat(neighborPhilosopher);
                // The left philosopher could eat; mark philosopher as no longer hungry
                m_PhilosopherIsHungry[neighborPhilosopher] = false;
            }

            // Can the right philosopher eat?
            neighborPhilosopher = RightIndex(philosopherId);
            if (m_PhilosopherIsHungry[neighborPhilosopher] && ForksFree(neighborPhilosopher))
            {
                LetPhilosopherEat(neighborPhilosopher);
                // The left philosopher could eat; mark philosopher as no longer hungry
                m_PhilosopherIsHungry[neighborPhilosopher] = false;
            }

            return null;
        }
        return TopState;
    }

    private int GetPhilosopherId(IQEvent qEvent)
    {
        var philosopherId = ((TableEvent)qEvent).PhilosopherId;
        Debug.Assert(
                     philosopherId < m_NumberOfPhilosophers,
                     "Philosopher id must be < number of philosophers"
                    );
        return philosopherId;
    }

    private void LetPhilosopherEat(int philosopherId)
    {
        UseForks(philosopherId);
        var tableEvent = new TableEvent(DPPSignal.Eat, philosopherId);
        Debug.WriteLine(
                        $"Table publishes Eat event for Philosopher {philosopherId}."
                       );

        Qf.Instance.Publish(tableEvent);
        Console.WriteLine($"Philosopher {philosopherId} is eating.");
    }

    private int LeftIndex(int index)
    {
        return (index + 1) % m_NumberOfPhilosophers;
    }

    private int RightIndex(int index)
    {
        return (index - 1 + m_NumberOfPhilosophers) % m_NumberOfPhilosophers;
    }

    private bool ForksFree(int philosopherId)
    {
        return (
                   m_ForkIsUsed[philosopherId] == false
                && m_ForkIsUsed[LeftIndex(philosopherId)] == false
               );
    }

    private void UseForks(int philosopherId)
    {
        m_ForkIsUsed[philosopherId]            = true;
        m_ForkIsUsed[LeftIndex(philosopherId)] = true;
    }

    private void FreeForks(int philosopherId)
    {
        m_ForkIsUsed[philosopherId]            = false;
        m_ForkIsUsed[LeftIndex(philosopherId)] = false;
    }

    public override string ToString()
    {
        return "Table";
    }

    protected override void HsmUnhandledException(Exception e)
    {
        throw new NotImplementedException();
    }
}
