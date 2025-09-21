using qf4net;

namespace DiningPhilosophers;

/// <summary>
/// Summary description for PhilosopherEvent.
/// </summary>
public class PhilosopherEvent : QEvent
{
    internal PhilosopherEvent(Signal signal)
        : base(signal) { }
}