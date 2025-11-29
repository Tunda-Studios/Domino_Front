using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    public ApiClient apiClient;
    public DominoGameController gameController;

    public async Task LoadMyGames()
    {
        string res = await apiClient.Get("/api/games");
        if (string.IsNullOrEmpty(res)) return;

        // If backend returns a bare array, you can use a wrapper or custom parsing.
        // For now we'll assume it returns `[ ... ]` and parse manually:
        // Note: Unity JsonUtility can't handle top-level arrays, so usually you'd wrap it.
        Debug.Log($"[Lobby] Raw games JSON: {res}");
        // You can swap to a JSON lib that supports arrays (like Newtonsoft in .NET, or use a wrapper in Node).
    }

    public async Task OpenGame(string selectedGameId)
    {
        gameController.gameId = selectedGameId;
        await gameController.RefreshGame();
        gameController.StartPolling();
    }
}
