// using System;
// using System.Threading.Tasks;
// using qf4net;
//
// namespace DiningPhilosophers;
//
// /// <summary>
// /// Simple test to demonstrate QStateMachine functionality
// /// </summary>
// public class SimpleStateMachine : QStateMachine
// {
//     private readonly QState _idle;
//     private readonly QState _working;
//
//     public SimpleStateMachine() : base(Console.WriteLine)
//     {
//         _idle = IdleState;
//         _working = WorkingState;
//     }
//
//     public void Initialize()
//     {
//         InitializeState(_idle);
//     }
//
//     private QState IdleState(IQEvent qEvent)
//     {
//         if (qEvent.IsSignal(QSignals.Entry))
//         {
//             Console.WriteLine("Entering Idle state");
//             return null;
//         }
//
//         if (qEvent.IsSignal(QSignals.Exit))
//         {
//             Console.WriteLine("Exiting Idle state");
//             return null;
//         }
//
//         if (qEvent.IsSignal(DPPSignal.Hungry)) // Reusing existing signal
//         {
//             Console.WriteLine("Got work signal, transitioning to Working");
//             TransitionTo(_working);
//             return null;
//         }
//
//         return null;
//     }
//
//     private QState WorkingState(IQEvent qEvent)
//     {
//         if (qEvent.IsSignal(QSignals.Entry))
//         {
//             Console.WriteLine("Entering Working state");
//             return null;
//         }
//
//         if (qEvent.IsSignal(QSignals.Exit))
//         {
//             Console.WriteLine("Exiting Working state");
//             return null;
//         }
//
//         if (qEvent.IsSignal(DPPSignal.Done))
//         {
//             Console.WriteLine("Work completed, transitioning to Idle");
//             TransitionTo(_idle);
//             return null;
//         }
//
//         return null;
//     }
//
//     public static async Task TestQStateMachine()
//     {
//         Console.WriteLine("\n--- Testing QStateMachine ---");
//
//         var sm = new SimpleStateMachine();
//         sm.Initialize();
//
//         // Test async event pump
//         var task = sm.RunEventPumpAsync(0);
//
//         // Send some events
//         await Task.Delay(100);
//         sm.PostFifo(new QEvent(DPPSignal.Hungry));
//
//         await Task.Delay(100);
//         sm.PostFifo(new QEvent(DPPSignal.Done));
//
//         await Task.Delay(100);
//         sm.PostFifo(new QEvent(QSignals.Terminate));
//
//         await task;
//         Console.WriteLine("QStateMachine test completed!");
//     }
// }
