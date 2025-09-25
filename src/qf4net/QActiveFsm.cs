namespace qf4net;

public class QActiveFsm : QFsm, IQActive
{
    public QActiveFsm(IQEventBroker eventBroker = null, StatemachineConfig fsmConfig = null) : base(fsmConfig)
    {
        _eventBroker = eventBroker;
        _eventPump   = new QEventPump(this, HsmUnhandledException, EventLoopTerminated);
    }

    public int  Priority                        => _eventPump.Priority;
    public Task RunEventPumpAsync(int priority) => _eventPump.RunEventPumpAsync(priority);
    public void RunEventPump(int priority)      => _eventPump.RunEventPump(priority);
    public void PostFifo(IQEvent qEvent)        => _eventPump.PostFifo(qEvent);
    public void PostLifo(IQEvent qEvent)        => _eventPump.PostLifo(qEvent);

    public void Publish(IQEvent qEvent)                       => _eventBroker.Publish(qEvent);
    public void Subscribe(IQActive qActive, Signal qSignal)   => _eventBroker.Subscribe(qActive, qSignal);
    public void Unsubscribe(IQActive qActive, Signal qSignal) => _eventBroker.Unsubscribe(qActive, qSignal);
    public void Unregister()                                  => _eventBroker.Unregister(this);

    public void Transition(QState targetState) => TransitionTo(targetState);

    protected virtual void HsmUnhandledException(Exception e)   { }
    protected virtual void EventLoopTerminated(IQEventPump obj) { }

    private readonly QEventPump         _eventPump;
    private readonly IQEventBroker      _eventBroker;
}

