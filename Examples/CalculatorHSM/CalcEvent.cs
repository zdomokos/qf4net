using System;
using qf4net;

namespace CalculatorHSM;

public class CalcEvent : QEvent
{
    public char KeyChar { get; }

    //This property isn't used
    [Obsolete("Not used.", false)] public Signal Signal => base.Signal;

    public CalcEvent(char keyChar)
        : base(CalcSignal.GetSignal(keyChar))
    {
        KeyChar = keyChar;
    }
}
