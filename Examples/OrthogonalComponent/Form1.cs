using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using qf4net;

namespace OrthogonalComponentHsm
{

	/// <summary>
	/// AlarmClock example main form. Please overlook the poor GUI used for this example.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBoxAlarm;
		private System.Windows.Forms.GroupBox groupBoxTime;
		private System.Windows.Forms.TextBox textBoxTime;
		private System.Windows.Forms.Label labelTime;
		private System.Windows.Forms.TextBox textBoxAlarm;
		private System.Windows.Forms.Label labelAlarm;
		private System.Windows.Forms.RadioButton radioButtonAlarmOn;
		private System.Windows.Forms.RadioButton radioButtonAlarmOff;
		private System.Windows.Forms.RadioButton radioButton12HourTime;
		private System.Windows.Forms.RadioButton radioButton24HourTime;
		private System.Windows.Forms.Button buttonStop;//exit button
		private System.Windows.Forms.PictureBox pictureBoxAlarm;
		private System.Windows.Forms.Button buttonStart;
		private System.Windows.Forms.Label label1;//only used for timing how long picture is shown
		private System.ComponentModel.IContainer components = null;
		
		//in this example, nothing works until after the start button is clicked once
		private void buttonStart_Click(object sender, System.EventArgs e)
		{
			this.buttonStart.Visible = false; //start button is used only once in place of the init signal
			
			this.textBoxAlarm.Enabled = true;
			this.radioButtonAlarmOn.Enabled = true;
			this.radioButtonAlarmOff.Enabled = true;
			this.radioButton12HourTime.Enabled = true;
			this.radioButton24HourTime.Enabled = true;

			//now listen to events from Alarm Clock
			AlarmClock.Instance.DisplayAlarmTime += new AlarmClock.AlarmDisplayHandler(UpdateAlarmTime);	
			AlarmClock.Instance.DisplayTimeOfDay += new AlarmClock.AlarmDisplayHandler(UpdateTimeOfDay);	
			AlarmClock.Instance.DisplayAlarmAlert += new AlarmClock.AlarmDisplayHandler(ShowAlarmPicture);

			DispatchMessage(AlarmClockSignals.Start);

			this.radioButtonAlarmOn.Checked = true;//an optional convenience

		}

		/// <summary>
		/// Exits Application
		/// </summary>
		private void buttonStop_Click(object sender, System.EventArgs e)
		{
			DispatchMessage(AlarmClockSignals.Terminate);
		}

		private void DispatchMessage(Signal signal)
		{
			AlarmInitEvent e = new AlarmInitEvent(signal);			
			AlarmClock.Instance.DispatchQ(e);
		}

		private void radioButtonAlarm_CheckedChanged(object sender, System.EventArgs e)
		{
			this.HideAlarmPicture();

			//The ideal alarm time text input validator would be another
			//state machine. However, that would complicate this example.
			//Therefore, we take a shortcut and don't fully validate input.
			if (this.radioButtonAlarmOn.Checked == true)
			{
				try
				{
					AlarmClock.Instance.AlarmTime = DateTime.Parse(this.textBoxAlarm.Text);
					DispatchMessage(AlarmClockSignals.AlarmOn);	
				}
				catch (System.FormatException exception)
				{
					radioButtonAlarmOn.Checked = false;
					MessageBox.Show(exception.Message, "Invalid Alarm Time");
				}				
			}
			else
			{
				DispatchMessage(AlarmClockSignals.AlarmOff);
			}
		}

