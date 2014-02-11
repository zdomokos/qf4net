using System;
using qf4net;

namespace CalculatorHSM
{
	public class CalcEvent : QEvent 
	{

		private char keyChar;
		public char KeyChar
		{
			get{return keyChar;}
		}

		//This property isn't used
		[Obsolete("Not used.", false)]
		public int Signal
		{
			get{return base.QSignal;}
		}

		public CalcEvent(char keyChar):base(CalcSignal.GetSignal(keyChar))
		{
			this.keyChar = keyChar;
		}//ctor

	}//CalcEvent


}//namespace CalculatorHSM
