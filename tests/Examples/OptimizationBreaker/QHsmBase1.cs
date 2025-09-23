using System;
using qf4net;

namespace OptimizationBreaker;

/// <summary>
/// </summary>
public class QHsmBase1 : QHsmClassic
{
    protected QState m_s0;
    protected QState m_s01;
    protected QState m_s02;

    /// <summary>
    /// Default constructor - initializes all fields to default values
    /// </summary>
    public QHsmBase1()
    {
        m_s0  = s0;
        m_s01 = s01;
        m_s02 = s02;
    }

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        Console.Write("top-INIT;");
        InitializeState(m_s0); // initial transition
    }

    protected QState s0(IQEvent qEvent)
    {
        if (qEvent.Signal == QSignals.Entry)
        {
            Console.Write("s0-ENTRY;");
            return null;
        }

        if (qEvent.Signal == QSignals.Exit)
        {
            Console.Write("s0-EXIT;");
            return null;
        }

        if (qEvent.Signal == QSignals.Init)
        {
            Console.Write("s0-INIT;");
            InitializeState(m_s01);
            return null;
        }

        return TopState;
    }

    private static TransitionChain s_Tran_s01_s02;

    protected QState s01(IQEvent qEvent)
    {
        if (qEvent.Signal == QSignals.Entry)
        {
            Console.Write("s01-ENTRY;");
            return null;
        }

        if (qEvent.Signal == QSignals.Exit)
        {
            Console.Write("s01-EXIT;");
            return null;
        }

        if (qEvent.Signal == MyQSignals.Sig1)
        {
            Console.Write("s01-Sig1;");
            TransitionTo(m_s02, ref s_Tran_s01_s02);
            return null;
        }

        return m_s0;
    }

    private static TransitionChain s_Tran_s02_s01;

    protected virtual QState s02(IQEvent qEvent)
    {
        if (qEvent.Signal == QSignals.Entry)
        {
            Console.Write("s02-ENTRY;");
            return null;
        }

        if (qEvent.Signal == QSignals.Exit)
        {
            Console.Write("s02-EXIT;");
            return null;
        }

        if (qEvent.Signal == MyQSignals.Sig2)
        {
            Console.Write("s02-Sig2;");
            TransitionTo(m_s01, ref s_Tran_s02_s01);
            return null;
        }

        return m_s0;
    }
}
