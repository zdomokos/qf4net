using System;
using qf4net;

namespace ReminderHsm
{
	public class ReminderEvent : QEvent 
	{
		public ReminderEvent(ReminderSignals signal):base((int)signal)
		{
		}//ctor

	}//ReminderEvent


}//namespace ReminderHsm
