using System;
using qf4net;

namespace QHsmTest;

internal class MainClass
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Which state machine should be used? Enter 1 or 2.");
        Console.WriteLine("1: QHsmTest, 2: QHsmTestDerived (this state machine derives from QHsmTest)");
        var choice = Console.ReadLine();

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
            var i = Console.Read();
            Console.ReadLine();
            if (i == -1)
            {
                break;
            }

            if (i is < 'a' or > 'h')
            {
                break;
            }

            test.Dispatch(new QEvent(MyQSignals.A_Sig));
        }
    }
}
