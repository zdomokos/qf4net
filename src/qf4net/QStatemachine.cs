using System.Diagnostics;

namespace qf4net;

/// <summary>
/// Simple state machine with event pump functionality.
/// Provides thread-safe event queueing and simple state dispatch.
/// </summary>
public class QStateMachine : IQHsm, IQActive
{
    private readonly IQEventQueue _eventQueue;
    private readonly Action<string> _log;
    private readonly object _synchObj = new();
    private Task _executionTask;
    private CancellationTokenSource _cancellationTokenSource;
    private QState _currentState;
    private bool _isInitialized;

    public QStateMachine(Action<string> log = null)
    {
        _log = log ?? Log;
        _eventQueue = EventQueueFactory.GetEventQueue();
        Priority = -1;
    }

    #region IQHsm Implementation

    public void Init()
    {
        // Empty implementation - use InitializeState instead
    }

    public bool IsInState(QState inquiredState)
    {
        lock (_synchObj)
        {
            return _currentState == inquiredState;
        }
    }

    public string CurrentStateName => _currentState?.Method.Name ?? "n/a";

    public void Dispatch(IQEvent qEvent)
    {
        if (_isInitialized)
        {
            _eventQueue.EnqueueFifo(qEvent);
        }
        else
        {
            // Direct dispatch when not running event pump
            lock (_synchObj)
            {
                DispatchDirectly(qEvent);
            }
        }
    }

    #endregion

    #region IQActive Implementation

    public Task RunEventPumpAsync(int priority)
    {
        if (_executionTask != null)
        {
            throw new InvalidOperationException("State machine is already running.");
        }

        if (priority < 0)
        {
            throw new ArgumentException("Priority cannot be negative.", nameof(priority));
        }

        Priority = priority;
        _cancellationTokenSource = new CancellationTokenSource();
        _executionTask = Task.Factory.StartNew(DoEventLoop, _cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _isInitialized = true;
        return _executionTask;
    }

    public void RunEventPump(int priority)
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("State machine is already running.");
        }

        if (priority < 0)
        {
            throw new ArgumentException("Priority cannot be negative.", nameof(priority));
        }

        Priority = priority;
        _cancellationTokenSource = new CancellationTokenSource();
        _isInitialized = true;
        DoEventLoop();
    }

    public int Priority { get; private set; }

    public void PostFifo(IQEvent qEvent)
    {
        _eventQueue.EnqueueFifo(qEvent);
    }

    public void PostLifo(IQEvent qEvent)
    {
        _eventQueue.EnqueueLifo(qEvent);
    }

    #endregion

    #region State Management

    public void InitializeState(QState state)
    {
        lock (_synchObj)
        {
            _currentState = state;
            _log($"Initializing state: {state.Method.Name}");
            _currentState(new QEvent(QSignals.Entry));
        }
    }

    public void TransitionTo(QState stateTo)
    {
        lock (_synchObj)
        {
            if (_currentState != stateTo)
            {
                _log($"Transitioning from {_currentState?.Method.Name ?? "null"} to {stateTo.Method.Name}");

                if (_currentState != null)
                {
                    _currentState(new QEvent(QSignals.Exit));
                }

                _currentState = stateTo;
                _currentState(new QEvent(QSignals.Entry));
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    public void Join()
    {
        _executionTask?.Wait();
        _executionTask = null;
    }

    #endregion

    #region Private Methods

    private void DoEventLoop()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var qEvent = _eventQueue.DeQueue(_cancellationTokenSource.Token);
                if (qEvent == null) // Cancelled
                {
                    break;
                }

                if (qEvent.IsSignal(QSignals.Terminate))
                {
                    break;
                }

                lock (_synchObj)
                {
                    DispatchDirectly(qEvent);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
    }

    private void DispatchDirectly(IQEvent qEvent)
    {
        if (_currentState != null && qEvent != null)
        {
            _log($"Dispatching {qEvent} to {_currentState.Method.Name}");
            _currentState(qEvent);
        }
    }

    private void Log(string message)
    {
        Trace.WriteLine(message);
    }

    #endregion
}
