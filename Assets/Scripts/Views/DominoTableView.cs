using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoTableView : MonoBehaviour
{
    [Header("Anchors (set in Inspector)")]
    public Transform bottomHandAnchor;
    public Transform rightHandAnchor;
    public Transform topHandAnchor;
    public Transform leftHandAnchor;

    public Transform boardAnchor;

    [Header("Prefabs")]
    public GameObject dominoFacePrefab;  
    public GameObject dominoBackPrefab;

    [Header("Default Skin")]
    public DominoSpriteDatabase defaultSkin;



    public string myUserId = "u1";       
    public DominoGame currentGame;        

    [Header("Layout")]
    public float tileSpacing = 1.2f;

    public void BuildTable()
    {
        if (currentGame == null || currentGame.players == null || currentGame.players.Count == 0)
        {
            Debug.LogError("No game or players set on DominoTableView");
            return;
        }

        var players = currentGame.players;

        //assign default skin for now - will be remove
        foreach (var p in players)
        {
            if (p.selectedSkin == null)
                p.selectedSkin = defaultSkin;
        }

        // 1) Find my index in the server's player list
        int myIndex = players.FindIndex(p => p.userId == myUserId);
        if (myIndex == -1)
        {
            Debug.LogError($"myUserId {myUserId} not found in game players");
            return;
        }

        // 2) Map seats around the table, clockwise from me:
        // seat 0 = bottom (me)
        // seat 1 = right
        // seat 2 = top
        // seat 3 = left
        int playerCount = players.Count;
        int[] seatToPlayerIndex = new int[playerCount];

        for (int seat = 0; seat < playerCount; seat++)
        {
            seatToPlayerIndex[seat] = (myIndex + seat) % playerCount;
        }

        // 3) Render each seat
        RenderSeat(bottomHandAnchor, players[seatToPlayerIndex[0]], isLocal: true);

        if (playerCount > 1)
            RenderSeat(rightHandAnchor, players[seatToPlayerIndex[1]], isLocal: false);
        if (playerCount > 2)
            RenderSeat(topHandAnchor, players[seatToPlayerIndex[2]], isLocal: false);
        if (playerCount > 3)
            RenderSeat(leftHandAnchor, players[seatToPlayerIndex[3]], isLocal: false);
    }
        // Start is called before the first frame update
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Defined render seat
    private void RenderSeat(Transform anchor, DominoPlayer player, bool isLocal)
    {
        if (anchor == null || player == null)
        {
            Debug.LogWarning("RenderSeat called with null anchor or player");
            return;
        }

        // Clear any previous children under this anchor
        for (int i = anchor.childCount - 1; i >= 0; i--)
        {
            Destroy(anchor.GetChild(i).gameObject);
        }

        // Simple horizontal row of tiles
        var hand = player.hand; // e.g. List<DominoTile> or List<int[]>

        for (int i = 0; i < hand.Count; i++)
        {
            GameObject prefabToUse = isLocal ? dominoFacePrefab : dominoBackPrefab;

            GameObject tileObj = Instantiate(prefabToUse, anchor);
            tileObj.name = $"Tile_{player.userId}_{i}";
            tileObj.transform.localPosition = new Vector3(i * tileSpacing, 0f, 0f);
          
            if (isLocal)
            {
               
                int left = hand[i][0];
                
                int right = hand[i][1];
               
                DominoSpriteDatabase skin = player.selectedSkin;
                Debug.Log("reached here" + skin);
                Sprite sprite = skin.GetTileSprite(left, right);
               
                var ui = tileObj.GetComponent<DominoTileUI>();
               
                if (ui != null)
                {
                    ui.Setup(left, right, sprite);
                }
                
            }
           
           
        }

        Debug.Log($"Rendered {hand.Count} tiles for {player.userId} at {anchor.name}, isLocal={isLocal}");

    }

    public void SpawnBoardTile(DominoPlayer owner, int left, int right, Vector3 position)
    {
        GameObject tileObj = Instantiate(dominoFacePrefab, boardAnchor);
        tileObj.transform.localPosition = position;

        DominoSpriteDatabase skin = owner.selectedSkin;
        Sprite sprite = skin.GetTileSprite(left, right);

        DominoTileUI ui = tileObj.GetComponent<DominoTileUI>();
        ui.Setup(left, right, sprite);
    }
}
