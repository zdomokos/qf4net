using System;
using qf4net;

namespace OrthogonalComponentHsm
{
	public class AlarmInitEvent : QEvent 
	{
		public AlarmInitEvent(Signal signal):base(signal)
		{
		}//ctor

	}//AlarmInitEvent

	public class TimeEvent : QEvent 
	{
		public DateTime CurrentTime
		{
			get { return currentTime; }
			set { currentTime = value; }
		}
		private DateTime currentTime;

		public TimeEvent(DateTime currentTime, Signal signal):base(signal)
		{
			this.currentTime = currentTime;
		}//ctor

	}//TimeEvent

}//namespace OrthogonalComponentHsm
