using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace CalculatorHSM
{
	/// <summary>
	/// Calculator example
	/// </summary>
	public class CalcForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox textBoxDisplay;
		private System.Windows.Forms.Button button0;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button buttonClear;
		private System.Windows.Forms.Button buttonClearEntry;
		private System.Windows.Forms.Button buttonPlus;
		private System.Windows.Forms.Button buttonMinus;
		private System.Windows.Forms.Button buttonMultiply;
		private System.Windows.Forms.Button buttonPoint;
		private System.Windows.Forms.Button buttonEquals;
		private System.Windows.Forms.Label labelState;
		private System.Windows.Forms.TextBox textBoxState;
		private System.Windows.Forms.Button buttonDivide;
		private System.Windows.Forms.Button buttonStateChart;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


		private void Form1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{	
			
			if (
				Char.IsDigit(e.KeyChar) ||
				e.KeyChar == 'C' || e.KeyChar == 'c' || //clear all
				e.KeyChar == 'E' || e.KeyChar == 'e' || //clear entry
				e.KeyChar == 'Q' || e.KeyChar == 'q' || //quit or terminate app
				e.KeyChar == '+' ||
				e.KeyChar == '-' ||
				e.KeyChar == '*' ||
				e.KeyChar == '/' ||
				e.KeyChar == '=' ||
				e.KeyChar == '.'
				)
			{
				Calc.Instance.IsHandled = true;
				CalcEvent calcEvent = new CalcEvent(e.KeyChar);
				Calc.Instance.Dispatch(calcEvent);
				e.Handled = Calc.Instance.IsHandled;
			}
			else
			{
				e.Handled = false;
			}
		}//Form1_KeyPress

		#region button click handlers
		//All input (number) button clicks are translated into key presses and 
		//forwarded to Form1_KeyPress
		//Focus is removed from the button to allow full keyboard functionality
		//at all times.
		private void button0_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('0'));
			this.textBoxDisplay.Focus();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('1'));
			this.textBoxDisplay.Focus();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('2'));
			this.textBoxDisplay.Focus();
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('3'));
			this.textBoxDisplay.Focus();
		}

		private void button4_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('4'));
			this.textBoxDisplay.Focus();
		}

		private void button5_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('5'));
			this.textBoxDisplay.Focus();
		}

		private void button6_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('6'));
			this.textBoxDisplay.Focus();
		}

		private void button7_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('7'));
			this.textBoxDisplay.Focus();
		}

		private void button8_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('8'));
			this.textBoxDisplay.Focus();
		}

		private void button9_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('9'));
			this.textBoxDisplay.Focus();
		}

		private void buttonPoint_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('.'));
			this.textBoxDisplay.Focus();
		}

		private void buttonPlus_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('+'));
			this.textBoxDisplay.Focus();
		}

		private void buttonMinus_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('-'));
			this.textBoxDisplay.Focus();
		}

		private void buttonMultiply_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('*'));
			this.textBoxDisplay.Focus();
		}

		private void buttonDivide_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('/'));
			this.textBoxDisplay.Focus();
		}

		private void buttonEquals_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('='));
			this.textBoxDisplay.Focus();
		}

		private void buttonClear_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('C'));
			this.textBoxDisplay.Focus();
		}

		private void buttonClearEntry_Click(object sender, System.EventArgs e)
		{
			Form1_KeyPress(sender, new System.Windows.Forms.KeyPressEventArgs('E'));
			this.textBoxDisplay.Focus();
		}

		private void textBoxState_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			Form1_KeyPress(sender, e);
			this.textBoxDisplay.Focus();
		}

		private void textBoxDisplay_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			Form1_KeyPress(sender, e);
		}

		private void UpdateCalcValues(object sender, CalcDisplayEventArgs e)
		{
			this.textBoxDisplay.Text = e.Message;
		}

		private void UpdateCalcStatus(object sender, CalcDisplayEventArgs e)
		{
			this.textBoxState.Text = e.Message;
		}

		private void buttonStateChart_Click(object sender, System.EventArgs e)
		{
			StateChartDialog.Instance.Show();
		}
		private void CalcForm_Activated(object sender, System.EventArgs e)
		{
			this.textBoxDisplay.Focus();
		}

		#endregion

		private CalcForm()
		{			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Calc.Instance.DisplayValue += new Calc.CalcDisplayHandler(UpdateCalcValues);
			Calc.Instance.DisplayState += new Calc.CalcDisplayHandler(UpdateCalcStatus);

		}//ctor

		//
		//Thread-safe implementation of singleton -- not strictly necessary for this
		//example project
		//
		private static volatile CalcForm singleton = null;
		private static object sync = new object();//for static lock

		public static CalcForm Instance
		{
			get
			{
				if (singleton == null)
				{
					lock (sync)
					{
						if (singleton == null)
						{
							singleton = new CalcForm();	
						}
					}
				}
			
				return singleton;
			}
		}//Instance


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.labelState = new System.Windows.Forms.Label();
			this.textBoxState = new System.Windows.Forms.TextBox();
			this.textBoxDisplay = new System.Windows.Forms.TextBox();
			this.button0 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
			this.buttonClear = new System.Windows.Forms.Button();
			this.buttonClearEntry = new System.Windows.Forms.Button();
			this.buttonPlus = new System.Windows.Forms.Button();
			this.buttonMinus = new System.Windows.Forms.Button();
			this.buttonMultiply = new System.Windows.Forms.Button();
			this.buttonDivide = new System.Windows.Forms.Button();
			this.buttonPoint = new System.Windows.Forms.Button();
			this.buttonEquals = new System.Windows.Forms.Button();
			this.buttonStateChart = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelState
			// 
			this.labelState.Location = new System.Drawing.Point(115, 18);
			this.labelState.Name = "labelState";
			this.labelState.Size = new System.Drawing.Size(48, 27);
			this.labelState.TabIndex = 19;
			this.labelState.Text = "State:";
			this.labelState.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxState
			// 
			this.textBoxState.Location = new System.Drawing.Point(163, 18);
			this.textBoxState.Name = "textBoxState";
			this.textBoxState.ReadOnly = true;
			this.textBoxState.Size = new System.Drawing.Size(173, 22);
			this.textBoxState.TabIndex = 20;
			this.textBoxState.TabStop = false;
			this.textBoxState.Text = "ready";
			this.textBoxState.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textBoxState.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxState_KeyPress);
			// 
			// textBoxDisplay
			// 
			this.textBoxDisplay.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textBoxDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxDisplay.Location = new System.Drawing.Point(19, 55);
			this.textBoxDisplay.Name = "textBoxDisplay";
			this.textBoxDisplay.ReadOnly = true;
			this.textBoxDisplay.Size = new System.Drawing.Size(317, 26);
			this.textBoxDisplay.TabIndex = 0;
			this.textBoxDisplay.Text = "0";
			this.textBoxDisplay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBoxDisplay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxDisplay_KeyPress);
			// 
			// button0
			// 
			this.button0.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button0.Location = new System.Drawing.Point(19, 240);
			this.button0.Name = "button0";
			this.button0.Size = new System.Drawing.Size(106, 55);
			this.button0.TabIndex = 9;
			this.button0.TabStop = false;
			this.button0.Text = "0";
			this.button0.Click += new System.EventHandler(this.button0_Click);
			// 
			// button1
			// 
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(19, 194);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(48, 37);
			this.button1.TabIndex = 10;
			this.button1.TabStop = false;
			this.button1.Text = "1";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button4
			// 
			this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button4.Location = new System.Drawing.Point(19, 148);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(48, 37);
			this.button4.TabIndex = 13;
			this.button4.TabStop = false;
			this.button4.Text = "4";
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button7
			// 
			this.button7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button7.Location = new System.Drawing.Point(19, 102);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(48, 36);
			this.button7.TabIndex = 16;
			this.button7.TabStop = false;
			this.button7.Text = "7";
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// button2
			// 
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button2.Location = new System.Drawing.Point(77, 194);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(48, 37);
			this.button2.TabIndex = 11;
			this.button2.TabStop = false;
			this.button2.Text = "2";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button5
			// 
			this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button5.Location = new System.Drawing.Point(77, 148);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(48, 37);
			this.button5.TabIndex = 14;
			this.button5.TabStop = false;
			this.button5.Text = "5";
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// button8
			// 
			this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button8.Location = new System.Drawing.Point(77, 102);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(48, 36);
			this.button8.TabIndex = 17;
			this.button8.TabStop = false;
			this.button8.Text = "8";
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// button3
			// 
			this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button3.Location = new System.Drawing.Point(134, 194);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(48, 37);
			this.button3.TabIndex = 12;
			this.button3.TabStop = false;
			this.button3.Text = "3";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button6
			// 
			this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button6.Location = new System.Drawing.Point(134, 148);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(48, 37);
			this.button6.TabIndex = 15;
			this.button6.TabStop = false;
			this.button6.Text = "6";
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// button9
			// 
			this.button9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button9.Location = new System.Drawing.Point(134, 102);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(48, 36);
			this.button9.TabIndex = 18;
			this.button9.TabStop = false;
			this.button9.Text = "9";
			this.button9.Click += new System.EventHandler(this.button9_Click);
			// 
			// buttonClear
			// 
			this.buttonClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonClear.Location = new System.Drawing.Point(230, 102);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(48, 36);
			this.buttonClear.TabIndex = 1;
			this.buttonClear.Text = "C";
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
			// 
			// buttonClearEntry
			// 
			this.buttonClearEntry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonClearEntry.Location = new System.Drawing.Point(288, 102);
			this.buttonClearEntry.Name = "buttonClearEntry";
			this.buttonClearEntry.Size = new System.Drawing.Size(48, 36);
			this.buttonClearEntry.TabIndex = 2;
			this.buttonClearEntry.Text = "CE";
			this.buttonClearEntry.Click += new System.EventHandler(this.buttonClearEntry_Click);
			// 
			// buttonPlus
			// 
			this.buttonPlus.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonPlus.Location = new System.Drawing.Point(230, 148);
			this.buttonPlus.Name = "buttonPlus";
			this.buttonPlus.Size = new System.Drawing.Size(48, 37);
			this.buttonPlus.TabIndex = 3;
			this.buttonPlus.Text = "+";
			this.buttonPlus.Click += new System.EventHandler(this.buttonPlus_Click);
			// 
			// buttonMinus
			// 
			this.buttonMinus.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonMinus.Location = new System.Drawing.Point(288, 148);
			this.buttonMinus.Name = "buttonMinus";
			this.buttonMinus.Size = new System.Drawing.Size(48, 37);
			this.buttonMinus.TabIndex = 4;
			this.buttonMinus.Text = "-";
			this.buttonMinus.Click += new System.EventHandler(this.buttonMinus_Click);
			// 
			// buttonMultiply
			// 
			this.buttonMultiply.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonMultiply.Location = new System.Drawing.Point(230, 194);
			this.buttonMultiply.Name = "buttonMultiply";
			this.buttonMultiply.Size = new System.Drawing.Size(48, 37);
			this.buttonMultiply.TabIndex = 5;
			this.buttonMultiply.Text = "X";
			this.buttonMultiply.Click += new System.EventHandler(this.buttonMultiply_Click);
			// 
			// buttonDivide
			// 
			this.buttonDivide.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonDivide.Location = new System.Drawing.Point(288, 194);
			this.buttonDivide.Name = "buttonDivide";
			this.buttonDivide.Size = new System.Drawing.Size(48, 37);
			this.buttonDivide.TabIndex = 6;
			this.buttonDivide.Text = "/";
			this.buttonDivide.Click += new System.EventHandler(this.buttonDivide_Click);
			// 
			// buttonPoint
			// 
			this.buttonPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonPoint.Location = new System.Drawing.Point(134, 240);
			this.buttonPoint.Name = "buttonPoint";
			this.buttonPoint.Size = new System.Drawing.Size(48, 55);
			this.buttonPoint.TabIndex = 8;
			this.buttonPoint.TabStop = false;
			this.buttonPoint.Text = ".";
			this.buttonPoint.Click += new System.EventHandler(this.buttonPoint_Click);
			// 
			// buttonEquals
			// 
			this.buttonEquals.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonEquals.Location = new System.Drawing.Point(230, 240);
			this.buttonEquals.Name = "buttonEquals";
			this.buttonEquals.Size = new System.Drawing.Size(106, 55);
			this.buttonEquals.TabIndex = 7;
			this.buttonEquals.Text = "=";
			this.buttonEquals.Click += new System.EventHandler(this.buttonEquals_Click);
			// 
			// buttonStateChart
			// 
			this.buttonStateChart.Location = new System.Drawing.Point(10, 18);
			this.buttonStateChart.Name = "buttonStateChart";
			this.buttonStateChart.Size = new System.Drawing.Size(86, 27);
			this.buttonStateChart.TabIndex = 21;
			this.buttonStateChart.Text = "StateChart";
			this.buttonStateChart.Click += new System.EventHandler(this.buttonStateChart_Click);
			// 
			// CalcForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(352, 312);
			this.Controls.Add(this.buttonStateChart);
			this.Controls.Add(this.buttonEquals);
			this.Controls.Add(this.buttonPoint);
			this.Controls.Add(this.buttonDivide);
			this.Controls.Add(this.buttonMultiply);
			this.Controls.Add(this.buttonMinus);
			this.Controls.Add(this.buttonPlus);
			this.Controls.Add(this.buttonClearEntry);
			this.Controls.Add(this.buttonClear);
			this.Controls.Add(this.button9);
			this.Controls.Add(this.button6);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button8);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button7);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.button0);
			this.Controls.Add(this.textBoxDisplay);
			this.Controls.Add(this.textBoxState);
			this.Controls.Add(this.labelState);
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(304, 296);
			this.Name = "CalcForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Calculator HSM";
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
			this.Activated += new System.EventHandler(this.CalcForm_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new CalcForm());
		}


	}//class CalcForm

}//namespace
