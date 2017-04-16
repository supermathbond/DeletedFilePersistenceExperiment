using System;
using System.Linq;
using System.Windows.Forms;

namespace ExperimentMakerNew
{
    public partial class MainForm : Form
    {
        public const string WEBSITE_ADDRESS = "http://130.211.98.43/dfpt";

        public MainForm()
        {
            InitializeComponent();

            comboBox_clients.DropDownStyle = ComboBoxStyle.DropDownList;
            textBox_tempFolder.Text = AppDomain.CurrentDomain.BaseDirectory;

            ToolTip tool = new ToolTip();
            tool.SetToolTip(label_ClientId, "מופיע בקובץ הקונפיגורציה שנוצר לאחר הרצה של הסוכן");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) 
                return;

            this.Enabled = false;

            try
            {                
                while (true)
                {
                    try
                    {
                        string parmaeters = string.Format("DateOfAttack={0}&TimeGap={1}&ClientID={2}&TempFolder={3}&NumOfClustersToCreate={4}&ClusterData={5}",
                                               dateTimePicker.Value.ToString("MM-dd-yyyy HH:mm:ss").Replace(" ", "%20"),
                                               (int)numericUpDown_CheckMinutes.Value * 1000 * 60,
                                               comboBox_clients.Items[comboBox_clients.SelectedIndex],
                                               textBox_tempFolder.Text.Replace(" ", "%20").Replace("\\", "\\\\"),        
                                               numericUpDown_numOfFiles.Value, RandomBase64Cluster());
                        string res = Common.SendPostRequest(WEBSITE_ADDRESS + "/AddExperiment.php", parmaeters);

                        MessageBox.Show(res);
                        break;
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error occured while calculating which files to use in attack: " + exception);
            }

            this.Enabled = true;
        }

        private string RandomBase64Cluster()
        {
            Random rnd = new Random();
            byte[] b = new byte[4096];
            rnd.NextBytes(b);
            return Convert.ToBase64String(b);
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrEmpty(textBox_tempFolder.Text))
            {
                MessageBox.Show("שימו ערך בתיקיה הזמנית!");
                return false;
            }
            if (string.IsNullOrEmpty(comboBox_clients.Text))
            {
                MessageBox.Show("נא למלא ClientId!");
                return false;
            }
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string url = WEBSITE_ADDRESS + "/GetClientsAndTempFolder.php";
            
            string response = Common.SendGetRequest(url);
            var rows = response.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
            var list = rows.Select(row => new { rowArr = row.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) }).
                Select(t => int.Parse(t.rowArr[0])).ToList();
            list.Sort();
            comboBox_clients.DataSource = list;
        }

        private void comboBox_clients_SelectedIndexChanged(object sender, EventArgs e)
        {
            string url = WEBSITE_ADDRESS + "/GetClientDetails.php?clientId=" + comboBox_clients.Items[comboBox_clients.SelectedIndex];

            string response = Common.SendGetRequest(url);
            var rows = response.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries).ToList();

            label_client_description.Text = (rows.Count <= 1) ? "" : Convert.ToString(rows[1]);
            textBox_tempFolder.Text = (rows.Count == 0) ? "" : Convert.ToString(rows[0]);
        }
    }
}
