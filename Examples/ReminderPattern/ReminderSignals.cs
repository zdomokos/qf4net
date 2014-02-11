using System;
using qf4net;

namespace ReminderHsm
{

	public enum ReminderSignals : int
	{
		DataReady =	QSignals.UserSig, //enum values must start at UserSig value or greater
		TimerTick,
		Terminate

	}//ReminderSignals

}//namespace ReminderHsm
