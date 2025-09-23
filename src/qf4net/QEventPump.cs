namespace qf4net;

public class QEventPump : IQEventPump
{
    public QEventPump(IQHsm qHsm, Action<Exception> unhandledException = null, Action<IQEventPump> eventLoopTerminated = null)
    {
        _qHsm                = qHsm ?? throw new ArgumentNullException(nameof(qHsm));
        _eventLoopTerminated = eventLoopTerminated;
        _unhandledException  = unhandledException;
    }

    public Task RunEventPumpAsync(int priority)
    {
        if (_executionTask != null)
        {
            throw new InvalidOperationException("This active object is already started. The Start method can only be invoked once.");
        }

        if (priority < 0)
        {
            throw new ArgumentException("The priority of an Active Object cannot be negative.", nameof(priority));
        }

        Priority = priority;

        _cancellationTokenSource = new CancellationTokenSource();
        _executionTask = Task.Factory.StartNew(
                                               DoEventLoop,
                                               _cancellationTokenSource.Token,
                                               TaskCreationOptions.LongRunning,
                                               TaskScheduler.Default
                                              );

        _isInitialized = true;
        return _executionTask;
    }

    public void RunEventPump(int priority)
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("This active object is already started. The Start method can only be invoked once.");
        }

        if (priority < 0)
        {
            throw new ArgumentException("The priority of an Active Object cannot be negative.", nameof(priority));
        }

        Priority = priority;

        _cancellationTokenSource = new CancellationTokenSource();
        _isInitialized           = true;

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

    private void DoEventLoop()
    {
        _qHsm.Init();

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var qEvent = _eventQueue.DeQueue(_cancellationTokenSource.Token);
                    if (qEvent == null) { break; } // Cancelled

                    if (qEvent.IsSignal(QSignals.Terminate)) { break; }

                    //Console.WriteLine(String.Format("Dispatching {0} on thread {1}.", qEvent.ToString(), Thread.CurrentThread.Name));
                    _qHsm.Dispatch(qEvent);
                    // QF.Propagate(qEvent);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    try
                    {
                        _unhandledException?.Invoke(e);
                    }
                    catch (Exception)
                    {
                        // TODO: log exception properly
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception)
        {
            // TODO: log exception properly
        }

        _eventLoopTerminated?.Invoke(this);
        // QEventBroadcaster.Remove(this);
    }

    /// <summary>
    /// Cancels the execution thread. Nothing happens thereafter!
    /// </summary>
    protected void Abort()
    {
        _cancellationTokenSource?.Cancel();
    }

    protected void Join()
    {
        _executionTask?.Wait();
        _executionTask = null;
    }

    private readonly IQHsm                   _qHsm;
    private readonly IQEventQueue            _eventQueue = EventQueueFactory.GetEventQueue();
    private          Task                    _executionTask;
    private          CancellationTokenSource _cancellationTokenSource;
    private          bool                    _isInitialized;
    private readonly Action<IQEventPump>     _eventLoopTerminated;
    private readonly Action<Exception>       _unhandledException;
}
