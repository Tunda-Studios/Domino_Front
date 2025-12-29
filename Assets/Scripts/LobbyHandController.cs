using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyHandController : MonoBehaviour
{
    [Header("Domino Sprites (28 total)")]
    public Sprite[] dominoSprites;

    [Header("UI")]
    public Transform handContainer;
    public GameObject dominoFacePrefab;

    [Header("Config")]
    public string myUserId = "u1";

    [Header("Skin")]
    public DominoSpriteDatabase defaultSkin;

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

        if (_me.selectedSkin == null)
            _me.selectedSkin = defaultSkin;

        RenderHand(_me.hand, _me.selectedSkin);
    }

    private void RenderHand(List<int[]> hand, DominoSpriteDatabase skin)
    {
        ClearHand();

        foreach (var values in hand)
        {
            int left = values[0];
            int right = values[1];

            GameObject go = Instantiate(dominoFacePrefab, handContainer);
            DominoTileUI ui = go.GetComponent<DominoTileUI>();

            ui.Setup(left, right, skin);
        }
    }

    private void ClearHand()
    {
        foreach (Transform child in handContainer)
            Destroy(child.gameObject);
    }

    private void Start()
    {
        // Local test without server
        var testHand = new List<int[]>
        {
            new[] {2,3},
            new[] {5,5},
            new[] {1,6},
            new[] {0,5},
            new[] {1,4},
            new[] {1,1},
            new[] {2,6}
        };

        RenderHand(testHand, defaultSkin);
    }


}
