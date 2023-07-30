using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keuangan
{
    public class Connection
    {
        static private string hostURL = "http://178.128.119.144:8080/";
        static public string signinURL = hostURL + "auth/signin";
        static public string getRecordsURL = hostURL + "records?sourceRecordId=1";
        static public string addRecordURL = hostURL + "records";
        static public string editRecordURL(int index)
        {
            return $"{hostURL}records/{index}";
        }
        static public string deleteRecordURL(int index)
        {
            return $"{hostURL}records/{index}";
        }
        static public async Task<string> PostDataAsync(string url, string requestBody)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                StringContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
        }
        static public string uploadPhotoURL = hostURL + "photos/upload";
        static public string postPhotoURL = hostURL + "photos";
        static public string setPhotoURL(int index)
        {
            return $"{hostURL}photos/{index}/image";
        }
        static public string getPhotoURL = hostURL + "photos?detail=Bank Prodesk Photo";

        static public async Task<string> PostAuthorizedDataAsync(string url, string requestBody, string token)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                StringContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                
                httpClient.DefaultRequestHeaders.Add("Authorization", token);

                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
        }

        static public async Task<string> PostFormDataAuthorizedDataAsync(string url, MultipartFormDataContent formData, string token)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", token);

                HttpResponseMessage response = await httpClient.PostAsync(url, formData);

                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
        }

        static public async Task<string> DeleteAuthorizedDataAsync(string url, string token)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", token);

                HttpResponseMessage response = await httpClient.DeleteAsync(url);

                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
        }

        static public async Task<string> GetAuthorizedDataAsync(string url, string token)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", token);

                HttpResponseMessage response = await httpClient.GetAsync(url);

                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
        }
    }
}
