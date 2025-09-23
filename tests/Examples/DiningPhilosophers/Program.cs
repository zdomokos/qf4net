using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using qf4net;

namespace DiningPhilosophers;

internal class Program
{
    private const int NumberOfPhilosophers = 5;

    [STAThread]
    private static async Task Main(string[] args)
    {
        List<Task> qtasks = [];

        IQEventPump table = new Table(NumberOfPhilosophers);
        IQEventPump[] philosophers = new IQEventPump[NumberOfPhilosophers];

        for (var i = 0; i < NumberOfPhilosophers; i++)
        {
            philosophers[i] = new Philosopher(i);
        }

        Console.WriteLine($"{NumberOfPhilosophers} philosophers gather around a table thinking ...");
        var t = table.RunEventPumpAsync(NumberOfPhilosophers);
        qtasks.Add(t);
        for (var i = 0; i < NumberOfPhilosophers; i++)
        {
            t = philosophers[i].RunEventPumpAsync(i);
            qtasks.Add(t);
        }

        Console.WriteLine("Running for 10 seconds...");
        Task.Delay(TimeSpan.FromSeconds(10)).Wait();

        // Stop all philosophers and the table
        for (var i = 0; i < NumberOfPhilosophers; i++)
        {
            philosophers[i].PostFifo(new QEvent(QSignals.Terminate));
        }
        table.PostFifo(new QEvent(QSignals.Terminate));

        await Task.WhenAll(qtasks);
        Console.WriteLine("Simulation ended.");
    }
}
