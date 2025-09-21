using System;
using qf4net;

namespace QHsmTest;

/// <summary>
/// TODO - Add class summary
/// </summary>
public class QHsmTest : QHsm
{
    protected bool   m_Foo;
    protected QState m_s0;
    protected QState m_s1;
    protected QState m_s11;
    protected QState m_s2;
    protected QState m_s21;
    protected QState m_s211;

    /// <summary>
    /// Default constructor - initializes all fields to default values
    /// </summary>
    public QHsmTest()
    {
        m_s0   = s0;
        m_s1   = s1;
        m_s11  = s11;
        m_s2   = s2;
        m_s21  = s21;
        m_s211 = s211;
    }

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        Console.Write("top-INIT;");
        m_Foo = false;
        InitializeState(m_s0); // initial transition
    }

    private static TransitionChain s_Tran_s0_s211;

    protected QState s0(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.Write("s0-ENTRY;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.Write("s0-EXIT;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Init)
        {
            Console.Write("s0-INIT;");
            InitializeState(m_s1);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.E_Sig)
        {
            Console.Write("s0-E;");
            TransitionTo(m_s211, ref s_Tran_s0_s211);
            return null;
        }

        return TopState;
    }

    private static TransitionChain s_Tran_s1_s1;
    private static TransitionChain s_Tran_s1_s11;
    private static TransitionChain s_Tran_s1_s2;
    private static TransitionChain s_Tran_s1_s0;
    private static TransitionChain s_Tran_s1_s211;

    protected virtual QState s1(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.Write("s1-ENTRY;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.Write("s1-EXIT;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Init)
        {
            Console.Write("s1-INIT;");
            InitializeState(m_s11);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.A_Sig)
        {
            Console.Write("s1-A;");
            TransitionTo(m_s1, ref s_Tran_s1_s1);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.B_Sig)
        {
            Console.Write("s1-B;");
            TransitionTo(m_s11, ref s_Tran_s1_s11);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.C_Sig)
        {
            Console.Write("s1-C;");
            TransitionTo(m_s2, ref s_Tran_s1_s2);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.D_Sig)
        {
            Console.Write("s1-D;");
            TransitionTo(m_s0, ref s_Tran_s1_s0);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.F_Sig)
        {
            Console.Write("s1-F;");
            TransitionTo(m_s211, ref s_Tran_s1_s211);
            return null;
        }

        return m_s0;
    }

    private static TransitionChain s_Tran_s11_s211;

    protected QState s11(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.Write("s11-ENTRY;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.Write("s11-EXIT;");
            return null;
        }

        if (qEvent.QSignal == MyQSignals.G_Sig)
        {
            Console.Write("s11-G;");
            TransitionTo(m_s211, ref s_Tran_s11_s211);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.H_Sig) // internal transition with a guard
        {
            if (m_Foo) // test the guard condition
            {
                Console.Write("s11-H;");
                m_Foo = false;
                return null;
            }
        }

        return m_s1;
    }

    private static TransitionChain s_Tran_s2_s1;
    private static TransitionChain s_Tran_s2_s11;

    protected QState s2(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.Write("s2-ENTRY;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.Write("s2-EXIT;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Init)
        {
            Console.Write("s2-INIT;");
            InitializeState(m_s21);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.C_Sig)
        {
            Console.Write("s2-C;");
            TransitionTo(m_s1, ref s_Tran_s2_s1);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.F_Sig)
        {
            Console.Write("s2-F;");
            TransitionTo(m_s11, ref s_Tran_s2_s11);
            return null;
        }

        return m_s0;
    }

    private static TransitionChain s_Tran_s21_s211;
    private static TransitionChain s_Tran_s21_s21;

    protected QState s21(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.Write("s21-ENTRY;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.Write("s21-EXIT;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Init)
        {
            Console.Write("s21-INIT;");
            InitializeState(m_s211);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.B_Sig)
        {
            Console.Write("s21-B;");
            TransitionTo(m_s211, ref s_Tran_s21_s211);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.H_Sig) // self transition with a guard
        {
            if (!m_Foo) // test the guard condition
            {
                Console.Write("s21-H;");
                m_Foo = true;
                TransitionTo(m_s21, ref s_Tran_s21_s21); // self transition
                return null;
            }
        } // break to return the superstate

        return m_s2; // return the superstate
    }

    private static TransitionChain s_Tran_s211_s21;
    private static TransitionChain s_Tran_s211_s0;

    protected QState s211(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.Write("s211-ENTRY;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.Write("s211-EXIT;");
            return null;
        }

        if (qEvent.QSignal == MyQSignals.D_Sig)
        {
            Console.Write("s211-D;");
            TransitionTo(m_s21, ref s_Tran_s211_s21);
            return null;
        }

        if (qEvent.QSignal == MyQSignals.G_Sig)
        {
            Console.Write("s211-G;");
            TransitionTo(m_s0, ref s_Tran_s211_s0);
            return null;
        }

        return m_s21;
    }
}
