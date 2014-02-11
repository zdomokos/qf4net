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

namespace CalculatorHSM
{
	// Define preprocessor variable STATIC_TRANS in order to use the implementation of the calculator that
	// uses static transitions which can be used even in cases where another state machine is derived from this
	// state machine

	/// <summary>
	/// Calculator state machine example for Rainer Hessmer's C# port of HQSM
	/// </summary>
	public sealed class Calc : QHsm
	{
#if (STATIC_TRANS)
		#region Boiler plate static stuff

		private static new TransitionChainStore s_TransitionChainStore = 
			new TransitionChainStore(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static Calc()
		{
			s_TransitionChainStore.ShrinkToActualSize();
		}

		/// <summary>
		/// Getter for an optional <see cref="TransitionChainStore"/> that can hold cached
		/// <see cref="TransitionChain"/> objects that are used to optimize static transitions.
		/// </summary>
		protected override TransitionChainStore TransChainStore
		{
			get { return s_TransitionChainStore; }
		}

		#endregion
#endif

		//communication with main form is via these events:
		public delegate void CalcDisplayHandler(object sender, CalcDisplayEventArgs e);
		public event CalcDisplayHandler DisplayValue;
		public event CalcDisplayHandler DisplayState;

		private bool isHandled;
		public bool IsHandled
		{
			get{return isHandled;}
			set{isHandled = value;}
		}//IsHandled

		private string myDisplay;
		private double myOperand1;
		private double myOperand2;
		private char myOperator;
		private const int PRECISION = 14;

		private QState Calculate;
		private QState Ready;
		private QState Result;
		private QState Begin;
		private QState Negated1;
		private QState Operand1;
		private QState Zero1;
		private QState Int1;
		private QState Frac1;
		private QState OpEntered;
		private QState Negated2;
		private QState Operand2;
		private QState Zero2;
		private QState Int2;
		private QState Frac2;
		private QState Final;


#if (STATIC_TRANS)
		private static int s_TranIdx_Calculate_Calculate = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Calculate_Final = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoCalculate(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("calc");
					return null;
				case (int)QSignals.Init:
					Clear();					
					InitializeState(Ready);
					return null;
				case (int)CalcSignals.ClearAll:
					Clear();
#if (STATIC_TRANS)
					TransitionTo(Calculate, s_TranIdx_Calculate_Calculate);
#else
					TransitionTo(Calculate);
#endif
					return null;
				case (int)CalcSignals.Quit:
#if (STATIC_TRANS)
					TransitionTo(Final, s_TranIdx_Calculate_Final);
#else
					TransitionTo(Final);
#endif
					return null;
			}
			if (qevent.QSignal >= (int)QSignals.UserSig)
			{
				isHandled = false;
			}
			return this.TopState;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Ready_Zero1 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Ready_Int1 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Ready_Frac1 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Ready_OpEntered = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoReady(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("ready");
					return null;
				case (int)QSignals.Init:
					InitializeState(Begin);
					return null;
				case (int)CalcSignals.ZeroDigit:
					Clear();
#if (STATIC_TRANS)
					TransitionTo(Zero1, s_TranIdx_Ready_Zero1);
#else
					TransitionTo(Zero1);
#endif
					return null;
				case (int)CalcSignals.NonZeroDigit:
					Clear();
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Int1, s_TranIdx_Ready_Int1);
#else
					TransitionTo(Int1);
#endif
					return null;  
				case (int)CalcSignals.DecimalPoint:
					Clear();
					Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
					TransitionTo(Frac1, s_TranIdx_Ready_Frac1);
#else
					TransitionTo(Frac1);
#endif
					return null;
				case (int)CalcSignals.Operator:					
					this.myOperand1 = double.Parse(myDisplay);
					myOperator = ((CalcEvent)qevent).KeyChar;
#if (STATIC_TRANS)
					TransitionTo(OpEntered, s_TranIdx_Ready_OpEntered);
#else
					TransitionTo(OpEntered);
#endif
					return null;
			}
			return Calculate;
		}


