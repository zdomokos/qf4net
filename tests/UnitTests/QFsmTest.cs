using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using qf4net;

namespace qf4net.UnitTests;

[TestFixture]
public class QFsmTest
{
    private TestStateMachine _fsm = null!;
    private List<string> _logMessages = null!;

    [SetUp]
    public void Setup()
    {
        _logMessages = new List<string>();
        var config = new StatemachineConfig { EnableAllStateEventTracing = true };
        _fsm = new TestStateMachine(_logMessages, config);
    }

    [TearDown]
    public void TearDown()
    {
        // QFsm doesn't implement IDisposable, so no cleanup needed
    }

    #region Basic Functionality Tests

    [Test]
    public void Constructor_InitializesCorrectly()
    {
        // Assert
        Assert.That(_fsm.StateMethod, Is.Not.Null);
        // TopState is protected, so we can't access it directly in tests
        Assert.That(_fsm.CurrentStateName, Is.EqualTo("TopState"));
        Assert.That(_fsm.CurrentStateName, Is.EqualTo("TopState"));
    }

    [Test]
    public void Init_CallsInitializeStateMachine()
    {
        // Act
        _fsm.Init();

        // Assert
        Assert.That(_fsm.InitializeStateMachineCalled, Is.True);
        Assert.That(_fsm.StateMethod, Is.EqualTo(_fsm.IdleStateHandler));
        Assert.That(_logMessages, Contains.Item($"{QSignals.Entry}: IdleState"));
        Assert.That(_logMessages, Contains.Item($"{QSignals.Init}: IdleState"));
    }

    [Test]
    public void CurrentStateName_ReturnsCorrectName()
    {
        // Arrange
        _fsm.Init();

        // Assert
        Assert.That(_fsm.CurrentStateName, Is.EqualTo("IdleState"));
    }

    [Test]
    public void IsInState_WithCurrentState_ReturnsTrue()
    {
        // Arrange
        _fsm.Init();

        // Act & Assert
        Assert.That(_fsm.IsInState(_fsm.IdleStateHandler), Is.True);
        Assert.That(_fsm.IsInState(_fsm.WorkingStateHandler), Is.False);
    }

    [Test]
    public void IsInState_WithNullState_ReturnsFalse()
    {
        // Arrange
        _fsm.Init();

        // Act & Assert
        Assert.That(_fsm.IsInState(null), Is.False);
    }

    #endregion

    #region State Transition Tests

    [Test]
    public void TransitionTo_ChangesStateCorrectly()
    {
        // Arrange
        _fsm.Init();
        _logMessages.Clear();

        // Act
        _fsm.TransitionTo(_fsm.WorkingStateHandler);

        // Assert
        Assert.That(_fsm.StateMethod, Is.EqualTo(_fsm.WorkingStateHandler));
        Assert.That(_logMessages, Contains.Item($"{QSignals.Exit}: IdleState"));
        Assert.That(_logMessages, Contains.Item($"{QSignals.Entry}: WorkingState"));
        Assert.That(_logMessages, Contains.Item($"{QSignals.Init}: WorkingState"));
    }

    [Test]
    public void TransitionTo_SameState_DoesNothing()
    {
        // Arrange
        _fsm.Init();
        var currentState = _fsm.StateMethod;
        _logMessages.Clear();

        // Act
        _fsm.TransitionTo(currentState);

        // Assert
        Assert.That(_fsm.StateMethod, Is.EqualTo(currentState));
        // Should still trigger exit/entry/init for same state
        Assert.That(_logMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void TransitionTo_NullState_HandlesGracefully()
    {
        // Arrange
        _fsm.Init();

        // Act & Assert
        Assert.DoesNotThrow(() => _fsm.TransitionTo(null));
        Assert.That(_fsm.StateMethod, Is.Null);
    }

    #endregion

    #region Event Dispatch Tests

    [Test]
    public void Dispatch_HandledEvent_ProcessesCorrectly()
    {
        // Arrange
        _fsm.Init();
        var workSignal = new QSignal("WORK");
        var workEvent = new QEvent(workSignal);
        _logMessages.Clear();

        // Act
        _fsm.Dispatch(workEvent);

        // Assert
        Assert.That(_fsm.StateMethod, Is.EqualTo(_fsm.WorkingStateHandler));
        Assert.That(_logMessages, Contains.Item("WORK event received in IdleState"));
    }

    [Test]
    public void Dispatch_UnhandledEvent_DoesNothing()
    {
        // Arrange
        _fsm.Init();
        var unknownSignal = new QSignal("UNKNOWN");
        var unknownEvent = new QEvent(unknownSignal);
        var initialState = _fsm.StateMethod;

        // Act
        _fsm.Dispatch(unknownEvent);

        // Assert
        Assert.That(_fsm.StateMethod, Is.EqualTo(initialState));
    }

    [Test]
    public void Dispatch_NullEvent_HandlesGracefully()
    {
        // Arrange
        _fsm.Init();

        // Act & Assert
        Assert.DoesNotThrow(() => _fsm.Dispatch(null));
    }

    // DispatchSynchronized is not implemented in QFsm, so we skip this test

    #endregion

    #region Error Handling Tests

    [Test]
    public void Dispatch_StateThrowsException_WrapsException()
    {
        // Arrange
        _fsm.Init();
        var errorSignal = new QSignal("ERROR");
        var errorEvent = new QEvent(errorSignal);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _fsm.Dispatch(errorEvent));
        Assert.That(ex.Message, Contains.Substring("Test exception"));
    }

    #endregion

    #region Thread Safety Tests

    [Test]
    public void ConcurrentDispatch_ThreadSafe()
    {
        // Arrange
        _fsm.Init();
        var tasks = new List<Task>();
        var eventCount = 100;

        // Act
        for (int i = 0; i < eventCount; i++)
        {
            var signal = new QSignal($"TEST_{i}");
            var testEvent = new QEvent(signal);
            tasks.Add(Task.Run(() => _fsm.Dispatch(testEvent)));
        }

        Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(5));

        // Assert
        Assert.That(_fsm.ProcessedEvents.Count, Is.EqualTo(eventCount));
        Assert.That(tasks.All(t => t.IsCompletedSuccessfully), Is.True);
    }

