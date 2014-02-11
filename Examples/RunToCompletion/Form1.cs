using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace RunToCompletionHsm
{
	/// <summary>
	/// RunToCompletion example
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.RichTextBox textBoxHistory;
		private System.Windows.Forms.Label labelState;
		private System.Windows.Forms.TextBox textBoxState;
		private System.Windows.Forms.Button buttonStart;
		private System.Windows.Forms.Button buttonAbort;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private delegate void Dispatcher(qf4net.IQEvent qe);
		private delegate void FinishDelegate();			

		#region click handlers, etc.
		
		private void buttonStart_Click(object sender, System.EventArgs e)
		{
			Abort.Status = false;
			this.buttonStart.Enabled = false;
			DispatchStartMessage();
		}

		private void DispatchStartMessage()
		{
			//RunToCompletion.Instance.IsHandled = true;
			RtcEvent calcEvent = new RtcEvent(RtcSignals.Start);
			//RunToCompletion.Instance.Dispatch(calcEvent);
			Dispatcher d = new Dispatcher(RunToCompletion.Instance.Dispatch);
			IAsyncResult ar = d.BeginInvoke(calcEvent, new AsyncCallback(Finished), d);
		}

		private void Finished(IAsyncResult iar)
		{
			Dispatcher d = (Dispatcher)iar.AsyncState;
			d.EndInvoke(iar);
			UpdateForFinished();
		}

		private void UpdateForFinished()
		{
			// Is this method on the UI thread? If not then we need to marshal it to the UI thread.
			if (this.InvokeRequired)
			{
				FinishDelegate finishDelegate = new FinishDelegate(UpdateForFinished);
				Invoke(finishDelegate);
			}
			else
			{
				if (this.textBoxState.Text != "Completed" && !Abort.Status)
				{
					DispatchStartMessage();
				}
				else
				{
					this.buttonStart.Enabled = true;
				}
			}
		}

		private void buttonAbort_Click(object sender, System.EventArgs e)
		{
			Abort.Status = true;
		}

		private void numericUpDown1_ValueChanged(object sender, System.EventArgs e)
		{
			System.Windows.Forms.NumericUpDown nud = sender as System.Windows.Forms.NumericUpDown;
			RunToCompletion.Instance.BigValue = (int)nud.Value;
		}

		private void UpdateRtcStatus(object sender, RtcDisplayEventArgs e)
		{
			// Is this method on the UI thread? If not then we need to marshal it to the UI thread.
			if (this.InvokeRequired)
			{
				RunToCompletion.RtcDisplayHandler rtcDisplayHandler = new RunToCompletion.RtcDisplayHandler(UpdateRtcStatus);
				Invoke(rtcDisplayHandler, new object[] {sender, e});
			}
			else
			{
				this.textBoxHistory.Text += "\r\n" + e.Message;
				this.textBoxState.Text = e.Message;
			}
		}

		#endregion

		private MainForm()
		{			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			RunToCompletion.Instance.DisplayState += new RunToCompletion.RtcDisplayHandler(UpdateRtcStatus);
	
		}//ctor

		//
		//Thread-safe implementation of singleton -- not strictly necessary for this
		//example project
		//
		private static volatile MainForm singleton = null;
		private static object sync = new object();//for static lock

		public static MainForm Instance
		{
			get
			{
				if (singleton == null)
				{
					lock (sync)
					{
						if (singleton == null)
						{
							singleton = new MainForm();	
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
			this.textBoxHistory = new System.Windows.Forms.RichTextBox();
			this.buttonStart = new System.Windows.Forms.Button();
			this.buttonAbort = new System.Windows.Forms.Button();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.SuspendLayout();
			// 
			// labelState
			// 
			this.labelState.Location = new System.Drawing.Point(29, 55);
			this.labelState.Name = "labelState";
			this.labelState.Size = new System.Drawing.Size(105, 19);
			this.labelState.TabIndex = 19;
			this.labelState.Text = "Current State:";
			this.labelState.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxState
			// 
			this.textBoxState.Location = new System.Drawing.Point(144, 55);
			this.textBoxState.Name = "textBoxState";
			this.textBoxState.ReadOnly = true;
			this.textBoxState.Size = new System.Drawing.Size(192, 22);
			this.textBoxState.TabIndex = 20;
			this.textBoxState.TabStop = false;
			this.textBoxState.Text = "";
			// 
			// textBoxHistory
			// 
			this.textBoxHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxHistory.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textBoxHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxHistory.Location = new System.Drawing.Point(19, 83);
			this.textBoxHistory.Name = "textBoxHistory";
			this.textBoxHistory.ReadOnly = true;
			this.textBoxHistory.Size = new System.Drawing.Size(314, 443);
			this.textBoxHistory.TabIndex = 0;
			this.textBoxHistory.Text = "State History:";
			// 
			// buttonStart
			// 
			this.buttonStart.Location = new System.Drawing.Point(19, 18);
			this.buttonStart.Name = "buttonStart";
			this.buttonStart.Size = new System.Drawing.Size(87, 27);
			this.buttonStart.TabIndex = 21;
			this.buttonStart.Text = "Start";
			this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
			// 
			// buttonAbort
			// 
			this.buttonAbort.Location = new System.Drawing.Point(128, 18);
			this.buttonAbort.Name = "buttonAbort";
			this.buttonAbort.Size = new System.Drawing.Size(86, 27);
			this.buttonAbort.TabIndex = 22;
			this.buttonAbort.Text = "Abort";
			this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Increment = new System.Decimal(new int[] {
																			 10000000,
																			 0,
																			 0,
																			 0});
			this.numericUpDown1.Location = new System.Drawing.Point(232, 18);
			this.numericUpDown1.Maximum = new System.Decimal(new int[] {
																		   1410065408,
																		   2,
																		   0,
																		   0});
			this.numericUpDown1.Minimum = new System.Decimal(new int[] {
																		   1,
																		   0,
																		   0,
																		   0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.numericUpDown1.Size = new System.Drawing.Size(106, 22);
			this.numericUpDown1.TabIndex = 23;
			this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDown1.Value = new System.Decimal(new int[] {
																		 100000000,
																		 0,
																		 0,
																		 0});
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(352, 541);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.buttonAbort);
			this.Controls.Add(this.buttonStart);
			this.Controls.Add(this.textBoxHistory);
			this.Controls.Add(this.textBoxState);
			this.Controls.Add(this.labelState);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(360, 573);
			this.MinimumSize = new System.Drawing.Size(304, 250);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Run To Completion with Abort";
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}


	}//class MainForm

	/// <summary>
	/// super simple shared data type for this example:
	/// used to signal aborted status to Hsm
	/// </summary>
	public class Abort
	{
		public static bool Status
		{
			get { return abort;}
			set { abort = value;}
		}
		private static bool abort = true;
	}


}//namespace
