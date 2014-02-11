using System;
using qf4net;

namespace CalculatorHSM
{

	public enum CalcSignals : int
	{
		ZeroDigit =	100, //QSignals.UserSig, //enum values must start at UserSig value or greater
		NonZeroDigit,
		DecimalPoint,
		EqualSign,
		Operator,
//		IDC_PERCENT,
		ClearAll,
		ClearEntry,
		Quit

	}//CalcSignals

	public sealed class CalcSignal
	{
		public static int GetSignal(char c)
		{
			if (Char.IsDigit(c))
			{
				if (c == '0')
				{
					return (int)CalcSignals.ZeroDigit;
				}
				return (int)CalcSignals.NonZeroDigit;
			}

			if (
				c == '.'
				)
			{
				return (int)CalcSignals.DecimalPoint;
			}

			if (
				c == '='
				)
			{
				return (int)CalcSignals.EqualSign;
			}

			if (
				c == '+' ||
				c == '-' ||
				c == '*' || //c == 'x' || c == 'X' || //could add various multiplication characters
				c == '/'
				)
			{
				return (int)CalcSignals.Operator;
			}

			if (
				c == 'C' ||
				c == 'c'
				)
			{
				return (int)CalcSignals.ClearAll;
			}

			if (
				c == 'E' ||
				c == 'e'
				)
			{
				return (int)CalcSignals.ClearEntry;
			}

			if (
				c == 'Q' ||
				c == 'q'
				)
			{
				return (int)CalcSignals.Quit;
			}

			return -1;

		}//GetSignal
		
	}//CalcSignal


}//namespace CalculatorHSM
