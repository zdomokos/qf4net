using System;
using qf4net;

namespace OptimizationBreaker
{
	/// <summary>
	/// </summary>
	public class QHsmDerived3 : QHsmBase3 
	{
		#region Boiler plate static stuff

		protected static new TransitionChainStore s_TransitionChainStore = 
			new TransitionChainStore(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static QHsmDerived3()
		{
			s_TransitionChainStore.ShrinkToActualSize();
		}

		#endregion

		protected QState			m_s021;

		/// <summary>
		/// Default constructor - initializes all fields to default values
		/// </summary>
		public QHsmDerived3()
		{
			m_s021 = new QState(this.s021);
		}

		/// <summary>
		/// Getter for an optional <see cref="TransitionChainStore"/> that can hold cached
		/// <see cref="TransitionChain"/> objects that are used to optimize static transitions.
		/// </summary>
		protected override TransitionChainStore TransChainStore
		{
			get { return s_TransitionChainStore; }
		}

		protected override QState s02(IQEvent qEvent)
		{
			switch (qEvent.QSignal)
			{
				case (int)QSignals.Init:    Console.Write("s02-INIT;");  InitializeState(m_s021); return null;
			} 
			return base.s02(qEvent);
		}

		protected QState s021(IQEvent qEvent)
		{
			switch (qEvent.QSignal)
			{
				case (int)QSignals.Entry:   Console.Write("s021-ENTRY;"); return null;
				case (int)QSignals.Exit:    Console.Write("s021-EXIT;");  return null;
			} 
			return m_s02;
		}
	}
}
