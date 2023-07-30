using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Keuangan
{
    public partial class FormCashFlow : Form
    {

        private User user;
        private BindingList<Record> records;
        private BindingList<int> photos;
        private Record selectedRecord = null;

        public FormCashFlow(User loggedinuser)
        {
            user = loggedinuser;
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private async void LoadRecordData()
        {
            records = new BindingList<Record>();
            dataGridView1.DataSource = null;
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
                };

                dataGridView1.DataSource = records;
                dataGridView1.Columns["id"].Visible = false;
                dataGridView1.Columns["photoRecordId"].Visible = false;


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while making the request: " + ex.Message);
            }
        }

        private async void LoadPhotoData()
        {
            photos = new BindingList<int>();
            try
            {
                string responseData = await Connection.GetAuthorizedDataAsync(Connection.getPhotoURL, user.Token);

                Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                Newtonsoft.Json.Linq.JArray datas = (Newtonsoft.Json.Linq.JArray)responseDataDictionary["data"];

                comboBox2.Items.Add("null");
                foreach (var selectedData in datas)
                {
                    int id = (int)selectedData["id"];
                    photos.Add(id);
                    comboBox2.Items.Add(id);
                };
                comboBox2.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while making the request: " + ex.Message);
            }
        }

        private void FormCashFlow_Load(object sender, EventArgs e)
        {
            LoadRecordData();
            LoadPhotoData();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                selectedRecord = records[e.RowIndex];
                comboBox1.SelectedIndex = selectedRecord.Transaction == "debit" ? 0 : 1;
                textBox3.Text = selectedRecord.ValueRecord.ToString();
                textBox4.Text = selectedRecord.Detail;
                dateTimePicker1.Value = selectedRecord.Date;
                textBox6.Text = selectedRecord.Tag;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to proceed to add record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string transaction = comboBox1.SelectedIndex == 0 ? "debit" : "credit";
                    float value = float.Parse(textBox3.Text);
                    string detail = textBox4.Text;
                    DateTime date = dateTimePicker1.Value;
                    string dateformatted = date.ToString("yyyy-MM-dd");

                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "actor", "me" },
                        { "transaction", transaction },
                        { "value", value.ToString() },
                        { "detail", detail },
                        { "date", $"{dateformatted}" },
                        { "sourceRecordId", "1" },
                    };

                    string requestBody = System.Text.Json.JsonSerializer.Serialize(data);

                    string responseData = await Connection.PostAuthorizedDataAsync(Connection.addRecordURL, requestBody, user.Token);

                    Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                    MessageBox.Show(responseDataDictionary["message"].ToString());
                    LoadRecordData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred while making the request: " + ex.Message);
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to proceed to edit record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string transaction = comboBox1.SelectedIndex == 0 ? "debit" : "credit";
                    float value = float.Parse(textBox3.Text);
                    string detail = textBox4.Text;
                    DateTime date = dateTimePicker1.Value;
                    string dateformatted = date.ToString("yyyy-MM-dd");

                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "actor", "me" },
                        { "transaction", transaction },
                        { "value", value.ToString() },
                        { "detail", detail },
                        { "date", $"{dateformatted}" },
                        { "sourceRecordId", "1" },
                    };

                    string requestBody = System.Text.Json.JsonSerializer.Serialize(data);

                    string responseData = await Connection.PostAuthorizedDataAsync(Connection.editRecordURL(selectedRecord.ID), requestBody, user.Token);

                    Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                    MessageBox.Show(responseDataDictionary["message"].ToString());
                    LoadRecordData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred while making the request: " + ex.Message);
                }
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to proceed to delete record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                    };

                    string responseData = await Connection.DeleteAuthorizedDataAsync(Connection.deleteRecordURL(selectedRecord.ID), user.Token);

                    Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                    MessageBox.Show(responseDataDictionary["message"].ToString());
                    LoadRecordData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred while making the request: " + ex.Message);
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

