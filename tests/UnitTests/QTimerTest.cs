using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests;

[TestFixture]
public class QTimerTest
{
    private TestEventPump _eventPump;
    private qf4net.QTimer _timer;

    [SetUp]
    public void SetUp()
    {
        _eventPump = new TestEventPump();
        _timer = new qf4net.QTimer(_eventPump);
    }

    [TearDown]
    public void TearDown()
    {
        _timer?.Dispose();
    }

    [Test]
    public void CanCreateQTimer()
    {
        // Arrange & Act
        var timer = new qf4net.QTimer(_eventPump);

        // Assert
        Assert.That(timer, Is.Not.Null);

        timer.Dispose();
    }

    [Test]
    public void FireIn_WithValidParameters_FiresEventAfterDelay()
    {
        // Arrange
        var testEvent = new TestEvent();
        var delay = TimeSpan.FromMilliseconds(50);

        // Act
        _timer.FireIn(delay, testEvent);
        Thread.Sleep(100); // Wait for timer to fire

        // Assert
        Assert.That(_eventPump.ReceivedEvents, Contains.Item(testEvent));
    }

    [Test]
    public void FireIn_WithZeroTimeSpan_ThrowsArgumentException()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _timer.FireIn(TimeSpan.Zero, testEvent));
    }

    [Test]
    public void FireIn_WithNegativeTimeSpan_ThrowsArgumentException()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _timer.FireIn(TimeSpan.FromMilliseconds(-100), testEvent));
    }

    [Test]
    public void FireIn_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => _timer.FireIn(TimeSpan.FromMilliseconds(100), null));
    }

    [Test]
    public void FireEvery_WithValidParameters_FiresEventsPeriodically()
    {
        // Arrange
        var testEvent = new TestEvent();
        var interval = TimeSpan.FromMilliseconds(50);

        // Act
        _timer.FireEvery(interval, testEvent);
        Thread.Sleep(150); // Wait for multiple timer fires

        // Assert
        Assert.That(_eventPump.ReceivedEvents.Count, Is.GreaterThan(1));
        Assert.That(_eventPump.ReceivedEvents, Has.All.EqualTo(testEvent));
    }

    [Test]
    public void FireEvery_WithZeroTimeSpan_ThrowsArgumentException()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _timer.FireEvery(TimeSpan.Zero, testEvent));
    }

    [Test]
    public void FireEvery_WithNegativeTimeSpan_ThrowsArgumentException()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _timer.FireEvery(TimeSpan.FromMilliseconds(-100), testEvent));
    }

    [Test]
    public void FireEvery_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => _timer.FireEvery(TimeSpan.FromMilliseconds(100), null));
    }

    [Test]
    public void Disarm_AfterFireIn_PreventsEventFromFiring()
    {
        // Arrange
        var testEvent = new TestEvent();
        _timer.FireIn(TimeSpan.FromMilliseconds(100), testEvent);

        // Act
        _timer.Disarm();
        Thread.Sleep(150); // Wait past when timer would have fired

        // Assert
        Assert.That(_eventPump.ReceivedEvents, Is.Empty);
    }

    [Test]
    public void Disarm_AfterFireEvery_StopsPeriodicEvents()
    {
        // Arrange
        var testEvent = new TestEvent();
        _timer.FireEvery(TimeSpan.FromMilliseconds(50), testEvent);
        Thread.Sleep(75); // Let one event fire
        var initialCount = _eventPump.ReceivedEvents.Count;

        // Act
        _timer.Disarm();
        Thread.Sleep(100); // Wait to see if more events fire

        // Assert
        Assert.That(_eventPump.ReceivedEvents.Count, Is.EqualTo(initialCount));
    }

    [Test]
    public void Rearm_WithValidTimeSpan_FiresEventAfterDelay()
    {
        // Arrange
        var testEvent = new TestEvent();
        _timer.FireIn(TimeSpan.FromMilliseconds(200), testEvent);
        // Don't disarm - we need the event to remain configured

        // Act
        _timer.Rearm(TimeSpan.FromMilliseconds(50));
        Thread.Sleep(100);

        // Assert
        Assert.That(_eventPump.ReceivedEvents, Contains.Item(testEvent));
    }

    [Test]
    public void Rearm_WithZeroTimeSpan_ThrowsArgumentException()
    {
        // Arrange
        var testEvent = new TestEvent();
        _timer.FireIn(TimeSpan.FromMilliseconds(100), testEvent);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _timer.Rearm(TimeSpan.Zero));
    }

    [Test]
    public void Rearm_WithNegativeTimeSpan_ThrowsArgumentException()
    {
        // Arrange
        var testEvent = new TestEvent();
        _timer.FireIn(TimeSpan.FromMilliseconds(100), testEvent);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _timer.Rearm(TimeSpan.FromMilliseconds(-50)));
    }

    [Test]
    public void Rearm_WithoutPreviousEvent_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => _timer.Rearm(TimeSpan.FromMilliseconds(100)));
    }

    [Test]
    public void FireIn_AfterDispose_DoesNotFireEvent()
    {
        // Arrange
        var testEvent = new TestEvent();
        _timer.Dispose();

        // Act
        _timer.FireIn(TimeSpan.FromMilliseconds(50), testEvent);
        Thread.Sleep(100);

        // Assert
        Assert.That(_eventPump.ReceivedEvents, Is.Empty);
    }

    [Test]
    public void MultipleFireIn_LastEventOverridesPrevious()
    {
        // Arrange
        var firstEvent = new TestEvent();
        var secondEvent = new TestEvent();

        // Act
        _timer.FireIn(TimeSpan.FromMilliseconds(100), firstEvent);
        _timer.FireIn(TimeSpan.FromMilliseconds(50), secondEvent);
        Thread.Sleep(150);

        // Assert
        Assert.That(_eventPump.ReceivedEvents, Contains.Item(secondEvent));
        Assert.That(_eventPump.ReceivedEvents, Does.Not.Contain(firstEvent));
    }

    [Test]
    public void ConcurrentAccess_ThreadSafeOperation()
    {
        // Arrange
        var tasks = new List<Task>();
        var events = new List<qf4net.IQEvent>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var testEvent = new TestEvent();
            events.Add(testEvent);
            tasks.Add(Task.Run(() => _timer.FireIn(TimeSpan.FromMilliseconds(50), testEvent)));
        }

        Task.WaitAll(tasks.ToArray());
        Thread.Sleep(100);

        // Assert
        Assert.That(_eventPump.ReceivedEvents.Count, Is.EqualTo(1));
        Assert.That(_eventPump.ReceivedEvents[0], Is.AnyOf(events));
    }

    private class TestEventPump : qf4net.IQEventPump
    {
        public List<qf4net.IQEvent> ReceivedEvents { get; } = new List<qf4net.IQEvent>();
        public int Priority => 0;

        public void PostFifo(qf4net.IQEvent qEvent)
        {
            lock (ReceivedEvents)
            {
                ReceivedEvents.Add(qEvent);
            }
        }

        public void PostLifo(qf4net.IQEvent qEvent)
        {
            lock (ReceivedEvents)
            {
                ReceivedEvents.Insert(0, qEvent);
            }
        }

        public Task RunEventPumpAsync(int priority) => Task.CompletedTask;
        public void RunEventPump(int priority) { }
    }

    private class TestEvent : qf4net.IQEvent
    {
        public qf4net.QSignal Signal => qf4net.QSignals.Empty;

        public bool IsSignal(qf4net.QSignal sig) => Signal.Equals(sig);
    }
}
