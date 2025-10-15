// -----------------------------------------------------------------------------
// Calculator Example for Rainer Hessmer's C# port of
// Samek's Quantum Hierarchical State Machine.
//
// Author: David Shields (david@shields.net)
// This code is adapted from Samek's C example.
// See the following site for the statechart:
// http://www.quantum-leaps.com/cookbook/recipes.htm
//
// References:
// Practical Statecharts in C/C++; Quantum Programming for Embedded Systems
// Author: Miro Samek, Ph.D.
// http://www.quantum-leaps.com/book.htm
//
// Rainer Hessmer, Ph.D. (rainer@hessmer.org)
// http://www.hessmer.org/dev/qhsm/
// -----------------------------------------------------------------------------

using System;
using qf4net;

namespace CalculatorHSM;
// Define preprocessor variable STATIC_TRANS in order to use the implementation of the calculator that
// uses static transitions which can be used even in cases where another state machine is derived from this
// state machine

/// <summary>
/// Calculator state machine example for Rainer Hessmer's C# port of HQSM
/// </summary>
public sealed class Calc : QHsmLegacy
{
#if (STATIC_TRANS)

    #region Boiler plate static stuff

    private static new readonly TransitionChainStore STransitionChainStore = new(
                                                                              System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
                                                                             );

    static Calc()
    {
        STransitionChainStore.ShrinkToActualSize();
    }

    /// <summary>
    /// Getter for an optional <see cref="TransitionChainStore"/> that can hold cached
    /// <see cref="TransitionChain"/> objects that are used to optimize static transitions.
    /// </summary>
    protected override TransitionChainStore TransChainStore => STransitionChainStore;

    #endregion

#endif

    //communication with main form is via these events:
    public delegate void CalcDisplayHandler(object sender, CalcDisplayEventArgs e);

    public event CalcDisplayHandler DisplayValue;
    public event CalcDisplayHandler DisplayState;

    public bool IsHandled { get; set; } //IsHandled

    private       string _myDisplay;
    private       double _myOperand1;
    private       double _myOperand2;
    private       char   _myOperator;
    private const int    Precision = 14;


#if (STATIC_TRANS)
    private static readonly int STranIdxCalculateCalculate = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxCalculateFinal     = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoCalculate(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("calc");
            return null;
        }
        else if (qevent.Signal == QSignals.Init)
        {
            Clear();
            InitializeState(DoReady);
            return null;
        }
        else if (qevent.Signal == CalcSignals.ClearAll)
        {
            Clear();
#if (STATIC_TRANS)
            TransitionTo(DoCalculate, STranIdxCalculateCalculate);
#else
                    TransitionTo(DoCalculate);
#endif
            return null;
        }
        else if (qevent.Signal == CalcSignals.Quit)
        {
#if (STATIC_TRANS)
            TransitionTo(DoFinal, STranIdxCalculateFinal);
#else
                    TransitionTo(DoFinal);
#endif
            return null;
        }

        // if (qevent.Signal >= (int)QSignals.UserSig)
        // {
        //     IsHandled = false;
        // }

        return TopState;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxReadyZero1     = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxReadyInt1      = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxReadyFrac1     = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxReadyOpEntered = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoReady(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("ready");
            return null;
        }

        if (qevent.Signal == QSignals.Init)
        {
            InitializeState(DoBegin);
            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit)
        {
            Clear();
#if (STATIC_TRANS)
            TransitionTo(DoZero1, STranIdxReadyZero1);
#else
                    TransitionTo(DoZero1);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Clear();
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoInt1, STranIdxReadyInt1);
#else
                    TransitionTo(DoInt1);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Clear();
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac1, STranIdxReadyFrac1);
#else
                    TransitionTo(DoFrac1);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.Operator)
        {
            _myOperand1 = double.Parse(_myDisplay);
            _myOperator = ((CalcEvent)qevent).KeyChar;
#if (STATIC_TRANS)
            TransitionTo(DoOpEntered, STranIdxReadyOpEntered);
#else
                    TransitionTo(DoOpEntered);
#endif
            return null;
        }

        return DoCalculate;
    }

    private QState DoResult(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("result");
            Eval();
            return null;
        }

        return DoReady;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxBeginNegated1 = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoBegin(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("begin");
            return null;
        }

        if (qevent.Signal == CalcSignals.Operator)
        {
            if (((CalcEvent)qevent).KeyChar == '-')
            {
#if (STATIC_TRANS)
                TransitionTo(DoNegated1, STranIdxBeginNegated1);
#else
                        TransitionTo(DoNegated1);
#endif
                return null; // event handled
            }
            //Uncomment the follow "else-if" block to get the same
            //behavior as the C version. It was commented out for
            //the following reason:
            //
            //Looking for a unary "+" introduces a small inconsistency:
            //Multiplication and division operators are accepted with
            //operand1 defaulting to zero. However, the addition operator
            //does not behave in the same way.
            //
            //else if (((CalcEvent)qevent).KeyChar == '+')
            //{      // unary "+"
            //	return null;							// event handled
            //}
            //
            //
            //One alternative is to ignore '+' '*' and '/' so they all behave
            //consistently
            //
        } // event unhandled!

        return DoReady;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxNegated1Begin = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxNegated1Zero1 = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxNegated1Int1  = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxNegated1Frac1 = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoNegated1(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("negated1");
            Negate();
            return null;
        }

        if (qevent.Signal == CalcSignals.ClearEntry)
        {
            Clear();
#if (STATIC_TRANS)
            TransitionTo(DoBegin, STranIdxNegated1Begin);
#else
                    TransitionTo(DoBegin);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoZero1, STranIdxNegated1Zero1);
