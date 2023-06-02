using System;
using System.Diagnostics;
using System.Threading;
using qf4net;

namespace DiningPhilosophers
{
	/// <summary>
	/// The active object that represents the table
	/// </summary>
	public class Table : QActive
	{
		private QState m_StateServing;
		private int m_NumberOfPhilosophers;
		private bool[] m_ForkIsUsed;
		private bool[] m_PhilosopherIsHungry;
		
		public Table(int numberOfPhilosophers)
		{
			m_NumberOfPhilosophers = numberOfPhilosophers;
			m_ForkIsUsed = new bool[m_NumberOfPhilosophers];
			m_PhilosopherIsHungry = new bool[m_NumberOfPhilosophers];

			for(int i = 0; i < m_NumberOfPhilosophers; i++)
			{
				m_ForkIsUsed[i] = false;
				m_PhilosopherIsHungry[i] = false;
			}

			m_StateServing = new QState(this.Serving);
		}

		/// <summary>
		/// Is called inside of the function Init to give the deriving class a chance to
		/// initialize the state machine.
		/// </summary>
		protected override void InitializeStateMachine()
		{
			Thread.CurrentThread.Name = this.ToString();
			// Subscribe for the relevant events raised by philosophers
			Qf.Instance.Subscribe(this, DPPSignal.Hungry);
			Qf.Instance.Subscribe(this, DPPSignal.Done);

			InitializeState(m_StateServing); // initial transition			
		}

				
		private QState Serving(IQEvent qEvent)
		{
			int philosopherId;

			if (qEvent.IsSignal(DPPSignal.Hungry))
            {
                philosopherId = GetPhilosopherId(qEvent);
                Debug.Assert(!m_PhilosopherIsHungry[philosopherId], "Philosopher must not be already hungry");

                Console.WriteLine(String.Format("Philosopher {0} is hungry.", philosopherId));

                if (ForksFree(philosopherId))
                {
                    LetPhilosopherEat(philosopherId);
                }
                else
                {
                    // The philosopher has to wait for free forks
                    m_PhilosopherIsHungry[philosopherId] = true; // mark philosopher as hungry
                    Console.WriteLine(String.Format("Philosopher {0} has to wait for forks.", philosopherId));
                }
                return null;
            }

			if (qEvent.IsSignal(DPPSignal.Done))
            {
                philosopherId = GetPhilosopherId(qEvent);
                Console.WriteLine(String.Format("Philosopher {0} is done eating.", philosopherId));
                m_PhilosopherIsHungry[philosopherId] = false;

                // free up the philosopher's forks
                FreeForks(philosopherId);

                // Can the left philosopher eat?
                int neighborPhilosopher = LeftIndex(philosopherId);
                if (m_PhilosopherIsHungry[neighborPhilosopher] && ForksFree(neighborPhilosopher))
                {
                    LetPhilosopherEat(neighborPhilosopher);
                    // The left philosopher could eat; mark philosopher as no longer hungry
                    m_PhilosopherIsHungry[neighborPhilosopher] = false;
                }

                // Can the right philosopher eat?
                neighborPhilosopher = RightIndex(philosopherId);
                if (m_PhilosopherIsHungry[neighborPhilosopher] && ForksFree(neighborPhilosopher))
                {
                    LetPhilosopherEat(neighborPhilosopher);
                    // The left philosopher could eat; mark philosopher as no longer hungry
                    m_PhilosopherIsHungry[neighborPhilosopher] = false;
                }

                return null;
            }
			return this.TopState;
		}

		private int GetPhilosopherId(IQEvent qEvent)
		{
			int philosopherId = ((TableEvent)qEvent).PhilosopherId;
			Debug.Assert(philosopherId < m_NumberOfPhilosophers, "Philosopher id must be < number of philosophers");
			return philosopherId;
		}

		private void LetPhilosopherEat(int philosopherId)
		{
			UseForks(philosopherId);
			TableEvent tableEvent = new TableEvent(DPPSignal.Eat, philosopherId);
			Debug.WriteLine(String.Format("Table publishes Eat event for Philosopher {0}.", philosopherId));

			Qf.Instance.Publish(tableEvent);
			Console.WriteLine(String.Format("Philosopher {0} is eating.", philosopherId));
		}


		private int LeftIndex(int index)
		{
			return (index  + 1) % m_NumberOfPhilosophers; 
		}

		private int RightIndex(int index)
		{
			return (index  - 1 + m_NumberOfPhilosophers) % m_NumberOfPhilosophers; 
		}

		private bool ForksFree(int philosopherId)
		{
			return (m_ForkIsUsed[philosopherId] == false && m_ForkIsUsed[LeftIndex(philosopherId)] == false);
		}

		private void UseForks(int philosopherId)
		{
			m_ForkIsUsed[philosopherId] = true;
			m_ForkIsUsed[LeftIndex(philosopherId)] = true;
		}

		private void FreeForks(int philosopherId)
		{
			m_ForkIsUsed[philosopherId] = false;
			m_ForkIsUsed[LeftIndex(philosopherId)] = false;
		}
 
		public override string ToString()
		{
			return "Table";
		}

		protected override void HsmUnhandledException(Exception e) { throw new NotImplementedException(); }
	}
}
