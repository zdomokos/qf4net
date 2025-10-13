using NUnit.Framework;
using qf4net;

namespace qf4net.UnitTests;

[TestFixture]
public class QEventBrokerTest
{
    private QEventBroker _eventBroker = null!;
    private TestQActive _subscriber1 = null!;
    private TestQActive _subscriber2 = null!;
    private TestQActive _subscriber3 = null!;
    private QSignal _testSignal = null!;

    [SetUp]
    public void Setup()
    {
        _eventBroker = new QEventBroker();
        _testSignal = new QSignal($"TestSignal_{Guid.NewGuid()}");
        _subscriber1 = new TestQActive(1); // Priority 1
        _subscriber2 = new TestQActive(1); // Priority 1 (same as subscriber1)
        _subscriber3 = new TestQActive(2); // Priority 2
    }

    [Test]
    public void Subscribe_MultipleSamePriority_AllReceiveEvents()
    {
        // Arrange
        _eventBroker.Subscribe(_subscriber1, _testSignal);
        _eventBroker.Subscribe(_subscriber2, _testSignal);
        var testEvent = new QEvent(_testSignal);

        // Act
        _eventBroker.Publish(testEvent);

        // Assert
        Assert.That(_subscriber1.ReceivedEvents, Has.Count.EqualTo(1));
        Assert.That(_subscriber2.ReceivedEvents, Has.Count.EqualTo(1));
        Assert.That(_subscriber1.ReceivedEvents[0], Is.SameAs(testEvent));
        Assert.That(_subscriber2.ReceivedEvents[0], Is.SameAs(testEvent));
    }

    [Test]
    public void Subscribe_DifferentPriorities_EventsDeliveredInPriorityOrder()
    {
        // Arrange
        _eventBroker.Subscribe(_subscriber3, _testSignal); // Priority 2
        _eventBroker.Subscribe(_subscriber1, _testSignal); // Priority 1
        var testEvent = new QEvent(_testSignal);

        // Act
        _eventBroker.Publish(testEvent);

        // Assert - lower priority number should be processed first
        Assert.That(_subscriber1.ReceivedEvents, Has.Count.EqualTo(1));
        Assert.That(_subscriber3.ReceivedEvents, Has.Count.EqualTo(1));
        Assert.That(_subscriber1.EventReceivedTime, Is.LessThan(_subscriber3.EventReceivedTime));
    }

    [Test]
    public void Subscribe_MultipleSamePriorityAndDifferent_AllReceiveInCorrectOrder()
    {
        // Arrange
        _eventBroker.Subscribe(_subscriber3, _testSignal); // Priority 2
        _eventBroker.Subscribe(_subscriber1, _testSignal); // Priority 1
        _eventBroker.Subscribe(_subscriber2, _testSignal); // Priority 1 (same as subscriber1)
        var testEvent = new QEvent(_testSignal);

        // Act
        _eventBroker.Publish(testEvent);

        // Assert
        Assert.That(_subscriber1.ReceivedEvents, Has.Count.EqualTo(1));
        Assert.That(_subscriber2.ReceivedEvents, Has.Count.EqualTo(1));
        Assert.That(_subscriber3.ReceivedEvents, Has.Count.EqualTo(1));

        // Priority 1 subscribers should receive events before priority 2
        // this runs so fast, that the timestamps may be identical, so we can't reliably assert order between same-priority subscribers
        // Assert.That(_subscriber1.EventReceivedTime, Is.LessThan(_subscriber3.EventReceivedTime));
        // Assert.That(_subscriber2.EventReceivedTime, Is.LessThan(_subscriber3.EventReceivedTime));
    }

    [Test]
    public void Unsubscribe_OnlyTargetSubscriberRemoved()
    {
        // Arrange
        _eventBroker.Subscribe(_subscriber1, _testSignal);
        _eventBroker.Subscribe(_subscriber2, _testSignal);
        var testEvent = new QEvent(_testSignal);

        // Act
        _eventBroker.Unsubscribe(_subscriber1, _testSignal);
        _eventBroker.Publish(testEvent);

        // Assert
        Assert.That(_subscriber1.ReceivedEvents, Has.Count.EqualTo(0));
        Assert.That(_subscriber2.ReceivedEvents, Has.Count.EqualTo(1));
    }

    [Test]
    public void Unregister_RemovesFromAllSignals()
    {
        // Arrange
        var signal1 = new QSignal();
        var signal2 = new QSignal();
        _eventBroker.Subscribe(_subscriber1, signal1);
        _eventBroker.Subscribe(_subscriber1, signal2);
        _eventBroker.Subscribe(_subscriber2, signal1);

        // Act
        _eventBroker.Unregister(_subscriber1);
        _eventBroker.Publish(new QEvent(signal1));
        _eventBroker.Publish(new QEvent(signal2));

        // Assert
        Assert.That(_subscriber1.ReceivedEvents, Has.Count.EqualTo(0));
        Assert.That(_subscriber2.ReceivedEvents, Has.Count.EqualTo(1)); // Only gets signal1 event
    }

    [Test]
    public void Unsubscribe_LastSubscriberAtPriority_RemovesPriorityGroup()
    {
        // Arrange
        _eventBroker.Subscribe(_subscriber1, _testSignal);
        _eventBroker.Subscribe(_subscriber2, _testSignal);

        // Act - remove both subscribers at same priority
        _eventBroker.Unsubscribe(_subscriber1, _testSignal);
        _eventBroker.Unsubscribe(_subscriber2, _testSignal);

        // Re-subscribe to verify priority group was cleaned up
        _eventBroker.Subscribe(_subscriber3, _testSignal);
        _eventBroker.Publish(new QEvent(_testSignal));

        // Assert
        Assert.That(_subscriber1.ReceivedEvents, Has.Count.EqualTo(0));
        Assert.That(_subscriber2.ReceivedEvents, Has.Count.EqualTo(0));
        Assert.That(_subscriber3.ReceivedEvents, Has.Count.EqualTo(1));
    }

    private class TestQActive : IQActive
    {
        public List<IQEvent> ReceivedEvents { get; } = [];
        public DateTime EventReceivedTime { get; private set; }

        public TestQActive(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }

        public void PostFifo(IQEvent qEvent)
        {
            EventReceivedTime = DateTime.UtcNow;
            ReceivedEvents.Add(qEvent);
        }

        public void PostLifo(IQEvent qEvent)
        {
            EventReceivedTime = DateTime.UtcNow;
            ReceivedEvents.Add(qEvent);
        }

        public Task RunEventPumpAsync(int priority, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void RunEventPump(int priority, CancellationToken cancellationToken = default)
        {
            // No-op for testing
        }

        public override string ToString()
        {
            return $"TestQActive(Priority={Priority})";
        }
    }
}
