using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using qf4net;

namespace DiningPhilosophersClassic;

internal class Program
{
    private const int NumberOfPhilosophers = 5;

    [STAThread]
    private static async Task Main(string[] args)
    {
        var        eventBroker = new QEventBroker();
        List<Task> qtasks = [];

        // create the table and philosophers
        IQActive   table        = new Table(NumberOfPhilosophers);
        IQActive[] philosophers = new IQActive[NumberOfPhilosophers];

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



        Console.WriteLine("---  Running ...");
        Task.Delay(TimeSpan.FromSeconds(60)).Wait();

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
