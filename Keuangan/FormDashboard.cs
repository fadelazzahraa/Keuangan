using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Keuangan
{
    public partial class FormDashboard : Form
    {
        private User user;
        private List<Record> records;

        public FormDashboard(User loggedinuser)
        {
            user = loggedinuser;
            InitializeComponent();
        }

        private async void LoadRecordData()
        {
            records = new List<Record>();
            ChangeProgressBarState(true);
            try
            {

                string responseData = await Connection.GetAuthorizedDataAsync(Connection.getRecordsURL, user.Token);

                Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                Newtonsoft.Json.Linq.JArray datas = (Newtonsoft.Json.Linq.JArray)responseDataDictionary["data"];

                foreach (var selectedData in datas)
                {
                    int id = (int)selectedData["id"];
                    string transaction = (string)selectedData["transaction"];
                    float value = (float)selectedData["value"];
                    string detail = (string)selectedData["detail"];
                    DateTime date = (DateTime)selectedData["date"];
                    string tag = (string)selectedData["tag"];
                    int? photoRecordId = (int?)selectedData["photoRecordId"];

                    records.Add(new Record(id, transaction, value, detail, date, tag, photoRecordId));
                }

                float balance = 0;
                float debitThisMonth = 0;
                float creditThisMonth = 0;

                foreach (Record record in records)
                {
                    if (record.Transaction == "debit")
                    {
                        balance += record.ValueRecord;
                    }
                    else
                    {
                        balance -= record.ValueRecord;
                    }

                    if (record.Transaction == "debit" && record.Date.Month == DateTime.Now.Month)
                    {
                        debitThisMonth += record.ValueRecord;
                    }
                    else if (record.Transaction == "credit" && record.Date.Month == DateTime.Now.Month)
                    {
                        creditThisMonth += record.ValueRecord;
                    }
                }

                CultureInfo info = new CultureInfo("id-ID");

                label11.Text = $"Pemasukan {DateTime.Now.ToString("MMMM", info)}";
                label3.Text = $"Pengeluaran {DateTime.Now.ToString("MMMM", info)}";

                label7.Text = balance.ToString("C", info);
                label7.ForeColor = balance > 0 ? Color.Green : Color.Red;
                label8.Text = debitThisMonth.ToString("C", info);
                label8.ForeColor = Color.Green;
                label9.Text = creditThisMonth.ToString("C", info);
                label9.ForeColor = Color.Red;

                label2.Text = records[records.Count - 1].ValueRecord.ToString("C", info);
                label2.ForeColor = records[records.Count - 1].Transaction == "debit" ? Color.Green : Color.Red;
                label12.Text = records[records.Count - 1].Detail;
                label13.Text = records[records.Count - 1].Date.ToString("dd/MM/yyyy");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while making the request: " + ex.Message);
            }
            ChangeProgressBarState(false);
        }

        private void FormDashboard_Load(object sender, EventArgs e)
        {
            LoadRecordData();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Logout success!");
            this.Hide();
            FormLogin formLogin = new FormLogin();
            formLogin.Closed += (s, args) => this.Close();
            formLogin.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormCashFlow formCashFlow = new FormCashFlow(user);
            formCashFlow.Closed += (s, args) =>
            {
                this.Show();
                LoadRecordData();
            };
            formCashFlow.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormUploadBill formUploadBill = new FormUploadBill(user);
            formUploadBill.Closed += (s, args) =>
            {
                this.Show();
                LoadRecordData();
            };
            formUploadBill.Show();
        }

        private void ChangeProgressBarState(bool isActivated = true)
        {
            if (isActivated)
            {
                toolStripStatusLabel1.Text = "Loading...";
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                toolStripProgressBar1.MarqueeAnimationSpeed = 100;
            }
            else
            {
                toolStripStatusLabel1.Text = "Ready";
                toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
                toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            }
        }
    }
}
