namespace qf4net;

public abstract class QActive2 : QHsm2, IQActive
{
    protected QActive2(IQEventBroker eventBroker = null)
    {
        _eventBroker = eventBroker;
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

    public void Publish(IQEvent qEvent)
    {
        _eventBroker?.Publish(qEvent);
    }

    protected abstract void HsmUnhandledException(Exception e);

    protected virtual void EventLoopTerminated(IQEventPump obj)
    {
        if (_eventBroker != null)
        {
            _eventBroker.Unregister(this);
        }
    }

    private readonly QEventPump    _messagePump;
    protected readonly IQEventBroker _eventBroker;
}
