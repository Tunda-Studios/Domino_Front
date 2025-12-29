using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine;



public class WebSocketClient : MonoBehaviour
{
    public event System.Action<DominoGame> OnGameReceived;
    private string serverUrl = "ws://localhost:3001";
    
    private WebSocket ws;

    // Start is called before the first frame update
    async void Start()
    {
        

        ws = new WebSocket(serverUrl);
        Debug.Log(ws);
        ws.OnOpen += () => Debug.Log("WS connected");
        ws.OnError += (e) => Debug.LogError("WS error: " + e);
        ws.OnClose += (e) => Debug.Log("WS closed: " + e);

        ws.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            DominoGame game = JsonUtility.FromJson<DominoGame>(json);
            if (game == null) return;
            OnGameReceived?.Invoke(game);

        };

        await ws.Connect();


    }

#if !UNITY_WEBGL || UNITY_EDITOR
    void Update()
    {
        ws?.DispatchMessageQueue();
    }
#endif

    async void OnApplicationQuit()
    {
        if (ws != null)
            await ws.Close();
    }
}
