using System;
using qf4net;

namespace OptimizationBreaker
{
	/// <summary>
	/// </summary>
	public class QHsmDerived1 : QHsmBase1 
	{
		protected QState			m_s021;
		
		/// <summary>
		/// Default constructor - initializes all fields to default values
		/// </summary>
		public QHsmDerived1()
		{
			m_s021 = new QState(this.s021);
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
