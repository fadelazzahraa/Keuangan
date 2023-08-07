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
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace Keuangan
{
    public partial class FormCashFlow : Form
    {

        private User user;
        private BindingList<Record> records;
        private List<List<string>> photos;
        private Record selectedRecord = null;

        public FormCashFlow(User loggedinuser)
        {
            user = loggedinuser;
            InitializeComponent();
        }

        private async void LoadRecordData()
        {
            records = new BindingList<Record>();
            dataGridView1.DataSource = null;
            ChangeProgressBarState(true);
            try
            {

                string responseData = await Connection.GetAuthorizedDataAsync(Connection.getRecordsURL, user.Token);

                Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                JArray datas = (JArray)responseDataDictionary["data"];

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
                dataGridView1.Columns["transaction"].HeaderText = "Transaksi";
                dataGridView1.Columns["valueRecord"].HeaderText = "Nilai";
                dataGridView1.Columns["date"].HeaderText = "Tanggal";
                dataGridView1.Columns["detail"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while making the request: " + ex.Message);
            }
            ChangeProgressBarState(false);
        }

        private async void LoadPhotoData()
        {
            photos = new List<List<string>>();
            ChangeProgressBarState(true);
            try
            {
                string responseData = await Connection.GetAuthorizedDataAsync(Connection.getPhotoURL, user.Token);

                Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                JArray datas = (JArray)responseDataDictionary["data"];

                comboBox2.Items.Add("null");
                
                photos.Add(new List<string>
                {
                    "0",
                    "null"
                });

                foreach (var selectedData in datas)
                {
                    string id = (string)selectedData["id"];
                    string tag = (string)selectedData["tag"];
                    photos.Add(new List<string>
                    {
                        id,
                        tag,
                    });

                    comboBox2.Items.Add($"{id} - {tag}");

                };
                comboBox2.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while making the request: " + ex.Message);
            }
            ChangeProgressBarState(false);
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
                ChangeProgressBarState(true);
                selectedRecord = records[e.RowIndex];
                comboBox1.SelectedIndex = selectedRecord.Transaction == "debit" ? 0 : 1;
                textBox3.Text = selectedRecord.ValueRecord.ToString();
                textBox4.Text = selectedRecord.Detail;
                dateTimePicker1.Value = selectedRecord.Date;
                textBox6.Text = selectedRecord.Tag;

                if (selectedRecord.PhotoRecordId != null)
                {
                    for (int i = 0; i < photos.Count; i++)
                    {
                        if (photos[i].Contains(selectedRecord.PhotoRecordId.ToString()))
                        {
                            comboBox2.SelectedIndex = i;
                            break;
                        }
                    }

                } else
                {
                    comboBox2.SelectedIndex = 0;
                }
                ChangeProgressBarState(false);

                SetPictureBoxImage();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to proceed to add record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                ChangeProgressBarState(true);
                try
                {
                    string transaction = comboBox1.SelectedIndex == 0 ? "debit" : "credit";
                    float value = float.Parse(textBox3.Text);
                    string detail = textBox4.Text;
                    DateTime date = dateTimePicker1.Value;
                    string dateformatted = date.ToString("yyyy-MM-dd");
                    string tag = textBox6.Text;

                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "actor", "me" },
                        { "transaction", transaction },
                        { "value", value.ToString() },
                        { "detail", detail },
                        { "date", $"{dateformatted}" },
                        { "tag", tag },
                        { "sourceRecordId", "1" },
                        { "photoRecordId", comboBox2.SelectedIndex != 0 ? photos[comboBox2.SelectedIndex][0] : null},
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
                ChangeProgressBarState(false);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to proceed to edit record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                ChangeProgressBarState(true);
                try
                {
                    string transaction = comboBox1.SelectedIndex == 0 ? "debit" : "credit";
                    float value = float.Parse(textBox3.Text);
                    string detail = textBox4.Text;
                    DateTime date = dateTimePicker1.Value;
                    string dateformatted = date.ToString("yyyy-MM-dd");
                    string tag = textBox6.Text;

                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "actor", "me" },
                        { "transaction", transaction },
                        { "value", value.ToString() },
                        { "detail", detail },
                        { "date", $"{dateformatted}" },
                        { "tag", tag },
                        { "sourceRecordId", "1" },
                        { "photoRecordId", comboBox2.SelectedIndex != 0 ? photos[comboBox2.SelectedIndex][0] : null},
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
                ChangeProgressBarState(false);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to proceed to delete record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                ChangeProgressBarState(true);
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
                ChangeProgressBarState(false);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPictureBoxImage();

        }

        private async void SetPictureBoxImage()
        {
            pictureBox4.Image = null;
            if (comboBox2.SelectedIndex != 0)
            {
                ChangeProgressBarState(true);
                try
                {
                    int indexImage = Int32.Parse(photos[comboBox2.SelectedIndex][0]);
                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("Authorization", user.Token);
                        byte[] imageData = await httpClient.GetByteArrayAsync(Connection.getPhotoImageWithIndexURL(indexImage));
                        using (var stream = new System.IO.MemoryStream(imageData))
                        {
                            Image fetchedImage = Image.FromStream(stream);
                            pictureBox4.Image = fetchedImage;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load the image. Error: {ex.Message}");
                    pictureBox4.Image.Dispose();
                }
                ChangeProgressBarState(false);
            }
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

