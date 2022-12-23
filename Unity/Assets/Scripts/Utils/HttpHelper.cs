using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;

public class HttpHelper
{
    static readonly HttpClient client = new HttpClient();

    public static async Task<string> TryGetAsync(string api, string token = "")
    {
        string url = $"{ConstValue.API_DOMAIN}/{api}";
        string responseBody = string.Empty;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("platform", "pc");
            if (string.IsNullOrEmpty(token) == false)
                request.Headers.Add("token", token);
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            Debug.WriteLine($"\nException Caught in [{api}]!");
            Debug.WriteLine($"Message :{e.Message} "); //目标计算机积极拒绝(服务器没开)
        }
        return responseBody;
    }
    public static async Task<string> TryPostAsync(string api, string json, string token = "")
    {
        string url = $"{ConstValue.API_DOMAIN}/{api}";
        string responseBody = string.Empty;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("platform", "pc");
            if (string.IsNullOrEmpty(token) == false)
                request.Headers.Add("token", token);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            Debug.WriteLine($"\nException Caught in [{api}]!");
            Debug.WriteLine("Message :{0} ", e.Message);
        }
        return responseBody;
    }
    public static async Task DownloadAsync(Uri uri, string FileName)
    {
        using (var s = await client.GetStreamAsync(uri))
        {
            using (var fs = new FileStream(FileName, FileMode.CreateNew))
            {
                await s.CopyToAsync(fs);
            }
        }
    }
}