// -----------------------------------------------------------------------------
// Run to Completion Example for Rainer Hessmer's C# port of
// Samek's Quantum Hierarchical State Machine.
//
// Author: David Shields (david@shields.net)
// 
// References:
// Practical Statecharts in C/C++; Quantum Programming for Embedded Systems
// Author: Miro Samek, Ph.D.
// http://www.quantum-leaps.com/book.htm
//
// Rainer Hessmer, Ph.D. (rainer@hessmer.org)
// http://www.hessmer.org/dev/qhsm/
// -----------------------------------------------------------------------------

using System;
using qf4net;

namespace RunToCompletionHsm
{

	/// <summary>
	/// RTC state machine example for Rainer Hessmer's C# port of HQSM
	/// 
	/// The purpose of this example is to answer the question:
	/// "Is it possible to implement a run to completion statechart that 
	/// allows the user to abort long actions prior to their completion?"
	/// 
	/// In this example we assume the operations must be done in sequence, but the user
	/// may want to stop temporarily to do other work and then resume this sequence
	/// without going all the way back to the beginning.
	/// 
	/// This example is not compatible with static transitions and several shortcuts are taken
	/// with regard to communication between threads and the threading immplementation in general.
	/// </summary>
	public sealed class RunToCompletion : QHsm
	{
		//communication with main form is via this event:
		public delegate void RtcDisplayHandler(object sender, RtcDisplayEventArgs e);
		public event RtcDisplayHandler DisplayState;
		
		public bool IsHandled
		{
			get{return isHandled;}
			set{isHandled = value;}
		}//IsHandled
		private bool isHandled;
		
		/// <summary>
		/// This value merely sets to loop termination for simulated "long operations"
		/// </summary>
		public int BigValue
		{
			set {bigValue = value;} 
		}
		private int bigValue = 100000000;
		
		
		private QState Dispatching;
			private QState CantInterrupt;//step 0 (very first work to be done -- assume we can't interrupt it)
			private QState Interruptible;//superstate for SlowOne & SlowTwo (doesn't include much functionality in this example)
				private QState SlowOne; //step 1 -- part 1 of long op that user may want to cancel early
				private QState SlowTwo; //step 2 -- part 2 of long op that user may also want to cancel
			private QState Completed;	//all finished with parts 0, 1 and 2
		private QState Final;

		//
		//holds a reference to another delegate
		//this is the state to continue with after an abort
		//
		//This style is not compatible with static transitions (see pg 126, Section 4.6 of Samek 2002)
		private QState continueWithState;
		//
		//
		
		private QState DoDispatching(IQEvent qevent)
		{
            if (qevent.IsSignal(QSignals.Entry))
            {
                OnDisplayState("Dispatching State");
                return null;
            }
            if (qevent.IsSignal(RtcSignals.Start))
            {
                TransitionTo(continueWithState);
                isHandled = false;
                return null;
            }
			return this.TopState;
		}


		private QState DoCantInterrupt(IQEvent qevent)
		{
            if (qevent.IsSignal(QSignals.Entry))
            {
                OnDisplayState("CantInterrupt");
                return null;
            }
            if (qevent.IsSignal(RtcSignals.Start))
            {
                bool completedOK = DoSomeUninterruptibleWork();
                if (completedOK)
                {
                    continueWithState = SlowOne;
                }
                TransitionTo(SlowOne);
                return null;
            }

			return TopState;
		}


		private QState DoSlowOne(IQEvent qevent)
		{
            if (qevent.IsSignal(QSignals.Entry))
            {
                OnDisplayState("SlowOne");
                return null;
            }
            if (qevent.IsSignal(RtcSignals.Start))
            {
                bool completedOK1 = DoSomeInterruptibleWork1();
                if (completedOK1)
                {
                    continueWithState = SlowTwo;
                    TransitionTo(SlowTwo);
                }
                else
                {
                    TransitionTo(Dispatching);
                }
                return null;
            }
			return Interruptible;
		}


