using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class DominoGameController : MonoBehaviour
{
    [Header("Refs")]
    public ApiClient apiClient;

    [Header("Settings")]
    public float pollInterval = 5f;  // seconds between refreshes

    [Header("Debug")]
    public string gameId;
    public DominoGame currentGame;

    private Coroutine pollRoutine;

    private async void Start()
    {
        // For now, we can auto-create a test game.
        // Remove this in production and drive from UI.
        if (apiClient == null)
        {
            apiClient = FindObjectOfType<ApiClient>();
        }

        Debug.Log("api client created");
        LoadMyGames();
        // Example: create a test game and start it
        // await CreateAndStartTestGame();
        await LoadAndLogGames();
    }

    public async void LoadMyGames()
    {
        await LoadAndLogGames();
    }

    private async Task LoadAndLogGames()
    {
        string res = await apiClient.Get("/api/games");
        Debug.Log(await apiClient.Get("/api/games"));
        if (string.IsNullOrEmpty(res))
        {
            Debug.LogError("[Lobby] Failed to load games.");
            return;
        }
        Debug.Log(res);
        DominoGameListResponse list = JsonUtility.FromJson<DominoGameListResponse>(res);
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
            var t = RefreshGame();
            while (!t.IsCompleted) yield return null;
            yield return new WaitForSeconds(pollInterval);
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

        string res = await apiClient.Post("/api/games", JsonUtility.ToJson(req));
        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonUtility.FromJson<DominoGame>(res);
        gameId = currentGame._id;
        Debug.Log($"[Unity] Created game {gameId}");

        // start first round
        res = await apiClient.Post($"/api/games/{gameId}/start", "{}");
        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonUtility.FromJson<DominoGame>(res);
        Debug.Log($"[Unity] Started round {currentGame.roundNumber}");

        StartPolling();
    }

    public async Task RefreshGame()
    {
        if (string.IsNullOrEmpty(gameId)) return;

        string res = await apiClient.Get($"/api/games/{gameId}");
        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonUtility.FromJson<DominoGame>(res);

        Debug.Log(
            $"[Unity] Game {gameId} | Status: {currentGame.status} | " +
            $"Round: {currentGame.roundNumber} | TargetWins: {currentGame.targetWins}"
        );

        // TODO: update your UI – hands, board, current player, wins etc.
        // This is where you'd re-render domino pieces.
    }

    // Example: click handler that plays *first tile* from your hand to the right
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

        string res = await apiClient.Post($"/api/games/{gameId}/move", JsonUtility.ToJson(req));
        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonUtility.FromJson<DominoGame>(res);

        Debug.Log($"[Unity] Played [{tile[0]},{tile[1]}].");
    }

    // Example: pass / say "I can't play"
    public async Task PassTurn()
    {
        if (string.IsNullOrEmpty(gameId)) return;

        var req = new MoveRequest
        {
            tile = null,
            end = "right"  // ignored by server on pass
        };

        string res = await apiClient.Post($"/api/games/{gameId}/move", JsonUtility.ToJson(req));
        if (string.IsNullOrEmpty(res)) return;

        currentGame = JsonUtility.FromJson<DominoGame>(res);
        Debug.Log("[Unity] Passed turn.");
    }
}
