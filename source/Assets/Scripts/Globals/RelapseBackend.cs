using System;
using System.IO;
using System.Net;
using System.Text;

public static class RelapseBackend
{
    public const string BaseUrl = "http://127.0.0.1:8844";
    public static string DetectedVersion { get; private set; }

    private static HttpWebRequest MakeRequest(string url, string method, int timeoutMs)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = method;
        request.Timeout = timeoutMs;
        request.ReadWriteTimeout = timeoutMs;
        request.KeepAlive = false;
        request.Proxy = null;
        return request;
    }

    public static bool Probe(out string version)
    {
        version = null;

        try
        {
            HttpWebRequest request = MakeRequest(BaseUrl + "/api/version", "GET", 1200);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                int status = (int)response.StatusCode;
                if (status < 200 || status >= 300)
                    return false;

                version = reader.ReadToEnd().Trim();
                if (string.IsNullOrEmpty(version))
                    return false;

                DetectedVersion = version;
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public static bool Install(string pathOrUrl, out string responseText)
    {
        responseText = null;

        if (string.IsNullOrEmpty(pathOrUrl))
        {
            responseText = "Package path/URL is empty.";
            return false;
        }

        try
        {
            string json = "{\"path\":\"" + JsonEscape(pathOrUrl) + "\"}";
            byte[] payload = Encoding.UTF8.GetBytes(json);

            HttpWebRequest request = MakeRequest(BaseUrl + "/api/install", "POST", 5000);
            request.ContentType = "application/json";
            request.ContentLength = payload.Length;

            using (Stream stream = request.GetRequestStream())
                stream.Write(payload, 0, payload.Length);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseText = reader.ReadToEnd();
                int status = (int)response.StatusCode;
                return status >= 200 && status < 300;
            }
        }
        catch (WebException ex)
        {
            responseText = ReadWebException(ex);
            return false;
        }
        catch (Exception ex)
        {
            responseText = ex.Message;
            return false;
        }
    }

    private static string ReadWebException(WebException ex)
    {
        if (ex.Response == null)
            return ex.Message;

        try
        {
            using (Stream responseStream = ex.Response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string body = reader.ReadToEnd();
                return string.IsNullOrEmpty(body) ? ex.Message : body;
            }
        }
        catch
        {
            return ex.Message;
        }
    }

    private static string JsonEscape(string value)
    {
        if (value == null)
            return string.Empty;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
