using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class APIServices : SingletonWithGameobject<APIServices>
{
    [SerializeField] private string token;
    [SerializeField] private string baseURL;
    [SerializeField] private string socketURL;
    public string GetBaseUrl => baseURL;
    public string GetSocketUrl => socketURL;

    public void Initialize(string baseURL, string token, string socketURL)
    {
        this.token = token;
        // this.socketURL = socketURL.EndsWith("/") ? socketURL : socketURL + "/";
        this.socketURL = socketURL;
        this.baseURL = baseURL.EndsWith("/") ? baseURL : baseURL + "/";
    }

    public void UpdateToken(string token)
    {
        this.token = token;
    }

    public async Task<T> GetAsync<T>(string url, Dictionary<string, string> attributes = null, bool includeAuthorization = true)
    {
        try
        {
            // Add additional attributes to the URL or request body
            if (attributes != null && attributes.Count > 0)
            {
                int index = 0;
                foreach (var kvp in attributes)
                {
                    if (index == 0)
                        url += $"?{kvp.Key}={kvp.Value}";
                    else
                        url += $"&{kvp.Key}={kvp.Value}";
                    index++;
                }
            }
            using (UnityWebRequest webRequest = UnityWebRequest.Get(baseURL + url))
            {
                // Debug.LogWarning($"REquest URL : {baseURL + url}");
                // Set authorization token
                if (includeAuthorization)
                {
                    if (!string.IsNullOrEmpty(token))
                        webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                    else
                        throw new Exception("Token Not Found : " + token);
                }

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseBody = webRequest.downloadHandler.text;
                    // Debug.LogWarning($"URL: {url}  \n(Responce: {webRequest.downloadHandler.text}");
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"Protocol Error : (URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                    string responseBody = webRequest.downloadHandler.text;
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
                else
                {
                    throw new Exception($"HTTP request error: {webRequest.error} (URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Endpoint: {url}");
            Debug.LogError($"Error: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
            return default;
        }
    }

    public async Task<T> PostAsync<T>(string url, string bodyJson, bool includeAuthorization = true)
    {
        try
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(baseURL + url, "POST"))
            {
                Debug.Log("Post URL : " + (baseURL + url));

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);
                Debug.Log("JSON Body : " + bodyJson);

                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                if (includeAuthorization)
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseBody = webRequest.downloadHandler.text;
                    // Debug.Log($"(URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"Protocol Error : (URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                    string responseBody = webRequest.downloadHandler.text;
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
                else
                {
                    throw new Exception($"HTTP request error: {webRequest.error} (URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Endpoint: {url}");
            Debug.LogError($"Error: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
            return default;
        }
    }

    public async Task<T> PostForm<T>(string url, Dictionary<string, string> data, Dictionary<string, string> files, bool includeAuthorization = true)
    {
        try
        {
            // Create form data
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

            // Add data fields to the form data
            foreach (var kvp in data)
            {
                formData.Add(new MultipartFormDataSection(kvp.Key, kvp.Value));
            }

            // Add file sections to the form data
            foreach (var kvp in files)
            {

                string fileExtension = Path.GetExtension(kvp.Value);

                // Create a file section with appropriate content type based on extension
                string contentType = GetContentTypeByExtension(fileExtension);
                byte[] fileBytes = File.ReadAllBytes(kvp.Value);
                formData.Add(new MultipartFormFileSection(kvp.Key, fileBytes, Path.GetFileName(kvp.Value), contentType));
            }

            // Create and send the request
            using (UnityWebRequest webRequest = UnityWebRequest.Post(baseURL + url, formData))
            {
                //Debug.Log("PostForm URL: " + (baseURL + url));

                if (includeAuthorization)
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseBody = webRequest.downloadHandler.text;
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"Protocol Error : (URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                    string responseBody = webRequest.downloadHandler.text;
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
                else
                {
                    throw new Exception($"HTTP request error: {webRequest.error} (URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Endpoint: {url}");
            Debug.LogError($"Error: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
            return default;
        }
    }


    public async Task<T> PutAsync<T>(string url, string bodyJson = null, bool includeAuthorization = true)
    {
        try
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(baseURL + url, "PUT"))
            {
                // Debug.Log("Put URL : " + (baseURL + url));


                // Debug.Log("JSON Body : " + bodyJson);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                if (includeAuthorization)
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseBody = webRequest.downloadHandler.text;
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;

                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"Protocol Error : (URL: {url})  \n(Responce: {webRequest.downloadHandler.text}");
                    string responseBody = webRequest.downloadHandler.text;
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    return result;
                }
                else
                {
                    throw new Exception($"HTTP request error: {webRequest.error} (URL: {url}) \n(Responce: {webRequest.downloadHandler.text})");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Endpoint: {url}");
            Debug.LogError($"Error: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
            return default;
        }
    }

    private static string GetContentTypeByExtension(string extension)
    {
        switch (extension.ToLower())
        {
            case ".txt":
                return "text/plain";
            case ".png":
                return "image/png";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            // Add more content types as needed
            default:
                return "application/octet-stream"; // Default for unknown extensions
        }
    }

}