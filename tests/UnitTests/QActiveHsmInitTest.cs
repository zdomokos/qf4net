using NUnit.Framework;
using qf4net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests;

/// <summary>
/// With <see cref="StatemachineConfig.SendStateJobAfterEntry"/>, the initial state's StateJob must reach
/// the initial state — also when events were posted before the event pump started, which is the usual
/// case: callers create the active object, start its pump on another thread and post to it at once.
/// The StateJob used to be appended behind those events, so the initial state never saw it and it
/// landed in whatever state the earlier events had moved the machine to.
/// </summary>
[TestFixture]
public class QActiveHsmInitTest
{
    private static readonly QSignal Go = new();

    private sealed class TwoStateHsm : QActiveHsm
    {
        public TwoStateHsm() : base(null, new StatemachineConfig { SendStateJobAfterEntry = true }) { }

        public List<string> StateJobs { get; } = [];
        public ManualResetEventSlim InSecond { get; } = new();

        protected override void InitializeStateMachine() => InitializeState(First);

        private QState First(IQEvent e)
        {
            if (e.IsSignal(QSignals.StateJob))
            {
                lock (StateJobs) StateJobs.Add(nameof(First));
                return null;
            }

            if (e.IsSignal(Go))
            {
                TransitionTo(Second);
                return null;
            }

            return TopState;
        }

        private QState Second(IQEvent e)
        {
            if (e.IsSignal(QSignals.Entry))
            {
                InSecond.Set();
                return null;
            }

            if (e.IsSignal(QSignals.StateJob))
            {
                lock (StateJobs) StateJobs.Add(nameof(Second));
                return null;
            }

            return TopState;
        }
    }

    [Test]
    public async Task InitialStateJob_ReachesInitialState_EvenWhenEventsWerePostedBeforeThePumpStarted()
    {
        var hsm = new TwoStateHsm();
        hsm.PostFifo(new QEvent(Go)); // before the pump runs Init

        using var cts = new CancellationTokenSource();
        var pump = hsm.RunEventPumpAsync(0, cts.Token);

        Assert.That(hsm.InSecond.Wait(TimeSpan.FromSeconds(2)), "The posted event should move the machine on");
        await Task.Delay(100); // let anything still queued be dispatched

        lock (hsm.StateJobs)
            Assert.That(hsm.StateJobs, Is.EqualTo(new[] { "First" }),
                        "The initial StateJob belongs to the initial state, and only there");

        cts.Cancel();
        try { await pump; } catch (OperationCanceledException) { }
    }
}
