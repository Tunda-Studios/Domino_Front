using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine;



public class WebSocketClient : MonoBehaviour
{
    public string serverUrl = "ws://localhost:8080";
    public DominoTableView tableView;
    public string myUserId = "u1";

    private WebSocket ws;

    // Start is called before the first frame update
    async void Start()
    {
        tableView.myUserId = myUserId;

        ws = new WebSocket(serverUrl);
        ws.OnOpen += () => Debug.Log("WS connected");
        ws.OnError += (e) => Debug.LogError("WS error: " + e);
        ws.OnClose += (e) => Debug.Log("WS closed: " + e);

        ws.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            DominoGame game = JsonUtility.FromJson<DominoGame>(json);

            // Assign + rebuild table view
            tableView.currentGame = game;
            tableView.BuildTable();
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