		private void radioButton12or24HourTime_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.radioButton12HourTime.Checked == true)
			{
				DispatchMessage(AlarmClockSignals.Mode12Hour);	
			}
			else
			{
				DispatchMessage(AlarmClockSignals.Mode24Hour);	
			}
		}


		private void UpdateTimeOfDay(object sender, AlarmClockEventArgs e)
		{
			// Is this method on the UI thread? If not then we need to marshal it to the UI thread.
			if (this.InvokeRequired)
			{
				AlarmClock.AlarmDisplayHandler alarmDisplayHandler = new AlarmClock.AlarmDisplayHandler(UpdateTimeOfDay);
				Invoke(alarmDisplayHandler, new object[] {sender, e});
			}
			else
			{
				this.textBoxTime.Text = e.Message;
			}
		}

		private void UpdateAlarmTime(object sender, AlarmClockEventArgs e)
		{
			// Is this method on the UI thread? If not then we need to marshal it to the UI thread.
			if (this.InvokeRequired)
			{
				AlarmClock.AlarmDisplayHandler alarmDisplayHandler = new AlarmClock.AlarmDisplayHandler(UpdateAlarmTime);
				Invoke(alarmDisplayHandler, new object[] {sender, e});
			}
			else
			{
				this.textBoxAlarm.Text = e.Message;
			}
		}

		private void ShowAlarmPicture(object sender, AlarmClockEventArgs e)
		{
			// Is this method on the UI thread? If not then we need to marshal it to the UI thread.
			if (this.InvokeRequired)
			{
				AlarmClock.AlarmDisplayHandler alarmDisplayHandler = new AlarmClock.AlarmDisplayHandler(ShowAlarmPicture);
				Invoke(alarmDisplayHandler, new object[] {sender, e});
			}
			else
			{
				this.pictureBoxAlarm.Show();
				//this.Refresh();
			}
		}

		private void HideAlarmPicture()
		{
			this.pictureBoxAlarm.Hide();
			//this.Refresh();
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{	
			//show this only after the alarm activates
			this.pictureBoxAlarm.Hide();

			//disable these until after Start is clicked
			this.textBoxAlarm.Enabled = false;
			this.radioButtonAlarmOn.Enabled = false;
			this.radioButtonAlarmOff.Enabled = false;
			this.radioButton12HourTime.Enabled = false;
			this.radioButton24HourTime.Enabled = false;
		}

		private MainForm()
		{			
			InitializeComponent();			
	
		}//ctor

		//
		//Thread-safe implementation of singleton -- not strictly necessary for this
		//example project
		//
		private static volatile MainForm singleton = null;
		private static object sync = new object();//for static lock

		public static MainForm Instance
		{
			[System.Diagnostics.DebuggerStepThrough()]
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
				this.pictureBoxAlarm.Dispose();

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.groupBoxAlarm = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.radioButtonAlarmOff = new System.Windows.Forms.RadioButton();
			this.radioButtonAlarmOn = new System.Windows.Forms.RadioButton();
			this.textBoxAlarm = new System.Windows.Forms.TextBox();
			this.labelAlarm = new System.Windows.Forms.Label();
			this.groupBoxTime = new System.Windows.Forms.GroupBox();
			this.radioButton24HourTime = new System.Windows.Forms.RadioButton();
			this.radioButton12HourTime = new System.Windows.Forms.RadioButton();
			this.textBoxTime = new System.Windows.Forms.TextBox();
			this.labelTime = new System.Windows.Forms.Label();
			this.pictureBoxAlarm = new System.Windows.Forms.PictureBox();
			this.buttonStop = new System.Windows.Forms.Button();
			this.buttonStart = new System.Windows.Forms.Button();
			this.groupBoxAlarm.SuspendLayout();
			this.groupBoxTime.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxAlarm
			// 
			this.groupBoxAlarm.Controls.Add(this.label1);
			this.groupBoxAlarm.Controls.Add(this.radioButtonAlarmOff);
			this.groupBoxAlarm.Controls.Add(this.radioButtonAlarmOn);
			this.groupBoxAlarm.Controls.Add(this.textBoxAlarm);
			this.groupBoxAlarm.Controls.Add(this.labelAlarm);
			this.groupBoxAlarm.Location = new System.Drawing.Point(10, 18);
			this.groupBoxAlarm.Name = "groupBoxAlarm";
			this.groupBoxAlarm.Size = new System.Drawing.Size(240, 130);
			this.groupBoxAlarm.TabIndex = 0;
			this.groupBoxAlarm.TabStop = false;
			this.groupBoxAlarm.Text = "Alarm";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(106, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(110, 19);
			this.label1.TabIndex = 4;
			this.label1.Text = "(input not validated)";
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// radioButtonAlarmOff
			// 
			this.radioButtonAlarmOff.Checked = true;
			this.radioButtonAlarmOff.Location = new System.Drawing.Point(134, 83);
			this.radioButtonAlarmOff.Name = "radioButtonAlarmOff";
			this.radioButtonAlarmOff.Size = new System.Drawing.Size(58, 28);
			this.radioButtonAlarmOff.TabIndex = 3;
			this.radioButtonAlarmOff.TabStop = true;
			this.radioButtonAlarmOff.Text = "Off";
			this.radioButtonAlarmOff.CheckedChanged += new System.EventHandler(this.radioButtonAlarm_CheckedChanged);
			// 
			// radioButtonAlarmOn
			// 
			this.radioButtonAlarmOn.Location = new System.Drawing.Point(29, 83);
			this.radioButtonAlarmOn.Name = "radioButtonAlarmOn";
			this.radioButtonAlarmOn.Size = new System.Drawing.Size(57, 28);
			this.radioButtonAlarmOn.TabIndex = 2;
			this.radioButtonAlarmOn.Text = "On";
			this.radioButtonAlarmOn.CheckedChanged += new System.EventHandler(this.radioButtonAlarm_CheckedChanged);
			// 
			// textBoxAlarm
			// 
			this.textBoxAlarm.Location = new System.Drawing.Point(96, 46);
			this.textBoxAlarm.Name = "textBoxAlarm";
			this.textBoxAlarm.Size = new System.Drawing.Size(125, 22);
			this.textBoxAlarm.TabIndex = 1;
			this.textBoxAlarm.TabStop = false;
			this.textBoxAlarm.Text = "12:01:01";
			this.textBoxAlarm.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelAlarm
			// 
			this.labelAlarm.Location = new System.Drawing.Point(14, 46);
			this.labelAlarm.Name = "labelAlarm";
			this.labelAlarm.Size = new System.Drawing.Size(82, 19);
			this.labelAlarm.TabIndex = 0;
			this.labelAlarm.Text = "Alarm Time:";
			this.labelAlarm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxTime
			// 
			this.groupBoxTime.Controls.Add(this.radioButton24HourTime);
			this.groupBoxTime.Controls.Add(this.radioButton12HourTime);
			this.groupBoxTime.Controls.Add(this.textBoxTime);
			this.groupBoxTime.Controls.Add(this.labelTime);
			this.groupBoxTime.Location = new System.Drawing.Point(10, 175);
			this.groupBoxTime.Name = "groupBoxTime";
			this.groupBoxTime.Size = new System.Drawing.Size(336, 116);
			this.groupBoxTime.TabIndex = 1;
			this.groupBoxTime.TabStop = false;
			this.groupBoxTime.Text = "TimeKeeping";
			// 
			// radioButton24HourTime
			// 
			this.radioButton24HourTime.Checked = true;
			this.radioButton24HourTime.Location = new System.Drawing.Point(202, 74);
			this.radioButton24HourTime.Name = "radioButton24HourTime";
			this.radioButton24HourTime.Size = new System.Drawing.Size(124, 28);
			this.radioButton24HourTime.TabIndex = 3;
			this.radioButton24HourTime.TabStop = true;
			this.radioButton24HourTime.Text = "24 Hour Mode";
			this.radioButton24HourTime.CheckedChanged += new System.EventHandler(this.radioButton12or24HourTime_CheckedChanged);
			// 
			// radioButton12HourTime
			// 
			this.radioButton12HourTime.Location = new System.Drawing.Point(29, 74);
			this.radioButton12HourTime.Name = "radioButton12HourTime";
			this.radioButton12HourTime.Size = new System.Drawing.Size(125, 28);
			this.radioButton12HourTime.TabIndex = 2;
			this.radioButton12HourTime.Text = "12 Hour Mode";
			this.radioButton12HourTime.CheckedChanged += new System.EventHandler(this.radioButton12or24HourTime_CheckedChanged);
			// 
			// textBoxTime
			// 
			this.textBoxTime.Location = new System.Drawing.Point(130, 28);
			this.textBoxTime.Name = "textBoxTime";
			this.textBoxTime.ReadOnly = true;
			this.textBoxTime.Size = new System.Drawing.Size(192, 22);
			this.textBoxTime.TabIndex = 1;
			this.textBoxTime.TabStop = false;
			this.textBoxTime.Text = "12:00:00";
			this.textBoxTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelTime
			// 
			this.labelTime.Location = new System.Drawing.Point(14, 28);
			this.labelTime.Name = "labelTime";
			this.labelTime.Size = new System.Drawing.Size(106, 18);
			this.labelTime.TabIndex = 0;
			this.labelTime.Text = "Time Of Day:";
			this.labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pictureBoxAlarm
			// 
			this.pictureBoxAlarm.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxAlarm.Image")));
			this.pictureBoxAlarm.Location = new System.Drawing.Point(269, 18);
			this.pictureBoxAlarm.Name = "pictureBoxAlarm";
			this.pictureBoxAlarm.Size = new System.Drawing.Size(100, 115);
			this.pictureBoxAlarm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBoxAlarm.TabIndex = 25;
			this.pictureBoxAlarm.TabStop = false;
			// 
			// buttonStop
			// 
			this.buttonStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonStop.Location = new System.Drawing.Point(365, 185);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(29, 101);
			this.buttonStop.TabIndex = 2;
			this.buttonStop.Text = "E X I T";
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// buttonStart
			// 
			this.buttonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonStart.Location = new System.Drawing.Point(269, 18);
			this.buttonStart.Name = "buttonStart";
			this.buttonStart.Size = new System.Drawing.Size(120, 133);
			this.buttonStart.TabIndex = 26;
			this.buttonStart.Text = "Start";
			this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(400, 310);
			this.Controls.Add(this.buttonStart);
			this.Controls.Add(this.buttonStop);
			this.Controls.Add(this.groupBoxAlarm);
			this.Controls.Add(this.groupBoxTime);
			this.Controls.Add(this.pictureBoxAlarm);
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Orthogonal Component Pattern";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.groupBoxAlarm.ResumeLayout(false);
			this.groupBoxTime.ResumeLayout(false);
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

}//namespace OrthogonalComponentHsm
