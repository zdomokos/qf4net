using System;
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
        var philosophers = new IQActive[c_NumberOfPhilosophers];

        for (var i = 0; i < c_NumberOfPhilosophers; i++)
        {
            philosophers[i] = new Philosopher(i);
        }

        Console.WriteLine(c_NumberOfPhilosophers + " philosophers gather around a table thinking ...");
        table.Start(c_NumberOfPhilosophers);
        for (var i = 0; i < c_NumberOfPhilosophers; i++)
        {
            philosophers[i].Start(i);
        }
    }
}
