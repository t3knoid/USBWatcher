using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Reflection;

namespace TestUSBWatcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            textBox1.BeginInvoke(new Action(() => { textBox1.AppendText("********** USB plugged in **********"); }));
            textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));

            //string driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
            //textBox1.BeginInvoke(new Action(() => { textBox1.AppendText("DriveName = " + driveName); }));
            //textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));
            
            IDictionary<string, string> properties = new Dictionary<string, string>();

            foreach (var property in e.NewEvent.Properties)
            {
                if (property.Value != null)
                {
                    properties[property.Name] = property.Value.ToString();
                    textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(property.Name + " = " + property.Value); }));
                    textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));
                }
            }

            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                if (property.Value != null)
                {
                    properties[property.Name] = property.Value.ToString();
                    textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(property.Name + " = " + property.Value); }));
                    textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));
                }
            }

            textBox1.BeginInvoke(new Action(() => { textBox1.AppendText("********** Drive Properties **********"); }));
            textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));
            System.Threading.Thread.Sleep(3000);
            SystemUSBDrives systemUSBDrives = new SystemUSBDrives("");
            var USBDrivesEnum = systemUSBDrives.GetUSBDrivesInfo("", "", "");
            
            foreach (SystemUSBDrives.USBDriveInfo info in USBDrivesEnum)
            {
                PropertyInfo[] p = typeof(SystemUSBDrives.USBDriveInfo).GetProperties();
                foreach (PropertyInfo property in p)
                {
                    textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(property.Name + " = " + property.GetValue(info, null)); }));
                    textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));
                }
            }

        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            textBox1.BeginInvoke(new Action(() => { textBox1.AppendText("********** USB plugged out **********"); }));
            textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));

            //string driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
            //textBox1.BeginInvoke(new Action(() => { textBox1.AppendText("DriveName = " + driveName); }));
            //textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));

            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(property.Name + " = " + property.Value); }));
                textBox1.BeginInvoke(new Action(() => { textBox1.AppendText(Environment.NewLine); }));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            //WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();

            // Do something while waiting for events
            System.Threading.Thread.Sleep(20000000);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Done");
        }
    }
}
