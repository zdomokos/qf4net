using System;
using qf4net;

namespace RunToCompletionHsm
{
	public class RtcEvent : QEvent 
	{
		public RtcEvent(Signal signal) : base(signal)
		{
		}//ctor

	}//RtcEvent


}//namespace RunToCompletionHsm
