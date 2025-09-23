// // -----------------------------------------------------------------------------
// // Run to Completion Example for Rainer Hessmer's C# port of
// // Samek's Quantum Hierarchical State Machine.
// //
// // Author: David Shields (david@shields.net)
// //
// // References:
// // Practical Statecharts in C/C++; Quantum Programming for Embedded Systems
// // Author: Miro Samek, Ph.D.
// // http://www.quantum-leaps.com/book.htm
// //
// // Rainer Hessmer, Ph.D. (rainer@hessmer.org)
// // http://www.hessmer.org/dev/qhsm/
// // -----------------------------------------------------------------------------
//
// using System;
// using qf4net;
//
// namespace RunToCompletionHsm;
//
// /// <summary>
// /// RTC state machine example for Rainer Hessmer's C# port of HQSM
// ///
// /// The purpose of this example is to answer the question:
// /// "Is it possible to implement a run to completion statechart that
// /// allows the user to abort long actions prior to their completion?"
// ///
// /// In this example we assume the operations must be done in sequence, but the user
// /// may want to stop temporarily to do other work and then resume this sequence
// /// without going all the way back to the beginning.
// ///
// /// This example is not compatible with static transitions and several shortcuts are taken
// /// with regard to communication between threads and the threading immplementation in general.
// /// </summary>
// public sealed class RunToCompletion : QHsm
// {
//     //communication with main form is via this event:
//     public delegate void           RtcDisplayHandler(object sender, RtcDisplayEventArgs e);
//     public event RtcDisplayHandler DisplayState;
//
//     public  bool IsHandled { get; set; } //IsHandled
//
//     /// <summary>
//     /// This value merely sets to loop termination for simulated "long operations"
//     /// </summary>
//     public int BigValue
//     {
//         set { bigValue = value; }
//     }
//     private int bigValue = 100000000;
//
//     private readonly QState Dispatching;
//     private readonly QState CantInterrupt; //step 0 (very first work to be done -- assume we can't interrupt it)
//     private readonly QState Interruptible; //superstate for SlowOne & SlowTwo (doesn't include much functionality in this example)
//     private readonly QState SlowOne;       //step 1 -- part 1 of long op that user may want to cancel early
//     private readonly QState SlowTwo;       //step 2 -- part 2 of long op that user may also want to cancel
//     private readonly QState Completed;     //all finished with parts 0, 1 and 2
//     private          QState Final;
//
//     //
//     //holds a reference to another delegate
//     //this is the state to continue with after an abort
//     //
//     //This style is not compatible with static transitions (see pg 126, Section 4.6 of Samek 2002)
//     private QState continueWithState;
//
//     //
//     //
//
//     private QState DoDispatching(IQEvent qevent)
//     {
//         if (qevent.IsSignal(QSignals.Entry))
//         {
//             OnDisplayState("Dispatching State");
//             return null;
//         }
//         if (qevent.IsSignal(RtcSignals.Start))
//         {
//             TransitionTo(continueWithState);
//             IsHandled = false;
//             return null;
//         }
//         return TopState;
//     }
//
//     private QState DoCantInterrupt(IQEvent qevent)
//     {
//         if (qevent.IsSignal(QSignals.Entry))
//         {
//             OnDisplayState("CantInterrupt");
//             return null;
//         }
//         if (qevent.IsSignal(RtcSignals.Start))
//         {
//             var completedOK = DoSomeUninterruptibleWork();
//             if (completedOK)
//             {
//                 continueWithState = SlowOne;
//             }
//             TransitionTo(SlowOne);
//             return null;
//         }
//
//         return TopState;
//     }
//
//     private QState DoSlowOne(IQEvent qevent)
//     {
//         if (qevent.IsSignal(QSignals.Entry))
//         {
//             OnDisplayState("SlowOne");
//             return null;
//         }
//         if (qevent.IsSignal(RtcSignals.Start))
//         {
//             var completedOK1 = DoSomeInterruptibleWork1();
//             if (completedOK1)
//             {
//                 continueWithState = SlowTwo;
//                 TransitionTo(SlowTwo);
//             }
//             else
//             {
//                 TransitionTo(Dispatching);
//             }
//             return null;
//         }
//         return Interruptible;
//     }
//
//     private QState DoSlowTwo(IQEvent qevent)
//     {
//         if (qevent.IsSignal(QSignals.Entry))
//         {
//             OnDisplayState("SlowTwo");
//             return null;
//         }
//         if (qevent.IsSignal(RtcSignals.Start))
//         {
//             var completedOK2 = DoSomeInterruptibleWork2();
//             if (completedOK2)
//             {
//                 TransitionTo(Completed);
//             }
//             else
//             {
//                 TransitionTo(Dispatching);
//             }
//             return null;
//         }
//         return Interruptible;
//     }
//
//     private QState DoInterruptible(IQEvent qevent)
//     {
//         if (qevent.IsSignal(QSignals.Entry))
//         {
//             OnDisplayState("Interruptible");
//             return null;
//         }
//         if (qevent.IsSignal(RtcSignals.Abort)) //this isn't actually used in this example
//         {
//             SendAbortSignal();
//             TransitionTo(Dispatching);
//             return null;
//         }
//         return TopState;
//     }
//
//     private QState DoCompleted(IQEvent qevent)
//     {
//         if (qevent.IsSignal(QSignals.Entry))
//         {
//             OnDisplayState("Completed");
//             continueWithState = CantInterrupt;
//             return null;
//         }
//         if (qevent.IsSignal(RtcSignals.Start))
//         {
//             TransitionTo(CantInterrupt);
//             return null;
//         }
//         return TopState;
//     }
//
//     //UNDONE: revise this code
//     private QState DoFinal(IQEvent qevent)
//     {
//         if (qevent.IsSignal(QSignals.Entry))
//         {
//             OnDisplayState("HSM terminated");
//             singleton = null;
//             MainForm.Instance.Close();
//             System.Windows.Forms.Application.Exit();
//             return null;
//         }
//         return TopState;
//     }
//
//     private void OnDisplayState(string stateInfo)
//     {
//         if (DisplayState != null)
//         {
//             DisplayState(this, new RtcDisplayEventArgs(stateInfo));
//         }
//     } //OnDisplayState
//
//     private bool DoSomeUninterruptibleWork()
//     {
//         // do lots of looping
//         for (var i = 0; i < bigValue; ++i)
//         {
//             double y = i + i;
//         }
//         return true;
//     } //DoSomeUninterruptibleWork
//
//     private bool DoSomeInterruptibleWork1()
//     {
//         var isAborted = Abort.Status;
//
//         for (var i = 0; i < bigValue; ++i)
//         {
//             if (Abort.Status)
//             {
//                 isAborted = true;
//                 OnDisplayState("<<Aborted>>");
//                 break;
//             }
//             double y = i * i * i;
//         }
//
//         return !isAborted;
//     } //DoSomeInterruptibleWork1
//
//     private bool DoSomeInterruptibleWork2()
//     {
//         var isAborted = Abort.Status;
//
//         for (var i = 0; i < bigValue; ++i)
//         {
//             if (Abort.Status)
//             {
//                 isAborted = true;
//                 OnDisplayState("<<Aborted>>");
//                 break;
//             }
//             double y = i * i * i;
//         }
//
//         return !isAborted;
//     } //DoSomeInterruptibleWork2
//
//     private void SendAbortSignal() //not used
//     {
//         Abort.Status = true;
//     } //SendAbortSignal
//
//     /// <summary>
//     /// Is called inside the function Init to give the deriving class a chance to
//     /// initialize the state machine.
//     /// </summary>
//     protected override void InitializeStateMachine()
//     {
//         continueWithState = CantInterrupt;
//         InitializeState(Dispatching); // initial transition
//     }
//
//     private RunToCompletion()
//     {
//         Dispatching   = DoDispatching;
//         CantInterrupt = DoCantInterrupt;
//         Interruptible = DoInterruptible;
//         SlowOne       = DoSlowOne;
//         SlowTwo       = DoSlowTwo;
//         Completed     = DoCompleted;
//         Final         = DoFinal;
//     }
//
//     //
//     //Thread-safe implementation of singleton as a property
//     //
//     private static volatile RunToCompletion singleton = null;
//     private static readonly object          sync      = new(); //for static lock
//
//     public static RunToCompletion Instance
//     {
//         get
//         {
//             if (singleton == null)
//             {
//                 lock (sync)
//                 {
//                     if (singleton == null)
//                     {
//                         singleton = new RunToCompletion();
//                         singleton.Init();
//                     }
//                 }
//             }
//
//             return singleton;
//         }
//     }
// }
//
// public class RtcDisplayEventArgs : EventArgs
// {
//     public           string Message { get; }
//
//     public RtcDisplayEventArgs(string message)
//     {
//         Message = message;
//     }
// }
