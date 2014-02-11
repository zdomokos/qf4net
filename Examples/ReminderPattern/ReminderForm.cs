using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace ReminderHsm
{

	/// <summary>
	/// Reminder example main form. It might be better to also include a start button on the form, but the 
	/// original C example doesn't have one, so this one doesn't either.
	/// 
	/// The initial couple state transitions are missed (in the display) because the state machine is 
	/// instantiated before the form is fully instantiated.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.RichTextBox textBoxHistory;
		private System.Windows.Forms.Label labelState;
		private System.Windows.Forms.TextBox textBoxState;
		private System.Windows.Forms.Button buttonStop;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.TextBox textBoxPollingCtr;
		private System.Windows.Forms.Label labelPollingCtr;
		private System.Windows.Forms.TextBox textBoxProcessingCtr;
		private System.Windows.Forms.Label labelProcessingCtr;
		private System.ComponentModel.IContainer components;

		

		private void buttonStop_Click(object sender, System.EventArgs e)
		{
			DispatchMessage(ReminderSignals.Terminate);
		}

		#region Unused Async code:
		//these lines are used to run the state machine asynchronously:
		//private delegate void Dispatcher(qf4net.QEvent qe);
		//private delegate void FinishDelegate();	

		private void DispatchMessage(ReminderSignals signal)
		{
			ReminderEvent reminderEvent = new ReminderEvent(signal);			
			Reminder.Instance.DispatchQ(reminderEvent);

			//these lines are used to run the state machine asynchronously:		
//			Dispatcher d = new Dispatcher(Reminder.Instance.DispatchQ);
//			IAsyncResult ar = d.BeginInvoke(calcEvent, new AsyncCallback(Finished), d);
		}


		private void Finished(IAsyncResult iar)
		{
			//these lines are used to run the state machine asynchronously:		
//			Dispatcher d = (Dispatcher)iar.AsyncState;
//			d.EndInvoke(iar);
//			this.buttonStop.Enabled = true;
		}
		#endregion

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			DispatchMessage(ReminderSignals.TimerTick);
		}

		//conforming to the example C++ code from Samek
		public void SetTimer()
		{
			this.timer1.Start();
		}

		//conforming to the example C++ code from Samek
		public void KillTimer()
		{
			this.timer1.Stop();
		}

		private void UpdateReminderStatus(object sender, ReminderDisplayEventArgs e)
		{
			this.textBoxHistory.Text += "\r\n" + e.Message;
			this.textBoxState.Text = e.Message;
		}

		private void UpdatePollStatus(object sender, ReminderDisplayEventArgs e)
		{
			this.textBoxPollingCtr.Text = e.Message;
		}

		private void UpdateProcStatus(object sender, ReminderDisplayEventArgs e)
		{
			this.textBoxProcessingCtr.Text = e.Message;
		}

		private MainForm()
		{			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			Reminder.Instance.DisplayState += new Reminder.ReminderDisplayHandler(UpdateReminderStatus);	
			Reminder.Instance.DisplayPoll += new Reminder.ReminderDisplayHandler(UpdatePollStatus);	
			Reminder.Instance.DisplayProc += new Reminder.ReminderDisplayHandler(UpdateProcStatus);
	
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
			this.components = new System.ComponentModel.Container();
			this.labelState = new System.Windows.Forms.Label();
			this.textBoxState = new System.Windows.Forms.TextBox();
			this.textBoxHistory = new System.Windows.Forms.RichTextBox();
			this.buttonStop = new System.Windows.Forms.Button();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.textBoxPollingCtr = new System.Windows.Forms.TextBox();
			this.labelPollingCtr = new System.Windows.Forms.Label();
			this.textBoxProcessingCtr = new System.Windows.Forms.TextBox();
			this.labelProcessingCtr = new System.Windows.Forms.Label();
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
			this.textBoxState.Text = "Polling";
			// 
			// textBoxHistory
			// 
			this.textBoxHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxHistory.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textBoxHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxHistory.Location = new System.Drawing.Point(19, 166);
			this.textBoxHistory.Name = "textBoxHistory";
			this.textBoxHistory.ReadOnly = true;
			this.textBoxHistory.Size = new System.Drawing.Size(314, 360);
			this.textBoxHistory.TabIndex = 0;
			this.textBoxHistory.Text = "State History:";
			// 
			// buttonStop
			// 
			this.buttonStop.Location = new System.Drawing.Point(125, 9);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(86, 27);
			this.buttonStop.TabIndex = 21;
			this.buttonStop.Text = "Exit";
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// timer1
			// 
			this.timer1.Interval = 500;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// textBoxPollingCtr
			// 
			this.textBoxPollingCtr.Location = new System.Drawing.Point(144, 92);
			this.textBoxPollingCtr.Name = "textBoxPollingCtr";
			this.textBoxPollingCtr.ReadOnly = true;
			this.textBoxPollingCtr.Size = new System.Drawing.Size(192, 22);
			this.textBoxPollingCtr.TabIndex = 23;
			this.textBoxPollingCtr.TabStop = false;
			this.textBoxPollingCtr.Text = "0";
			// 
			// labelPollingCtr
			// 
			this.labelPollingCtr.Location = new System.Drawing.Point(29, 92);
			this.labelPollingCtr.Name = "labelPollingCtr";
			this.labelPollingCtr.Size = new System.Drawing.Size(105, 19);
			this.labelPollingCtr.TabIndex = 22;
			this.labelPollingCtr.Text = "Polling Counter:";
			this.labelPollingCtr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxProcessingCtr
			// 
			this.textBoxProcessingCtr.Location = new System.Drawing.Point(144, 129);
			this.textBoxProcessingCtr.Name = "textBoxProcessingCtr";
			this.textBoxProcessingCtr.ReadOnly = true;
			this.textBoxProcessingCtr.Size = new System.Drawing.Size(192, 22);
			this.textBoxProcessingCtr.TabIndex = 25;
			this.textBoxProcessingCtr.TabStop = false;
			this.textBoxProcessingCtr.Text = "0";
			// 
			// labelProcessingCtr
			// 
			this.labelProcessingCtr.Location = new System.Drawing.Point(29, 129);
			this.labelProcessingCtr.Name = "labelProcessingCtr";
			this.labelProcessingCtr.Size = new System.Drawing.Size(105, 19);
			this.labelProcessingCtr.TabIndex = 24;
			this.labelProcessingCtr.Text = "Processing Ctr:";
			this.labelProcessingCtr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(352, 541);
			this.Controls.Add(this.textBoxProcessingCtr);
			this.Controls.Add(this.labelProcessingCtr);
			this.Controls.Add(this.textBoxPollingCtr);
			this.Controls.Add(this.labelPollingCtr);
			this.Controls.Add(this.buttonStop);
			this.Controls.Add(this.textBoxHistory);
			this.Controls.Add(this.textBoxState);
			this.Controls.Add(this.labelState);
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(304, 250);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Reminder Pattern";
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

}//namespace
