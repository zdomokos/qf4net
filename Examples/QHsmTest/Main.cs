using System;
using qf4net;

namespace QHsmTest
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Which state machine should be used? Enter 1 or 2."); 
			Console.WriteLine("1: QHsmTest, 2: QHsmTestDerived (this state machine derives from QHsmTest)");
			string choice = Console.ReadLine();
			if (choice == "1")
			{
				RunStateMachine(new QHsmTest());
			} 
			else if (choice == "2")
			{
				RunStateMachine(new QHsmTestDerived());
			}
		}

		private static void RunStateMachine(QHsmTest test)
		{
			test.Init(); // take the initial transition
			for (;;)
			{
				Console.Write("\nSignal<-");
				int i = Console.Read();
				Console.ReadLine();
				if (i == -1)
				{
					break;
				}
				else
				{
					if (i < (int)'a' || i > (int)'h')
					{
						break;
					}
					else
					{
						test.Dispatch(new QEvent((ushort)(i - (int)'a' + (int)MyQSignals.A_Sig)));
					}
				}
			}
		}
	}
}
