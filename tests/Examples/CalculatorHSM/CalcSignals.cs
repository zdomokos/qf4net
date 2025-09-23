using qf4net;

namespace CalculatorHSM;

public static class CalcSignals
{
    public static readonly Signal ZeroDigit    = new(nameof(ZeroDigit));
    public static readonly Signal NonZeroDigit = new(nameof(NonZeroDigit));
    public static readonly Signal DecimalPoint = new(nameof(DecimalPoint));
    public static readonly Signal EqualSign    = new(nameof(EqualSign));
    public static readonly Signal Operator     = new(nameof(Operator));
    public static readonly Signal ClearAll     = new(nameof(ClearAll));
    public static readonly Signal ClearEntry   = new(nameof(ClearEntry));
    public static readonly Signal Quit         = new(nameof(Quit));
}

public sealed class CalcSignal
{
    public static Signal GetSignal(char c)
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