		private QState DoResult(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("result");
					Eval();
					return null;
			}
			return Ready;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Begin_Negated1 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoBegin(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("begin");
					return null;
				case (int)CalcSignals.Operator:
					if (((CalcEvent)qevent).KeyChar == '-') 
					{
#if (STATIC_TRANS)
						TransitionTo(Negated1, s_TranIdx_Begin_Negated1);
#else
						TransitionTo(Negated1);
#endif
						return null;							// event handled
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
					break;										// event unhandled!
			}
			return Ready;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Negated1_Begin = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Negated1_Zero1 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Negated1_Int1 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Negated1_Frac1 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoNegated1(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{    
				case (int)QSignals.Entry:
					OnDisplayState("negated1");
					Negate();
					return null;
				case (int)CalcSignals.ClearEntry:
					Clear();
#if (STATIC_TRANS)
					TransitionTo(Begin, s_TranIdx_Negated1_Begin);
#else
					TransitionTo(Begin);
#endif
					return null;
				case (int)CalcSignals.ZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Zero1, s_TranIdx_Negated1_Zero1);
#else
					TransitionTo(Zero1);
#endif
					return null;
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Int1, s_TranIdx_Negated1_Int1);
#else
					TransitionTo(Int1);
#endif
					return null;
				case (int)CalcSignals.DecimalPoint:
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Frac1, s_TranIdx_Negated1_Frac1);
#else
					TransitionTo(Frac1);
#endif
					return null;
			}
			return Calculate;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Operand1_Begin = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Operand1_OpEntered = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoOperand1(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("operand1");
					return null;
				case (int)CalcSignals.ClearEntry:
					Clear();
#if (STATIC_TRANS)
					TransitionTo(Begin, s_TranIdx_Operand1_Begin);
#else
					TransitionTo(Begin);
#endif
					return null;
				case (int)CalcSignals.Operator:
					this.myOperand1 = double.Parse(myDisplay);
					myOperator = ((CalcEvent)qevent).KeyChar;
#if (STATIC_TRANS)
					TransitionTo(OpEntered, s_TranIdx_Operand1_OpEntered);
#else
					TransitionTo(OpEntered);
#endif
					return null;
			}
			return Calculate;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Zero1_Int1 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Zero1_Frac1 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoZero1(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("zero1");
					return null;
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Int1, s_TranIdx_Zero1_Int1);
#else
					TransitionTo(Int1);
#endif
					return null;
				case (int)CalcSignals.DecimalPoint:
					Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
					TransitionTo(Frac1, s_TranIdx_Zero1_Frac1);
#else
					TransitionTo(Frac1);
#endif
					return null;
			}
			return Operand1;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Int1_Frac1 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoInt1(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("int1");
					return null;
				case (int)CalcSignals.ZeroDigit:
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
					return null;
				case (int)CalcSignals.DecimalPoint:
					Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
					TransitionTo(Frac1, s_TranIdx_Int1_Frac1);
#else
					TransitionTo(Frac1);
#endif
					return null;
			}
			return Operand1;
		}


		private QState DoFrac1(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("frac1");
					return null;
				case (int)CalcSignals.ZeroDigit:
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
					return null;
			}
			return Operand1;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_OpEntered_Negated2 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_OpEntered_Zero2 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_OpEntered_Int2 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_OpEntered_Frac2 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoOpEntered(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("opEntered");
					return null;
				case (int)CalcSignals.Operator:
					if (((CalcEvent)qevent).KeyChar == '-') 
					{
						Clear();
#if (STATIC_TRANS)
						TransitionTo(Negated2, s_TranIdx_OpEntered_Negated2);
#else
						TransitionTo(Negated2);
#endif
					}
					return null;
				case (int)CalcSignals.ZeroDigit:
					Clear();
#if (STATIC_TRANS)
					TransitionTo(Zero2, s_TranIdx_OpEntered_Zero2);
#else
					TransitionTo(Zero2);
#endif
					return null;
				case (int)CalcSignals.NonZeroDigit:
					Clear();
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Int2, s_TranIdx_OpEntered_Int2);
#else
					TransitionTo(Int2);
