using System;
using System.Threading.Tasks;
using qf4net;

namespace DiningPhilosophers;

/// <summary>
/// Summary description for Class1.
/// </summary>
internal class Class1
{
    private const int c_NumberOfPhilosophers = 5;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        IQActive table = new Table(c_NumberOfPhilosophers);
        IQActive[] philosophers = [];

        for (var i = 0; i < c_NumberOfPhilosophers; i++)
        {
            philosophers[i] = new Philosopher(i);
        }

        Console.WriteLine($"{c_NumberOfPhilosophers} philosophers gather around a table thinking ...");
        table.StartAsync(c_NumberOfPhilosophers);
        for (var i = 0; i < c_NumberOfPhilosophers; i++)
        {
            philosophers[i].StartAsync(i);
        }

        // Keep the application running
        Console.WriteLine("Running for 30 seconds...");
        Task.Delay(30000).Wait();
    }
}
