namespace qf4net;

public abstract class QActive2 : IQActive
{
    protected QActive2(IQEventBroker eventBroker)
    {
        _eventBroker = eventBroker;
        _hsm         = new QHsm2(InitializeStateMachine);
        _messagePump = new QEventPump(_hsm,
                                      HsmUnhandledException,
                                      mp => _eventBroker.Unregister(this));
    }

    public int    Priority               => _messagePump.Priority;
    public string CurrentStateName       => _hsm.CurrentStateName;
    public string CurrentNestedStateName => _hsm.CurrentNestedStateName;

    public Task RunEventPumpAsync(int priority) => _messagePump.RunEventPumpAsync(priority);
    public void RunEventPump(int priority)      => _messagePump.RunEventPump(priority);
    public void PostFifo(IQEvent qEvent)        => _messagePump.PostFifo(qEvent);
    public void PostLifo(IQEvent qEvent)        => _messagePump.PostLifo(qEvent);
    public void Publish(IQEvent qEvent)         => _eventBroker?.Publish(qEvent);

    public void InitializeState(QState state) => _hsm.InitializeState(state);

    protected abstract void HsmUnhandledException(Exception e);
    protected abstract void InitializeStateMachine();

    public void TransitionTo(QState state)
    {
        _hsm.TransitionTo(state);
        PostFifo(QSignals.EvtStateJob);
    }

    public void TransitionAfterRetry(QState nextState, int maxRetry, TimeSpan retryDelay, Exception ex)
    {

    }

    public QState TopState => _hsm.TopState;


    protected readonly QHsm2         _hsm;
    protected readonly QEventPump    _messagePump;
    protected readonly IQEventBroker _eventBroker;
}