#endif
					return null;
				case (int)CalcSignals.DecimalPoint:
					Clear();
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Frac2, s_TranIdx_OpEntered_Frac2);
#else
					TransitionTo(Frac2);
#endif
					return null;
			}
			return Calculate;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Negated2_OpEntered = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Negated2_Zero2 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Negated2_Int2 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Negated2_Frac2 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoNegated2(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{    
				case (int)QSignals.Entry:
					OnDisplayState("negated2");
					Negate();
					return null;
				case (int)CalcSignals.ClearEntry:
#if (STATIC_TRANS)
					TransitionTo(OpEntered, s_TranIdx_Negated2_OpEntered);
#else
					TransitionTo(OpEntered);
#endif
					return null;
				case (int)CalcSignals.ZeroDigit:
#if (STATIC_TRANS)
					TransitionTo(Zero2, s_TranIdx_Negated2_Zero2);
#else
					TransitionTo(Zero2);
#endif
					return null;
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Int2, s_TranIdx_Negated2_Int2);
#else
					TransitionTo(Int2);
#endif
					return null;
				case (int)CalcSignals.DecimalPoint:
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Frac2, s_TranIdx_Negated2_Frac2);
#else
					TransitionTo(Frac2);
#endif
					return null;
			}
			return Calculate;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Operand2_OpEntered = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Operand2_Result = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoOperand2(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("operand2");
					return null;
				case (int)CalcSignals.ClearEntry:
					Clear();
#if (STATIC_TRANS)
					TransitionTo(OpEntered, s_TranIdx_Operand2_OpEntered);
#else
					TransitionTo(OpEntered);
#endif
					return null;
				case (int)CalcSignals.Operator:
					this.myOperand2 = double.Parse(myDisplay);
					Eval();
					this.myOperand1 = double.Parse(myDisplay);
					myOperator = ((CalcEvent)qevent).KeyChar;
#if (STATIC_TRANS)
					TransitionTo(OpEntered, s_TranIdx_Operand2_OpEntered);
#else
					TransitionTo(OpEntered);
#endif
					return null;
				case (int)CalcSignals.EqualSign:
					this.myOperand2 = double.Parse(myDisplay);
#if (STATIC_TRANS)
					TransitionTo(Result, s_TranIdx_Operand2_Result);
#else
					TransitionTo(Result);
#endif
					return null;
			}
			return Calculate;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Zero2_Int2 = s_TransitionChainStore.GetOpenSlot();
		private static int s_TranIdx_Zero2_Frac2 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoZero2(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("zero2");
					return null;
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
#if (STATIC_TRANS)
					TransitionTo(Int2, s_TranIdx_Zero2_Int2);
#else
					TransitionTo(Int2);
#endif
					return null;
				case (int)CalcSignals.DecimalPoint:
					Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
					TransitionTo(Frac2, s_TranIdx_Zero2_Frac2);
#else
					TransitionTo(Frac2);
#endif
					return null;
			}
			return Operand2;
		}


#if (STATIC_TRANS)
		private static int s_TranIdx_Int2_Frac2 = s_TransitionChainStore.GetOpenSlot();
