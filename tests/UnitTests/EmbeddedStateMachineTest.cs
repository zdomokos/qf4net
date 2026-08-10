using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using qf4net;

namespace qf4net.UnitTests;

[TestFixture]
public class EmbeddedStateMachineTest
{
    private QSignal _sig1;
    private QSignal _sig2;

    [SetUp]
    public void SetUp()
    {
        _sig1 = new QSignal(1);
        _sig2 = new QSignal(2);
    }

    [Test]
    public void QSignal_CreatesDistinctIdentifiers()
    {
        var sigA = new QSignal(1);
        var sigB = new QSignal(2);
        Assert.That(sigA, Is.Not.EqualTo(sigB));
        Assert.That(sigA.GetHashCode(), Is.Not.EqualTo(sigB.GetHashCode()));
    }

    [Test]
    public void QSignal_SameIdentifier_Matches()
    {
        var sig1 = new QSignal(1);
        var sig2 = new QSignal(1);
        Assert.That(sig1, Is.EqualTo(sig2));
        Assert.That(sig1.GetHashCode(), Is.EqualTo(sig2.GetHashCode()));
    }

    [Test]
    public void QEvent_IsSignal_ValidatesCorrectly()
    {
        var sig = new QSignal(42);
        var evt = new QEvent(sig);
        Assert.That(evt.IsSignal(sig), Is.True);
        Assert.That(evt.IsSignal(new QSignal(99)), Is.False);
    }

    [Test]
    public void IQActive_PostFifo_PreservesOrder()
    {
        var machine = new TestStateMachine();
        machine.PostFifo(new QEvent(_sig1));
        machine.PostFifo(new QEvent(_sig2));

        Assert.That(machine.ReceivedEvents.Count, Is.EqualTo(2));
        Assert.That(machine.ReceivedEvents[0].Signal, Is.SameAs(_sig1));
        Assert.That(machine.ReceivedEvents[1].Signal, Is.SameAs(_sig2));
    }

    [Test]
    public void IQActive_PostLifo_ReversesOrder()
    {
        var machine = new TestStateMachine();
        machine.PostLifo(new QEvent(_sig1));
        machine.PostLifo(new QEvent(_sig2));

        Assert.That(machine.ReceivedEvents.Count, Is.EqualTo(2));
        Assert.That(machine.ReceivedEvents[0].Signal, Is.SameAs(_sig2));
        Assert.That(machine.ReceivedEvents[1].Signal, Is.SameAs(_sig1));
    }

    private class TestStateMachine : IQActive
    {
        public List<IQEvent> ReceivedEvents { get; } = new();
        public int Priority { get; } = 1;

        public void PostFifo(IQEvent qEvent) => ReceivedEvents.Add(qEvent);
        public void PostLifo(IQEvent qEvent) => ReceivedEvents.Add(qEvent);
        public Task RunEventPumpAsync(int priority, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void RunEventPump(int priority, CancellationToken cancellationToken = default) { }
    }
}
