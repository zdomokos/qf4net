using qf4net;

namespace DiningPhilosophersClassic;

/// <summary>
/// Summary description for PhilosopherEvent.
/// </summary>
public class PhilosopherEvent : QEvent
{
    internal PhilosopherEvent(Signal signal)
        : base(signal) { }
}