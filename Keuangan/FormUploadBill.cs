using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Keuangan
{
    public partial class FormUploadBill : Form
    {
        private HttpClient httpClient;
        private OpenFileDialog openFileDialog;
        private User user;

        public FormUploadBill(User loggedinuser)
        {
            user = loggedinuser;
            InitializeComponent();

            openFileDialog = new OpenFileDialog
            {
                Title = "Select Image File",
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };
        }

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                using (var f = File.OpenRead(selectedFilePath))
                {
                    var content = new StreamContent(f);
                    MultipartFormDataContent mpcontent = new MultipartFormDataContent();
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    mpcontent.Add(content, "file", Path.GetFileName(selectedFilePath));

                    string responseData = await Connection.PostFormDataAuthorizedDataAsync(Connection.uploadPhotoURL, mpcontent, user.Token);

                    Dictionary<string, object> responseDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                    if (responseDataDictionary["status"].ToString() == "True") {
                        string path = responseDataDictionary["path"].ToString();

                        Dictionary<string, string> data = new Dictionary<string, string>
                        {
                            { "detail", "Bank Prodesk Photo" },
                            { "startDate", DateTime.Now.ToString("yyyy-MM-dd")},
                        };

                        string requestBody = System.Text.Json.JsonSerializer.Serialize(data);

                        string responseData2 = await Connection.PostAuthorizedDataAsync(Connection.postPhotoURL, requestBody, user.Token);

                        Dictionary<string, object> responseDataDictionary2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData2);

                        if (responseDataDictionary2["status"].ToString() == "True")
                        {
                            JObject datas = (JObject)responseDataDictionary2["data"];
                            int id = Int32.Parse(datas["id"].ToString());

                            Dictionary<string, string> data2 = new Dictionary<string, string>
                            {
                                { "path", path },
                            };

                            string requestBody2 = System.Text.Json.JsonSerializer.Serialize(data2);

                            string responseData3 = await Connection.PostAuthorizedDataAsync(Connection.setPhotoURL(id), requestBody2, user.Token);

                            Dictionary<string, object> responseDataDictionary3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData3);

                            if (responseDataDictionary3["status"].ToString() == "True")
                            {
                                MessageBox.Show("Upload photo bill success!");
                            } else
                            {
                                MessageBox.Show(responseDataDictionary3["message"].ToString());
                            }
                            
                        }
                        else
                        {
                            MessageBox.Show(responseDataDictionary2["message"].ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show(responseDataDictionary["message"].ToString());
                    }
                }
            } else
            {
                MessageBox.Show("Select correct photo first!");
            }
        }

        private void SelectImageButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;

                pictureBox.Image = Image.FromFile(selectedFilePath);

                uploadButton.Enabled = true;
            }
        }
    }
}
    

