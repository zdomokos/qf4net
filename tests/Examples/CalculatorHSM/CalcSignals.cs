using qf4net;

namespace CalculatorHSM;

public static class CalcSignals
{
    public static readonly QSignal ZeroDigit    = new();
    public static readonly QSignal NonZeroDigit = new();
    public static readonly QSignal DecimalPoint = new();
    public static readonly QSignal EqualSign    = new();
    public static readonly QSignal Operator     = new();
    public static readonly QSignal ClearAll     = new();
    public static readonly QSignal ClearEntry   = new();
    public static readonly QSignal Quit         = new();
}

public sealed class CalcSignal
{
    public static QSignal GetSignal(char c)
    {
        if (char.IsDigit(c))
        {
            if (c == '0')
            {
                return CalcSignals.ZeroDigit;
            }

            return CalcSignals.NonZeroDigit;
        }

        if (c == '.')
        {
            return CalcSignals.DecimalPoint;
        }

        if (c == '=')
        {
            return CalcSignals.EqualSign;
        }

        if (
                c == '+'
             || c == '-'
             || c == '*'
             || //c == 'x' || c == 'X' || //could add various multiplication characters
                c == '/'
            )
        {
            return CalcSignals.Operator;
        }

        if (c is 'C' or 'c')
        {
            return CalcSignals.ClearAll;
        }

        if (c is 'E' or 'e')
        {
            return CalcSignals.ClearEntry;
        }

        if (c is 'Q' or 'q')
        {
            return CalcSignals.Quit;
        }

        return null; // TODO: this was -1, don't what the right signa is
    } //GetSignal
}     //CalcSignal
//namespace CalculatorHSM