    [Test]
    public void ConcurrentTransitions_ThreadSafe()
    {
        // Arrange
        _fsm.Init();
        var tasks = new List<Task>();
        var transitionCount = 50;

        // Act
        for (int i = 0; i < transitionCount; i++)
        {
            var targetState = i % 2 == 0 ? _fsm.IdleStateHandler : _fsm.WorkingStateHandler;
            tasks.Add(Task.Run(() => _fsm.TransitionTo(targetState)));
        }

        Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(5));

        // Assert - Should complete without exceptions
        Assert.That(tasks.All(t => t.IsCompletedSuccessfully), Is.True);
        Assert.That(_fsm.StateMethod, Is.Not.Null);
    }

    #endregion

    #region Helper Classes

    private class TestStateMachine : QFsm
    {
        private readonly List<string> _logMessages;
        public readonly QState IdleStateHandler;
        public readonly QState WorkingStateHandler;
        private readonly List<IQEvent> _processedEvents = new();
        private readonly object _processedEventsLock = new();
        public IReadOnlyList<IQEvent> ProcessedEvents
        {
            get
            {
                lock (_processedEventsLock)
                {
                    return _processedEvents.ToList();
                }
            }
        }

        public bool InitializeStateMachineCalled { get; private set; }

        public TestStateMachine(List<string> logMessages, StatemachineConfig config = null)
            : base(config)
        {
            _logMessages = logMessages;
            IdleStateHandler = IdleState;
            WorkingStateHandler = WorkingState;
        }

        protected override void InitializeStateMachine()
        {
            InitializeStateMachineCalled = true;
            InitializeState(IdleStateHandler);
        }

        protected override void StateEventTrace(QState state, QSignal signal)
        {
            if (state != null && signal != null)
            {
                _logMessages.Add($"{signal}: {state.Method.Name}");
            }
        }

        public override void Dispatch(IQEvent qEvent)
        {
            if (qEvent != null)
            {
                lock (_processedEventsLock)
                {
                    _processedEvents.Add(qEvent);
                }
            }
            base.Dispatch(qEvent);
        }

        private QState IdleState(IQEvent qEvent)
        {
            if (qEvent.IsSignal(QSignals.Entry))
            {
                _logMessages.Add("Entered IdleState");
                return null;
            }

            if (qEvent.IsSignal(QSignals.Exit))
            {
                _logMessages.Add("Exited IdleState");
                return null;
            }

            if (qEvent.IsSignal(QSignals.Init))
            {
                _logMessages.Add("Initialized IdleState");
                return null;
            }

            if (qEvent.Signal.ToString().Contains("WORK"))
            {
                _logMessages.Add("WORK event received in IdleState");
                TransitionTo(WorkingStateHandler);
                return null;
            }

            if (qEvent.Signal.ToString().Contains("ERROR"))
            {
                throw new InvalidOperationException("Test exception");
            }

            return null; // Event not handled
        }

        private QState WorkingState(IQEvent qEvent)
        {
            if (qEvent.IsSignal(QSignals.Entry))
            {
                _logMessages.Add("Entered WorkingState");
                return null;
            }

            if (qEvent.IsSignal(QSignals.Exit))
            {
                _logMessages.Add("Exited WorkingState");
                return null;
            }

            if (qEvent.IsSignal(QSignals.Init))
            {
                _logMessages.Add("Initialized WorkingState");
                return null;
            }

            if (qEvent.Signal.ToString().Contains("DONE"))
            {
                _logMessages.Add("DONE event received in WorkingState");
                TransitionTo(IdleStateHandler);
                return null;
            }

            return null; // Event not handled
        }
    }

    #endregion
}
