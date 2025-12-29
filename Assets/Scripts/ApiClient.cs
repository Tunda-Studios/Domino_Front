using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [Header("Server Config")]
    [SerializeField] private string baseUrl = "http://localhost:3001"; // change when hosting
    [SerializeField] public string userId;            

    public void SetUserId(string newUserId)
    {
        userId = newUserId;
    }

    public async Task<string> Get(string path)
    {
        var url = $"{baseUrl}{path}";
        using (var req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("X-User-Id", userId);
           
            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GET {url} failed: {req.result} - {req.error} - {req.downloadHandler.text}");
                return null;
            }

            return req.downloadHandler.text;
        }
    }

    public async Task<string> Post(string path, string jsonBody)
    {
        var url = $"{baseUrl}{path}";
        using (var req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody ?? "{}");
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-User-Id", userId);

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"POST {url} failed: {req.result} - {req.error} - {req.downloadHandler.text}");
                return null;
            }

            return req.downloadHandler.text;
        }
    }

    //  PUT
    public async Task<string> Put(string path, string jsonBody)
    {
        var url = $"{baseUrl}{path}";
        // UnityWebRequest.Put sets method + upload handler, but we override to ensure JSON
        using (var req = UnityWebRequest.Put(url, jsonBody ?? "{}"))
        {
            req.method = "PUT"; // make sure it’s actually PUT
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-User-Id", userId);

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"PUT {url} failed: {req.result} - {req.error} - {req.downloadHandler.text}");
                return null;
            }

            return req.downloadHandler.text;
        }
    }

    //  DELETE 
    public async Task<string> Delete(string path)
    {
        var url = $"{baseUrl}{path}";
        using (var req = UnityWebRequest.Delete(url))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("X-User-Id", userId);

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"DELETE {url} failed: {req.result} - {req.error} - {req.downloadHandler.text}");
                return null;
            }

            return req.downloadHandler.text;
        }
    }
}
