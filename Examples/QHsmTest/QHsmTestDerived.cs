using System;
using qf4net;

namespace QHsmTest
{
	/// <summary>
	/// This hierarchical state machine inherits from QHsmTest and just overrides the handling of the B signal
	/// in the state s1.
	/// </summary>
	public class QHsmTestDerived : QHsmTest
	{
		protected override QState s1(IQEvent qEvent)
		{
			switch (qEvent.QSignal)
			{
				case (int)MyQSignals.B_Sig: Console.Write("s1-B-overriden;"); return null;
			}
			// Everything else we pass on to the state handler of the base class
			return base.s1(qEvent);
		}

	}
}
