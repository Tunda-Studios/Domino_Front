using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyHandController : MonoBehaviour
{
    [Header("Domino Sprites (28 total)")]
    public Sprite[] dominoSprites;

    [Header("UI")]
    public Transform handContainer;
    public GameObject dominoPrefab;

    [Header("Config")]
    public string myUserId = "u1";

    private DominoGame _game;
    private DominoPlayer _me;

    // Call this once you've loaded _game from API
    public void ShowLobbyHand(DominoGame game)
    {
        _game = game;
        _me = _game.players.Find(p => p.userId == myUserId);

        if (_me == null)
        {
            Debug.LogError("Couldn't find my player in game.");
            return;
        }

        // Clear current hand
        foreach (Transform child in handContainer)
            Destroy(child.gameObject);

        // Spawn one prefab per domino in my hand
        foreach (var values in _me.hand)
        {
            int left = values[0];
            int right = values[1];

            var go = Instantiate(dominoPrefab, handContainer);
            var ui = go.GetComponent<DominoTileUI>();

            Sprite sprite = GetDominoSprite(left, right);
            ui.Setup(left, right, sprite);
        }
    }


    // Map (left,right) → sprite index 0..27
    private Sprite GetDominoSprite(int left, int right)
    {
        int a = Mathf.Min(left, right);
        int b = Mathf.Max(left, right);

        // Index formula assuming order:
        // 0: 0-0
        // 1: 0-1
        // 2: 0-2
        // ...
        // 6: 0-6
        // 7: 1-1
        // 8: 1-2
        // ...
        // 27: 6-6
        int index = a * 7 - (a * (a - 1)) / 2 + (b - a);

        if (index < 0 || index >= dominoSprites.Length)
        {
            Debug.LogError($"Domino sprite index out of range for {a}|{b}: {index}");
            return null;
        }

        return dominoSprites[index];
    }
    // Start is called before the first frame update
    void Start()
    {
        var hand = new List<int[]>
        {
            new[] {2,3},
            new[] {5,5},
            new[] {1,6},
            new[] {0,5},
            new[] {1,4},
            new[] {1,1},
            new[] {2,6}
        };

        DisplayHand(hand);
    }

    void DisplayHand(List<int[]> hand)
    {
        foreach (Transform child in handContainer)
            Destroy(child.gameObject);

        foreach (var tile in hand)
        {
            int left = tile[0];
            int right = tile[1];

            Sprite sprite = GetDominoSprite(left, right);

            GameObject go = Instantiate(dominoPrefab, handContainer);
            var ui = go.GetComponent<DominoTileUI>();
            ui.Setup(left, right, sprite);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
