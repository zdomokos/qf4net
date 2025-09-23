using System;
using System.Threading;
using System.Threading.Tasks;
using qf4net;

namespace DiningPhilosophers;

/// <summary>
/// Simple test to verify cancellation works properly
/// </summary>
public class TestQActive : QActive
{
    protected override void InitializeStateMachine()
    {
        InitializeState(Working);
    }

    private QState Working(IQEvent qEvent)
    {
        if (qEvent.IsSignal(QSignals.Entry))
        {
            Console.WriteLine("TestQActive started and working...");
            return null;
        }
        return TopState;
    }

    protected override void HsmUnhandledException(Exception e)
    {
        Console.WriteLine($"Exception: {e.Message}");
    }

    public static async Task TestCancellation()
    {
        Console.WriteLine("Testing cancellation...");

        var testActive = new TestQActive();
        testActive.StartAsync(0);

        // Let it run for a bit
        await Task.Delay(1000);

        // Test cancellation by calling Abort
        Console.WriteLine("Calling Abort...");
        testActive.Abort();

        // Wait for it to finish
        testActive.Join();

        Console.WriteLine("Cancellation test completed successfully!");
    }
}
