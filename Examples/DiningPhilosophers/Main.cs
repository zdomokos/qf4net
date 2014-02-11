using System;
using qf4net;

namespace DiningPhilosophers
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		private const int c_NumberOfPhilosophers = 5;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            DPPSignal _signals = new DPPSignal();
            QF.Instance.Initialize(Signal.MaxSignalCount);

			IQActive table = new Table(c_NumberOfPhilosophers);
			IQActive[] philosophers = new IQActive[c_NumberOfPhilosophers];

			for(int i = 0; i < c_NumberOfPhilosophers; i++)
			{
				philosophers[i] = new Philosopher(i);
			}

			Console.WriteLine(c_NumberOfPhilosophers + " philosophers gather around a table thinking ...");
			table.Start(c_NumberOfPhilosophers);
			for(int i = 0; i < c_NumberOfPhilosophers; i++)
			{
				philosophers[i].Start(i);
			}
		}
	}
}
