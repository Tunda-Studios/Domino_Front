using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using NativeWebSocket;
using System.Text;


public class DominoGameController : MonoBehaviour
{
    [Header("Refs")]
    public ApiClient apiClient;

    [Header("Settings")]
    public float pollInterval = 5f;  // seconds between refreshes
    private WebSocket websocket;
    private Coroutine pollRoutine;
    public bool enablePolling = true;

    [Header("Debug")]
    public string gameId;

    public DominoGame currentGame;

    public DominoTableView tableView;

    private bool wsConnected = false;

    private async void Start()
    {

        if (apiClient == null)
            apiClient = FindObjectOfType<ApiClient>();

        Debug.Log("DominoGameController started.");

        // 1) Load games (for now just first one)
        await LoadMyGames();

        // 2) Connect WS
        await ConnectWebSocket();

        // 3) Join the WS room for this game
        if (!string.IsNullOrEmpty(gameId))
            await JoinRoom(gameId);

        // 4) Poll in background as fallback
        if (enablePolling)
            StartPolling();
    }

    public async Task LoadMyGames()
    {
        string res = await apiClient.Get("/api/games");
        if (string.IsNullOrEmpty(res))
        {
            Debug.LogError("Failed to load games.");
            return;
        }

        DominoGameListResponse list = JsonConvert.DeserializeObject<DominoGameListResponse>(res);
        if (list == null || list.games == null || list.games.Count == 0)
        {
            Debug.Log("[Lobby] No games found.");
            return;
        }

        // Pick first game for demo
        currentGame = list.games[0];
        gameId = currentGame._id;

        Debug.Log("Loaded game: " + gameId);

        // Render table
        tableView.currentGame = currentGame;
        tableView.myUserId = "u1";
        tableView.BuildTable();
    }

    private async Task ConnectWebSocket()
    {
        websocket = new WebSocket("ws://localhost:3000");

        websocket.OnOpen += () =>
        {
            Debug.Log("WS Connected.");
            wsConnected = true;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WS Closed.");
            wsConnected = false;
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("WS Error: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            Debug.Log("WS Message Received: " + msg);

            var packet = JsonConvert.DeserializeObject<GamePacket>(msg);

            if (packet.type == "game_update" && packet.game._id == gameId)
            {
                Debug.Log("WS → Updating board for game " + gameId);
                ApplyGameState(packet.game);
            }
        };

        await websocket.Connect();
    }

    private async Task JoinRoom(string gameId)
    {
        if (!wsConnected)
        {
            Debug.LogWarning("WS not connected yet. Cannot join room.");
            return;
        }

        var obj = new
        {
            type = "join_room",
            gameId = gameId
        };

        string json = JsonConvert.SerializeObject(obj);
        await websocket.SendText(json);

        Debug.Log("Joined WS room for game " + gameId);
    }
    private async Task LoadAndLogGames()
    {
        
        string res = await apiClient.Get("/api/games");
        Debug.Log(res);
        if (string.IsNullOrEmpty(res))
        {
            Debug.LogError("[Lobby] Failed to load games.");
            return;
        }

        DominoGameListResponse list = JsonConvert.DeserializeObject<DominoGameListResponse>(res);

        foreach (var g in list.games)
        {
            Debug.Log("Game ID: " + g._id);
        }
        Debug.Log("the game is " + list.games[0]);

        DominoGame game = list.games[0];
        if (list == null || list.games == null)
        {
            Debug.LogWarning("[Lobby] No games found or parse failed.");
            return;
        }
        

        Debug.Log($"[Lobby] Loaded {list.games.Count} games:");
        Debug.Log("reach");
        foreach (var g in list.games)
        {
            string winnerInfo = string.IsNullOrEmpty(g.matchWinnerUserId)
                ? "Match in progress"
                : $"Winner: {g.matchWinnerUserId}";

            Debug.Log(
                $" - Game {g._id} | Mode: {g.mode} | " +
                $"Status: {g.status} | Round: {g.roundNumber} | " +
                $"Players: {g.players?.Count ?? 0} | {winnerInfo}"
            );
        }

        // 1) Assign the game to the table view
        tableView.currentGame = game;
        tableView.myUserId = "u1";
        
        tableView.BuildTable();
    }

    // ---- Play-at-your-pace polling ----

    public void StartPolling()
    {
        if (pollRoutine != null) StopCoroutine(pollRoutine);
        pollRoutine = StartCoroutine(PollLoop());
    }




    private IEnumerator PollLoop()
    {
        while (!string.IsNullOrEmpty(gameId))
        {
            if (!wsConnected) // use polling ONLY if WS isn't active
            {
                var task = RefreshGame();
                while (!task.IsCompleted) yield return null;
            }

            yield return new WaitForSeconds(pollInterval);
        }
    }

    public async Task RefreshGame()
    {
        if (string.IsNullOrEmpty(gameId)) return;

        string res = await apiClient.Get($"/api/games/{gameId}");
        if (string.IsNullOrEmpty(res)) return;

        var latest = JsonConvert.DeserializeObject<DominoGame>(res);
        ApplyGameState(latest);
    }

    private void ApplyGameState(DominoGame newState)
    {
        currentGame = newState;
        tableView.currentGame = newState;
        tableView.BuildTable();
    }

    public async Task PlayFirstTileRight()
    {
        if (currentGame == null || currentGame.players == null) return;

        // Find "me" (for now we just use currentTurnIndex player)
        var me = currentGame.players[currentGame.currentTurnIndex];

        if (me.hand == null || me.hand.Count == 0)
        {
            Debug.Log("[Unity] No tiles in hand to play.");
            return;
        }

        var tile = me.hand[0];

        var req = new MoveRequest
        {
            tile = tile,
            end = "right"
        };
        string body = JsonConvert.SerializeObject(req);

        string res = await apiClient.Post(
            $"/api/games/{gameId}/move",
            body
        );

        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonConvert.DeserializeObject<DominoGame>(res);

        Debug.Log($"[Unity] Played [{tile[0]},{tile[1]}].");
    }

    public async Task PassTurn()
    {
        var req = new MoveRequest { tile = null, end = "right" };
        string res = await apiClient.Post($"/api/games/{gameId}/move", JsonConvert.SerializeObject(req));

        if (!string.IsNullOrEmpty(res))
        {
            var updated = JsonConvert.DeserializeObject<DominoGame>(res);
            ApplyGameState(updated);
        }
    }


    // ---- Backend interactions ----

    public async Task CreateAndStartTestGame()
    {
        var req = new CreateGameRequest
        {
            mode = "cutthroat",
            displayName = "Rachad", // later from login
            maxPlayers = 4
        };

        string res = await apiClient.Post("/api/games", JsonConvert.SerializeObject(req));
        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonConvert.DeserializeObject<DominoGame>(res);
        gameId = currentGame._id;
        Debug.Log($"[Unity] Created game {gameId}");

        // start first round
        res = await apiClient.Post(
            $"/api/games/{gameId}/start",
            JsonConvert.SerializeObject(new { })
        );

        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonConvert.DeserializeObject<DominoGame>(res);
        Debug.Log($"[Unity] Started round {currentGame.roundNumber}");

        StartPolling();

    }





    void Update()
    {
        websocket?.DispatchMessageQueue();
    }

    private async void OnDestroy()
    {
        await websocket.Close();
    }
}
