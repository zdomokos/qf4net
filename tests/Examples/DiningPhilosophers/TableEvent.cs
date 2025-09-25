using qf4net;

namespace DiningPhilosophers;

/// <summary>
/// Summary description for TableEvent.
/// </summary>
public class TableEvent : QEvent
{
    internal readonly int PhilosopherId;

    internal TableEvent(QSignal signal, int philosopherId)
        : base(signal)
    {
        PhilosopherId = philosopherId;
    }
}
