using System;
using qf4net;

namespace OptimizationBreaker;

/// <summary>
/// </summary>
public class QHsmDerived2 : QHsmBase2
{
    protected QState m_s021;

    protected new enum TransIdx
    {
        s021_s02 = QHsmBase2.TransIdx.End,
        End,
    }

    private static readonly TransitionChain[] s_TransitionChains = new TransitionChain[
            (int)TransIdx.End
        ];

    //private static TransitionChain[] s_TransitionChains = new TransitionChain[(int)QHsmBase2.TransIdx.End];

    /// <summary>
    /// Must be used as the base constructor by an inheriting state machine that adds more
    /// state hierarchy levels.
    /// </summary>
    /// <param name="maxHierarchyDepth">The maximum required depth of state hierarchies.</param>
    public QHsmDerived2()
    {
        m_s021 = s021;
    }

    protected override TransitionChain[] TransitionChains => s_TransitionChains;

    protected override QState s02(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Init)
        {
            Console.Write("s02-INIT;");
            InitializeState(m_s021);
            return null;
        }

        return base.s02(qEvent);
    }

    protected QState s021(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.Write("s021-ENTRY;");
            return null;
        }

        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.Write("s021-EXIT;");
            return null;
        }

        return m_s02;
    }
}
