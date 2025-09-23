using System;
using qf4net;

namespace OptimizationBreaker;

/// <summary>
/// </summary>
public class QHsmDerived1 : QHsmBase1
{
    protected QState m_s021;

    /// <summary>
    /// Default constructor - initializes all fields to default values
    /// </summary>
    public QHsmDerived1()
    {
        m_s021 = s021;
    }

    protected override QState s02(IQEvent qEvent)
    {
        if (qEvent.Signal == QSignals.Init)
        {
            Console.Write("s02-INIT;");
            InitializeState(m_s021);
            return null;
        }

        return base.s02(qEvent);
    }

    protected QState s021(IQEvent qEvent)
    {
        if (qEvent.Signal == QSignals.Entry)
        {
            Console.Write("s021-ENTRY;");
            return null;
        }

        if (qEvent.Signal == QSignals.Exit)
        {
            Console.Write("s021-EXIT;");
            return null;
        }

        return m_s02;
    }
}
