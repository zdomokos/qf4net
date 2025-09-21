using System;
using qf4net;

namespace OptimizationBreaker
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(
                "Which pair (QHsmBaseX / QHsmDerivedX) do you want to run? Enter 1, 2 or 3."
            );
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    TestStateMachines(1);
                    break;
                case "2":
                    TestStateMachines(2);
                    break;
                case "3":
                    TestStateMachines(3);
                    break;
            }
            Console.ReadLine();
        }

        private static void TestStateMachines(int pairType)
        {
            Console.WriteLine(
                "Creating instance of QHsmBase{0} and sending signals to it:",
                pairType
            );
            QHsm qhsm = CreateQHsmBase(pairType);
            qhsm.Init(); // take the initial transition
            Console.WriteLine();

            Console.WriteLine("Exercising all transitions once -> they will be recorded.");

            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig1));
            Console.WriteLine();
            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig2));
            Console.WriteLine();

            Console.WriteLine(
                "Exercising the transitions a second time -> playing back recorded steps."
            );
            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig1));
            Console.WriteLine();
            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig2));
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine(
                "Creating instance of QHsmDerived{0} and sending signals to it:",
                pairType
            );
            qhsm = CreateQHsmDerived(pairType);
            qhsm.Init(); // take the initial transition
            Console.WriteLine();
            Console.WriteLine("Exercising all transitions once -> they will be recorded.");
            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig1));
            Console.WriteLine();
            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig2));
            Console.WriteLine();
            Console.WriteLine(
                "Exercising the transitions a second time -> playing back recorded steps."
            );
            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig1));
            Console.WriteLine();
            qhsm.Dispatch(new QEvent((int)MyQSignals.Sig2));
            Console.WriteLine();
        }

        private static QHsm CreateQHsmBase(int pairType)
        {
            switch (pairType)
            {
                case 1:
                    return new QHsmBase1();
                case 2:
                    return new QHsmBase2();
                case 3:
                    return new QHsmBase3();
            }
            return null;
        }

        private static QHsm CreateQHsmDerived(int pairType)
        {
            switch (pairType)
            {
                case 1:
                    return new QHsmDerived1();
                case 2:
                    return new QHsmDerived2();
                case 3:
                    return new QHsmDerived3();
            }
            return null;
        }
    }
}