#endif
		private QState DoInt2(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("int2");
					return null;
				case (int)CalcSignals.ZeroDigit:
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
					return null;
				case (int)CalcSignals.DecimalPoint:
					Insert(((CalcEvent)qevent).KeyChar);
#if (STATIC_TRANS)
					TransitionTo(Frac2, s_TranIdx_Int2_Frac2);
#else
					TransitionTo(Frac2);
#endif
					return null;
			}
			return Operand2;
		}


		private QState DoFrac2(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("frac2");
					return null;
				case (int)CalcSignals.ZeroDigit:
				case (int)CalcSignals.NonZeroDigit:
					Insert(((CalcEvent)qevent).KeyChar); 
					return null;
			}
			return Operand2;
		}


		//UNDONE: revise this code
		private QState DoFinal(IQEvent qevent)
		{
			switch (qevent.QSignal) 
			{
				case (int)QSignals.Entry:
					OnDisplayState("HSM terminated");
					singleton = null;
					CalcForm.Instance.Close();
					System.Windows.Forms.Application.Exit();
					return null;
			}
			return this.TopState;
		}


		private void Clear()
		{
			this.myDisplay = " 0";
			OnDisplayValue(myDisplay);

		}//Clear

		private void Insert(char keyChar)
		{
			if (this.myDisplay.Length < PRECISION)
			{
				if (keyChar != '.')//TODO: clean this logic up
				{
					if (myDisplay == " 0")
					{
						this.myDisplay = " ";
					}
					else if (myDisplay == "-0")
					{
						this.myDisplay = "-";
					}
				}
				this.myDisplay += keyChar.ToString();
				OnDisplayValue(myDisplay);
			}
		}

		private void Negate()
		{
			string temp = myDisplay.Remove(0, 1);
			myDisplay = temp.Insert(0, "-");
			OnDisplayValue(myDisplay);
		}

		private void Eval()
		{

			double r = double.NaN;
			
			switch (myOperator) 
			{
				case '+':
					r = myOperand1 + myOperand2;
					break;
				case '-':
					r = myOperand1 - myOperand2;
					break;
				case '*':
					r = myOperand1 * myOperand2;
					break;
				case '/':
					if (myOperand2 != 0.0) 
					{
						r = myOperand1 / myOperand2;
					}
					else 
					{
						System.Windows.Forms.MessageBox.Show("Cannot Divide by 0", "Calculator HSM", 
							System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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
				myDisplay = r.ToString();//TODO: add formatting
			}
			else 
			{
				System.Windows.Forms.MessageBox.Show("Result out of range", "Calculator HSM", 
					System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
				Clear();
			}
			OnDisplayValue(myDisplay);
		}


		private void OnDisplayState(string stateInfo)
		{
			if (DisplayState != null)
			{
				DisplayState(this, new CalcDisplayEventArgs(stateInfo));
			}
		}//OnDisplayState

		
		private void OnDisplayValue(string valueInfo)
		{
			if (DisplayValue != null)
			{
				DisplayValue(this, new CalcDisplayEventArgs(valueInfo));
			}
		}//OnDisplayValue


		/// <summary>
		/// Is called inside of the function Init to give the deriving class a chance to
		/// initialize the state machine.
		/// </summary>
		protected override void InitializeStateMachine()
		{
			InitializeState(Calculate); // initial transition			
		}

		private Calc()
		{
			Calculate = new QState(this.DoCalculate);
			Ready = new QState(this.DoReady);
			Result = new QState(this.DoResult);
			Begin = new QState(this.DoBegin);
			Negated1 = new QState(this.DoNegated1);
			Operand1 = new QState(this.DoOperand1);
			Zero1 = new QState(this.DoZero1);
			Int1 = new QState(this.DoInt1);
			Frac1 = new QState(this.DoFrac1);
			OpEntered = new QState(this.DoOpEntered);
			Negated2 = new QState(this.DoNegated2);
			Operand2 = new QState(this.DoOperand2);
			Zero2 = new QState(this.DoZero2);
			Int2 = new QState(this.DoInt2);
			Frac2 = new QState(this.DoFrac2);
			Final = new QState(this.DoFinal);
		}


		//
		//Thread-safe implementation of singleton as a property
		//
		private static volatile Calc singleton = null;
		private static object sync = new object();//for static lock

		public static Calc Instance
		{
			get
			{
				if (singleton == null)
				{
					lock (sync)
					{
						if (singleton == null)
						{
							singleton = new Calc();	
							singleton.Init();
						}
					}
				}
			
				return singleton;
			}
		}//Instance


	}//class Calc

	public class CalcDisplayEventArgs : EventArgs
	{
		private string s;
		public string Message { get { return s; } }

		public CalcDisplayEventArgs(string message) { s = message;}
	}

}//namespace
