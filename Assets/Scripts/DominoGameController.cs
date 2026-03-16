using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using NativeWebSocket;
using System.Text;
using System;

public class DominoGameController : MonoBehaviour
{

    [Header("Refs")]
    public ApiClient apiClient;
    [SerializeField] private WebSocketClient webSocketClient;
    [Header("Settings")]
    public float pollInterval = 5f;  // seconds between refreshes
    private Coroutine pollRoutine;
    public bool enablePolling = true;

    [Header("Debug")]
    public string gameId;

    public DominoGame currentGame;

    public DominoTableView tableView;
    private DominoTileUI selectedTile;

    public bool offlineMode = true;

    private bool gameReady = false;

    public static DominoGameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        /*
        webSocketClient.OnGameReceived += HandleGameFromWS;
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
        */

        if (apiClient == null)
        {
            apiClient = FindObjectOfType<ApiClient>();
        }

        //subscribe to WS updates
        webSocketClient.OnGameReceived += HandleGameFromWS;
    }

    private void HandleGameFromWS(DominoGame game)
    {
        if (game == null) return;

        if (game.players == null || game.players.Count == 0)
            return;

        TryApplyGame(currentGame);
    }

    private void TryApplyGame(DominoGame game)
    {
        if (game.players == null || game.players.Count == 0)
            return;

        gameReady = true;

        currentGame = game;
        tableView.currentGame = game;
        tableView.BuildTable();
    }

    private bool IsMyTurn(DominoGame game)
    {
        if (game == null || game.players == null) return false;

        return game.players[game.currentTurnIndex].userId == apiClient.userId;
    }


    public async Task LoadMyGames()
    {/*
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

        if(currentGame.players != null && currentGame.players.Count > 0)
        {

            gameReady = true;
        }

        Debug.Log("Loaded game: " + gameId);

        // Render table
        tableView.currentGame = currentGame;
        tableView.myUserId = "u1";
        tableView.BuildTable();
        */

        string res = await apiClient.Get("/api/games");

        if (string.IsNullOrEmpty(res))
        {
            Debug.LogError("res is empty");
            return;
        }

        DominoGameListResponse list =
        JsonUtility.FromJson<DominoGameListResponse>(res);

        if (list == null || list.games == null || list.games.Count == 0)
        {
            Debug.LogError($"The response is empty  {list} or {list.games} or {list.games.Count} ");
            return;
        }

        currentGame = list.games[0];
        gameId = currentGame._id;

        TryApplyGame(currentGame);

    }

    /*
    private async Task ConnectWebSocket()
    {
        websocket = new WebSocket("ws://localhost:3001");

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
            if (packet == null) return;

            //ignore ws updates until game is initialized
            if(!gameReady)
            {
                Debug.LogWarning("[WS] Game not ready yet. Ignoring update.");
                return;
            }

            if (packet.type == "game_update" && packet.game._id == gameId)
            {
                // Extra safety
                if (packet.game.players == null || packet.game.players.Count == 0)
                {
                    Debug.LogWarning("[WS] Game update received but players missing.");
                    return;
                }

                if (packet.game._id == gameId)
                {
                    ApplyGameState(packet.game);
                }
            }
        };

        await websocket.Connect();
    }
    */
    /*
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
    */
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
            var task = RefreshGame();
            while (!task.IsCompleted)
                yield return null;

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
        if (newState == null)
        {
            return;
        }

        //hard guard
        if (newState.players == null || newState.players.Count == 0)
        {
            Debug.LogWarning("[Unity] Game update received but players not ready yet.");
            return;
        }

        //rebuild on game changed
        bool boardChanged = currentGame == null || newState.board.Count != currentGame.board.Count || newState.currentTurnIndex != currentGame.currentTurnIndex;


        gameReady = true;
        currentGame = newState;
        tableView.currentGame = newState;

        if (boardChanged)
        {
            tableView.BuildTable();
        }
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


    public async void TryPlayTile(Domino tile, Vector2 dropPosition, DominoTileUI tileUI)
    {
        if (tableView.enableLocalPlayTesting)
        {
            tableView.OnTileClicked(tile.left, tile.right);
            return;
        }
        //if not the players turn
        if (!IsMyTurn(currentGame))
        {
            Debug.Log("Not your turn.");
            tileUI.ReturnToHand();
            return;
        }

        BoardEnd end = DecidePlacementFromDrop(tile,dropPosition);

       
        // create a move request
        var req = new MoveRequest
        {
            tile = new int[] { tile.left, tile.right },
            end = end.ToString().ToLower()
        };

        //convert the object to an json string
        string body = JsonConvert.SerializeObject(req);

        //send the move request
        string res = await apiClient.Post(
            $"/api/games/{gameId}/move",
            body
        );

        //if null or empty return
        if (string.IsNullOrEmpty(res))
        {
            tileUI.ReturnToHand();
            return;
        }

        // convert the resoponse back to the Domino game object;
        DominoGame updated = JsonConvert.DeserializeObject<DominoGame>(res);
        //update game state
        ApplyGameState(updated);
    }

    private BoardEnd DecidePlacementFromDrop(Domino tile,Vector2 dropPosition)
    {

        //if nothing is on the board
        if (currentGame.board.Count == 0)
        {
            //the first tile doesn't matter
            return BoardEnd.RIGHT;
        }

        // get the left and right end of the board
        Vector3 leftPos = tableView.GetLeftEndWorldPosition();
        Vector3 rightPos = tableView.GetRightEndWorldPosition();

        float distLeft = Vector2.Distance(dropPosition, leftPos);
        float distRight = Vector2.Distance(dropPosition, rightPos);

        BoardEnd chosenEnd = distLeft < distRight ? BoardEnd.LEFT : BoardEnd.RIGHT;

        int leftEnd = currentGame.board.First.Value.left;
        int rightEnd = currentGame.board.Last.Value.right;

        bool matchLeft = tile.left == leftEnd || tile.right == leftEnd;
        bool matchRight = tile.left == rightEnd || tile.right == rightEnd;

        if (chosenEnd == BoardEnd.LEFT && matchLeft)
            return BoardEnd.LEFT;

        if (chosenEnd == BoardEnd.RIGHT && matchRight)
            return BoardEnd.RIGHT;

        throw new Exception("Tile cannot be placed on that end.");
    }

    private void SetSelectedTile(DominoTileUI tileUI)
    {
        if (selectedTile != null && selectedTile != tileUI)
        {
            selectedTile.setSelected(false);
        }

        selectedTile = tileUI;
    }





    void Update()
    {
    
    }

    private async void OnDestroy()
    {
       
    }
}
