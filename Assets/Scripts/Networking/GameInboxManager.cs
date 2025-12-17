using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameInboxManager : MonoBehaviour
{
    public string apiBaseUrl = "http://localhost:3000/api";
    public string userId = "u1"; // plug your real one

    public GameObject inboxItemPrefab;
    public Transform inboxListParent;

    void OnEnable()
    {
        StartCoroutine(LoadInbox());
    }

    IEnumerator LoadInbox()
    {
        string url = $"{apiBaseUrl}/games-inbox";

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("x-user-id", userId);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Inbox error: " + req.error);
            yield break;
        }

        var json = req.downloadHandler.text;
        var resp = JsonUtility.FromJson<GameInboxResponse>(json);

        // TODO: clear old list, then:

        foreach (var g in resp.games)
        {
            var go = Instantiate(inboxItemPrefab, inboxListParent);
            var ui = go.GetComponent<InboxItemUI>();
            ui.Setup(g, this);
        }
    }

    // Called by InboxItemUI when user clicks a row
    public void OpenGame(GameInboxItem game)
    {
        // store selected game id somewhere (GameSession, static, etc.)
        SelectedGame.Id = game._id;
        // load scene with actual board
        UnityEngine.SceneManagement.SceneManager.LoadScene("DominoGameScene");
    }
}

public static class SelectedGame
{
    public static string Id;
}