		private QState DoSlowTwo(IQEvent qevent)
		{
            if (qevent.IsSignal(QSignals.Entry))
            {
                OnDisplayState("SlowTwo");
                return null;
            }
            if (qevent.IsSignal(RtcSignals.Start))
            {
                bool completedOK2 = DoSomeInterruptibleWork2();
                if (completedOK2)
                {
                    TransitionTo(Completed);
                }
                else
                {
                    TransitionTo(Dispatching);
                }
                return null;
            }
            return Interruptible;
		}

		private QState DoInterruptible(IQEvent qevent)
		{
            if (qevent.IsSignal(QSignals.Entry))
            {
				OnDisplayState("Interruptible");
				return null;
            }
            if (qevent.IsSignal(RtcSignals.Abort))//this isn't actually used in this example
            {
				SendAbortSignal();
				TransitionTo(Dispatching);
				return null;
            }
			return TopState;
		}


		private QState DoCompleted(IQEvent qevent)
        {
            if (qevent.IsSignal(QSignals.Entry))
            {
				OnDisplayState("Completed");
				continueWithState = CantInterrupt;					
				return null;
            }
            if (qevent.IsSignal(RtcSignals.Start))
            {
				TransitionTo(CantInterrupt);
				return null;
            }
			return TopState;
		}


		//UNDONE: revise this code
		private QState DoFinal(IQEvent qevent)
		{
            if (qevent.IsSignal(QSignals.Entry))
            {
				OnDisplayState("HSM terminated");
				singleton = null;
				MainForm.Instance.Close();
				System.Windows.Forms.Application.Exit();
				return null;
            }
			return this.TopState;
		}


		private void OnDisplayState(string stateInfo)
		{
			if (DisplayState != null)
			{
				DisplayState(this, new RtcDisplayEventArgs(stateInfo));
			}
		}//OnDisplayState


		private bool DoSomeUninterruptibleWork()
		{
			// do lots of looping
			for (int i = 0; i < bigValue; ++i)
			{
				double y = (double)(i + i);
			}
			return true;
		}//DoSomeUninterruptibleWork

		private bool DoSomeInterruptibleWork1()
		{
			bool isAborted = Abort.Status;

			for (int i = 0; i < bigValue; ++i)
			{
				if (Abort.Status) 
				{
					isAborted = true;
					OnDisplayState("<<Aborted>>");
					break;
				}
				double y = (double)(i * i * i);
			}

			return !isAborted;
		}//DoSomeInterruptibleWork1

		private bool DoSomeInterruptibleWork2()
		{
			bool isAborted = Abort.Status;

			for (int i = 0; i < bigValue; ++i)
			{
				if (Abort.Status) 
				{
					isAborted = true;
					OnDisplayState("<<Aborted>>");
					break;
				}
				double y = (double)(i * i * i);
			}

			return !isAborted;

		}//DoSomeInterruptibleWork2


		private void SendAbortSignal()//not used
		{
			Abort.Status = true;
		}//SendAbortSignal


		/// <summary>
		/// Is called inside of the function Init to give the deriving class a chance to
		/// initialize the state machine.
		/// </summary>
		protected override void InitializeStateMachine()
		{
			continueWithState = CantInterrupt;
			InitializeState(Dispatching); // initial transition			
		}

		private RunToCompletion()
		{
			Dispatching = new QState(this.DoDispatching);
			CantInterrupt = new QState(this.DoCantInterrupt);
			Interruptible = new QState(this.DoInterruptible);
				SlowOne = new QState(this.DoSlowOne);
				SlowTwo = new QState(this.DoSlowTwo);
			Completed = new QState(this.DoCompleted);
			Final = new QState(this.DoFinal);
		}


		//
		//Thread-safe implementation of singleton as a property
		//
		private static volatile RunToCompletion singleton = null;
		private static object sync = new object();//for static lock

		public static RunToCompletion Instance
		{
			get
			{
				if (singleton == null)
				{
					lock (sync)
					{
						if (singleton == null)
						{
							singleton = new RunToCompletion();	
							singleton.Init();
						}
					}
				}
			
				return singleton;
			}
		}//Instance


	}//class RunToCompletion

	public class RtcDisplayEventArgs : EventArgs
	{
		private string s;
		public string Message { get { return s; } }

		public RtcDisplayEventArgs(string message) { s = message;}
	}
}//namespace
