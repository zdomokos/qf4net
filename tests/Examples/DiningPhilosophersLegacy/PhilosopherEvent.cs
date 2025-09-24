using qf4net;

namespace DiningPhilosophersLegacy;

/// <summary>
/// Summary description for PhilosopherEvent.
/// </summary>
public class PhilosopherEvent : QEvent
{
    internal PhilosopherEvent(Signal signal)
        : base(signal) { }
}
