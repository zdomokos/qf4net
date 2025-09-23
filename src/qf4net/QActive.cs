namespace qf4net;

public abstract class QActive : QHsmClassic, IQActive
{
    protected QActive()
    {
        _messagePump = new QEventPump(this, HsmUnhandledException, EventLoopTerminated);
    }

    public Task RunEventPumpAsync(int priority)
    {
        return _messagePump.RunEventPumpAsync(priority);
    }

    public void RunEventPump(int priority)
    {
        _messagePump.RunEventPump(priority);
    }

    public int Priority => _messagePump.Priority;

    public void PostFifo(IQEvent qEvent)
    {
        _messagePump.PostFifo(qEvent);
    }

    public void PostLifo(IQEvent qEvent)
    {
        _messagePump.PostLifo(qEvent);
    }

    protected abstract void HsmUnhandledException(Exception e);
    protected virtual  void EventLoopTerminated(IQEventPump obj) { }

    private readonly QEventPump _messagePump;
}
