using System;
using System.Diagnostics;
using System.Threading;
using qf4net;

namespace DiningPhilosophers
{
	/// <summary>
	/// The active object that represents the table
	/// </summary>
	public class Philosopher : QActive
	{
		private readonly TimeSpan c_ThinkTime = new TimeSpan(0, 0, 7); // last parameter represents seconds
		private readonly TimeSpan c_EatTime = new TimeSpan(0, 0, 5); // last parameter represents seconds

		private QTimer m_Timer;
		private int m_PhilosopherId;

		private QState m_StateThinking;
		private QState m_StateHungry;
		private QState m_StateEating;
		
		public Philosopher(int philosopherId)
		{
			m_PhilosopherId= philosopherId;

			m_StateThinking = new QState(this.Thinking);
			m_StateHungry = new QState(this.Hungry);
			m_StateEating = new QState(this.Eating);

			m_Timer = new QTimer(this);
		}

		/// <summary>
		/// Is called inside of the function Init to give the deriving class a chance to
		/// initialize the state machine.
		/// </summary>
		protected override void InitializeStateMachine()
		{
			Thread.CurrentThread.Name = this.ToString();
			LogMessage(String.Format("Initializing philosopher {0}.", m_PhilosopherId));
			Qf.Instance.Subscribe(this, DPPSignal.Eat);
			InitializeState(m_StateThinking); // initial transition
		}

		private static TransitionChain s_Tran_Thinking_Hungry;
		private QState Thinking(IQEvent qEvent)
		{
			if (qEvent.IsSignal(QSignals.Entry))
            {
                LogMessage(String.Format("Philosopher {0} is thinking.", m_PhilosopherId));
                m_Timer.FireIn(c_ThinkTime, new PhilosopherEvent(DPPSignal.Timeout));
                return null;
            }

			if (qEvent.IsSignal(DPPSignal.Timeout))
            {
                TransitionTo(m_StateHungry, ref s_Tran_Thinking_Hungry);
                return null;
            }

			if (qEvent.IsSignal(QSignals.Exit))
            {
                LogMessage(String.Format("Philosopher {0} is exiting thinking state.", m_PhilosopherId));
                return null;
            }
			return this.TopState;
		}

		private static TransitionChain s_Tran_Hungry_Eating;
		private QState Hungry(IQEvent qEvent)
		{
			if (qEvent.IsSignal(QSignals.Entry))
            {
                LogMessage(String.Format("Philosopher {0} is hungry.", m_PhilosopherId));
                TableEvent tableEvent = new TableEvent(DPPSignal.Hungry, m_PhilosopherId); 
                LogMessage(String.Format("Philosopher {0} publishes Hungry event.", m_PhilosopherId));
                Qf.Instance.Publish(tableEvent);
                return null;
            }

			if (qEvent.IsSignal(DPPSignal.Eat))
            {
                if (((TableEvent)qEvent).PhilosopherId == m_PhilosopherId)
                {
                    LogMessage(String.Format("Philosopher {0} receives eat signal.", m_PhilosopherId));
                    TransitionTo(m_StateEating, ref s_Tran_Hungry_Eating);
                }
                return null;
            }

			if (qEvent.IsSignal(QSignals.Exit))
            {
                LogMessage(String.Format("Philosopher {0} is exiting hungry state.", m_PhilosopherId));
                return null;
            }
			return this.TopState;
		}

		private static TransitionChain s_Tran_Eating_Thinking;
		private QState Eating(IQEvent qEvent)
		{
			if (qEvent.IsSignal(QSignals.Entry))
            {
                LogMessage(String.Format("Philosopher {0} is eating.", m_PhilosopherId));
                m_Timer.FireIn(c_EatTime, new PhilosopherEvent(DPPSignal.Timeout));
                return null;
            }

			if (qEvent.IsSignal(DPPSignal.Timeout))
            {
                TransitionTo(m_StateThinking, ref s_Tran_Eating_Thinking);
                return null;
            }

			if (qEvent.IsSignal(QSignals.Exit))
            {
                LogMessage(String.Format("Philosopher {0} is exiting eating state.", m_PhilosopherId));
				TableEvent tableEvent = new TableEvent(DPPSignal.Done, m_PhilosopherId); 
				LogMessage(String.Format("Philosopher {0} publishes Done event.", m_PhilosopherId));
				Qf.Instance.Publish(tableEvent);
				return null;
			}
			return this.TopState;
		}

		public override string ToString()
		{
			return "Philosopher " + m_PhilosopherId;
		}

		private void LogMessage(string message)
		{
			Debug.WriteLine("\t" + Thread.CurrentThread.Name + ": " + message);
		}

		protected override void HsmUnhandledException(Exception e) { throw new NotImplementedException(); }
	}
}
