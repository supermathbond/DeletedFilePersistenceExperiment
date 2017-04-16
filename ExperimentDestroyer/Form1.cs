using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;


namespace ExperimentDestroyer
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> _experimentors = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();

            if (!IsAdministrator())
            {
                MessageBox.Show("Please run as administrator!!");
                Environment.Exit(0);
            }

            comboBox_people.DropDownStyle = ComboBoxStyle.DropDownList;
            _experimentors.Add("Avigail-Work", "C:\\Yoel\\");
            _experimentors.Add("Michal-AvigailFriend", "C:\\df\\");
            _experimentors.Add("Nisan-AvigailFriend", "c:\\Users\\nisan\\df\\");
            _experimentors.Add("Eli-AvigailFriend", "C:\\df\\dfpt-client\\");
            _experimentors.Add("Yasmin-AvigailFriend", "C:\\df\\");

            _experimentors.Add("Yaniv-Laptop", "C:\\Users\\yovitz\\Desktop\\dfpt-client\\");
            _experimentors.Add("Shaboodi", "C:\\dfpt-client\\");
            _experimentors.Add("Peny-Laptop", "C:\\dfpt-client\\");
            _experimentors.Add("Yoel-PC-Home", "C:\\Users\\anonymous\\Desktop\\New folder\\New folder\\papers\\Yoel\\Deleted file persistence experiment\\FFExperiment\\ExperimentRunner\\bin\\Release\\");
            _experimentors.Add("Yoel Lenovo", "C:\\Users\\ygrinstein\\Desktop\\DFPT-CLient\\");
            _experimentors.Add("Family-PC", "C:\\yoel\\DFPT-CLient\\");
            comboBox_people.DataSource = _experimentors.Keys.ToList();
        }

        private void comboBox_people_SelectedIndexChanged(object sender, EventArgs e)
        {
            label_path.Text = _experimentors[(string)comboBox_people.SelectedItem];
            textBox1.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempDb-" + (string)comboBox_people.SelectedItem + ".db");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label_status.Text = "stopping service...";

            if (DoesServiceExist("ExperimentRunner"))
            {
                ServiceController sc = new ServiceController("ExperimentRunner");
                if (!sc.Status.Equals(ServiceControllerStatus.Stopped) && !sc.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    sc.Stop();
                }
            }

            label_status.Text = "copying file...";

            string localDb = Path.Combine(_experimentors[(string)comboBox_people.SelectedItem], "tempDb.db");
            if (File.Exists(localDb))
                File.Copy(localDb, textBox1.Text);
            else if (MessageBox.Show(localDb + " wasn't found should I continue?", "Problem", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
                    

            label_status.Text = "deleting service...";
            Process.Start("sc", "delete ExperimentRunner").WaitForExit();
            Thread.Sleep(2000);

            label_status.Text = "deleting folder...";
            try
            {
                Directory.Delete(_experimentors[(string)comboBox_people.SelectedItem], true);
                label_status.Text = "Done!";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                label_status.Text = "Done! but error while deleting folder. please delete manually.";
            }
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool DoesServiceExist(string serviceName)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(serviceName));
        }

        private void label_path_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetData(DataFormats.Text, label_path.Text);
        }
    }
}