#else
                    TransitionTo(DoZero1);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoInt1, STranIdxNegated1Int1);
#else
                    TransitionTo(DoInt1);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac1, STranIdxNegated1Frac1);
#else
                    TransitionTo(DoFrac1);
#endif
            return null;
        }

        return DoCalculate;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxOperand1Begin     = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxOperand1OpEntered = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoOperand1(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("operand1");
            return null;
        }

        if (qevent.Signal == CalcSignals.ClearEntry)
        {
            Clear();
#if (STATIC_TRANS)
            TransitionTo(DoBegin, STranIdxOperand1Begin);
#else
                    TransitionTo(DoBegin);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.Operator)
        {
            _myOperand1 = double.Parse(_myDisplay);
            _myOperator = ((CalcEvent)qevent).KeyChar;
#if (STATIC_TRANS)
            TransitionTo(DoOpEntered, STranIdxOperand1OpEntered);
#else
                    TransitionTo(DoOpEntered);
#endif
            return null;
        }

        return DoCalculate;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxZero1Int1  = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxZero1Frac1 = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoZero1(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("zero1");
            return null;
        }

        if (qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoInt1, STranIdxZero1Int1);
#else
                    TransitionTo(DoInt1);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac1, STranIdxZero1Frac1);
#else
                    TransitionTo(DoFrac1);
#endif
            return null;
        }

        return DoOperand1;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxInt1Frac1 = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoInt1(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("int1");
            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit || qevent.Signal ==CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac1, STranIdxInt1Frac1);
#else
                    TransitionTo(DoFrac1);
#endif
            return null;
        }

        return DoOperand1;
    }

    private QState DoFrac1(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("frac1");
            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit || qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
            return null;
        }

        return DoOperand1;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxOpEnteredNegated2 = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxOpEnteredZero2    = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxOpEnteredInt2     = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxOpEnteredFrac2    = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoOpEntered(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("opEntered");
            return null;
        }

        if (qevent.Signal == CalcSignals.Operator)
        {
            if (((CalcEvent)qevent).KeyChar == '-')
            {
                Clear();
#if (STATIC_TRANS)
                TransitionTo(DoNegated2, STranIdxOpEnteredNegated2);
#else
                        TransitionTo(DoNegated2);
#endif
            }

            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit)
        {
            Clear();
#if (STATIC_TRANS)
            TransitionTo(DoZero2, STranIdxOpEnteredZero2);
#else
                    TransitionTo(DoZero2);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Clear();
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoInt2, STranIdxOpEnteredInt2);
#else
                    TransitionTo(DoInt2);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Clear();
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac2, STranIdxOpEnteredFrac2);
#else
                    TransitionTo(DoFrac2);
#endif
            return null;
        }

        return DoCalculate;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxNegated2OpEntered = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxNegated2Zero2     = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxNegated2Int2      = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxNegated2Frac2     = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoNegated2(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("negated2");
            Negate();
            return null;
        }

        if (qevent.Signal == CalcSignals.ClearEntry)
        {
#if (STATIC_TRANS)
            TransitionTo(DoOpEntered, STranIdxNegated2OpEntered);
#else
                    TransitionTo(DoOpEntered);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit)
        {
#if (STATIC_TRANS)
            TransitionTo(DoZero2, STranIdxNegated2Zero2);
#else
                    TransitionTo(DoZero2);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoInt2, STranIdxNegated2Int2);
#else
                    TransitionTo(DoInt2);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac2, STranIdxNegated2Frac2);
#else
                    TransitionTo(DoFrac2);
#endif
            return null;
        }

        return DoCalculate;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxOperand2OpEntered = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxOperand2Result    = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoOperand2(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("operand2");
            return null;
        }

        if (qevent.Signal == CalcSignals.ClearEntry)
        {
            Clear();
#if (STATIC_TRANS)
            TransitionTo(DoOpEntered, STranIdxOperand2OpEntered);
#else
                    TransitionTo(DoOpEntered);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.Operator)
        {
            _myOperand2 = double.Parse(_myDisplay);
            Eval();
            _myOperand1 = double.Parse(_myDisplay);
            _myOperator = ((CalcEvent)qevent).KeyChar;
#if (STATIC_TRANS)
            TransitionTo(DoOpEntered, STranIdxOperand2OpEntered);
#else
                    TransitionTo(DoOpEntered);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.EqualSign)
        {
            _myOperand2 = double.Parse(_myDisplay);
#if (STATIC_TRANS)
            TransitionTo(DoResult, STranIdxOperand2Result);
#else
                    TransitionTo(DoResult);
#endif
            return null;
        }

        return DoCalculate;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxZero2Int2  = STransitionChainStore.GetOpenSlot();
    private static readonly int STranIdxZero2Frac2 = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoZero2(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("zero2");
            return null;
        }

        if (qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoInt2, STranIdxZero2Int2);
