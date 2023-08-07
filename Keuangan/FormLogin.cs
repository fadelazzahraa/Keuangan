using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Net;

namespace Keuangan
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private async void Login()
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            button1.Visible = false;
            pictureBox1.Visible = false;
            label4.Visible = false;
            label5.Visible = true;
            try
            {
                // Create a dictionary to represent the data you want to send in the request body
                Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

                // Serialize the data dictionary to JSON string
                string requestBody = System.Text.Json.JsonSerializer.Serialize(data);

                string responseData = await Connection.PostDataAsync(Connection.signinURL, requestBody);

                Dictionary<string, string> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
                if (responseDataDictionary["status"] == "true")
                {
                    User newUser = new User(username, responseDataDictionary["token"]);
                    MessageBox.Show("Login success!");

                    this.Hide();
                    FormDashboard formDashboard = new FormDashboard(newUser);
                    formDashboard.Closed += (s, args) =>
                    {
                        this.Close();
                    };
                    formDashboard.Show();
                }
                else
                {
                    MessageBox.Show("Wrong username/password!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while making the request: " + ex.Message);
            }

            button1.Visible = true;
            pictureBox1.Visible = true;
            label4.Visible = true;
            label5.Visible = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Login();
        }
    }
}