#else
                    TransitionTo(DoInt2);
#endif
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac2, STranIdxZero2Frac2);
#else
                    TransitionTo(DoFrac2);
#endif
            return null;
        }

        return DoOperand2;
    }

#if (STATIC_TRANS)
    private static readonly int STranIdxInt2Frac2 = STransitionChainStore.GetOpenSlot();
#endif

    private QState DoInt2(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("int2");
            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit || qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
            return null;
        }

        if (qevent.Signal == CalcSignals.DecimalPoint)
        {
            Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
            TransitionTo(DoFrac2, STranIdxInt2Frac2);
#else
                    TransitionTo(DoFrac2);
#endif
            return null;
        }

        return DoOperand2;
    }

    private QState DoFrac2(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("frac2");
            return null;
        }

        if (qevent.Signal == CalcSignals.ZeroDigit || qevent.Signal == CalcSignals.NonZeroDigit)
        {
            Insert(((CalcEvent)qevent).KeyChar);
            return null;
        }

        return DoOperand2;
    }

    //UNDONE: revise this code
    private QState DoFinal(IQEvent qevent)
    {
        if (qevent.Signal == QSignals.Entry)
        {
            OnDisplayState("HSM terminated");
            _singleton = null;
            CalcForm.Instance.Close();
            System.Windows.Forms.Application.Exit();
            return null;
        }

        return TopState;
    }

    private void Clear()
    {
        _myDisplay = " 0";
        OnDisplayValue(_myDisplay);
    } //Clear

    private void Insert(char keyChar)
    {
        if (_myDisplay.Length < Precision)
        {
            if (keyChar != '.') //TODO: clean this logic up
            {
                if (_myDisplay == " 0")
                {
                    _myDisplay = " ";
                }
                else if (_myDisplay == "-0")
                {
                    _myDisplay = "-";
                }
            }

            _myDisplay += keyChar.ToString();
            OnDisplayValue(_myDisplay);
        }
    }

    private void Negate()
    {
        var temp = _myDisplay.Remove(0, 1);
        _myDisplay = temp.Insert(0, "-");
        OnDisplayValue(_myDisplay);
    }

    private void Eval()
    {
        var r = double.NaN;

        switch (_myOperator)
        {
            case '+':
                r = _myOperand1 + _myOperand2;
                break;
            case '-':
                r = _myOperand1 - _myOperand2;
                break;
            case '*':
                r = _myOperand1 * _myOperand2;
                break;
            case '/':
                if (_myOperand2 != 0.0)
                {
                    r = _myOperand1 / _myOperand2;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                                                         "Cannot Divide by 0",
                                                         "Calculator HSM",
                                                         System.Windows.Forms.MessageBoxButtons.OK,
                                                         System.Windows.Forms.MessageBoxIcon.Error
                                                        );
                    r = 0.0;
                }

                break;
            default:
                System.Diagnostics.Debug.Assert(false);
                break;
        }

        if (-1.0e10 < r && r < 1.0e10)
        {
            //sprintf(myDisplay, "%24.11g", r);
            _myDisplay = r.ToString(); //TODO: add formatting
        }
        else
        {
            System.Windows.Forms.MessageBox.Show(
                                                 "Result out of range",
                                                 "Calculator HSM",
                                                 System.Windows.Forms.MessageBoxButtons.OK,
                                                 System.Windows.Forms.MessageBoxIcon.Error
                                                );
            Clear();
        }

        OnDisplayValue(_myDisplay);
    }

    private void OnDisplayState(string stateInfo)
    {
        if (DisplayState != null)
        {
            DisplayState(this, new CalcDisplayEventArgs(stateInfo));
        }
    } //OnDisplayState

    private void OnDisplayValue(string valueInfo)
    {
        if (DisplayValue != null)
        {
            DisplayValue(this, new CalcDisplayEventArgs(valueInfo));
        }
    } //OnDisplayValue

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// </summary>
    protected override void InitializeStateMachine()
    {
        InitializeState(DoCalculate); // initial transition
    }

    private Calc()
    {
    }

    //
    //Thread-safe implementation of singleton as a property
    //
    private static volatile Calc   _singleton;
    private static readonly object Sync      = new(); //for static lock

    public static Calc Instance
    {
        get
        {
            if (_singleton == null)
            {
                lock (Sync)
                {
                    if (_singleton == null)
                    {
                        _singleton = new Calc();
                        _singleton.Init();
                    }
                }
            }

            return _singleton;
        }
    }
}

public class CalcDisplayEventArgs : EventArgs
{
    public string Message { get; }

    public CalcDisplayEventArgs(string message)
    {
        Message = message;
    }
}
