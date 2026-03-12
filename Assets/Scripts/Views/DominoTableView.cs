using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoTableView : MonoBehaviour
{
    [Header("Anchors (set in Inspector)")]
    public RectTransform bottomHandAnchor;
    public RectTransform rightHandAnchor;
    public RectTransform topHandAnchor;
    public RectTransform leftHandAnchor;

    public RectTransform boardAnchor;

    [Header("Prefabs")]
    public GameObject dominoFacePrefab;  
    public GameObject dominoBackPrefab;

    [Header("Default Skin")]
    public DominoSpriteDatabase defaultSkin;

    [Header("UNITY TEST BOARD")]
    public List<int[]> testBoard = new List<int[]>();

    float tileWidth;
    float tileHeight;

    public string myUserId = "u1";       
    public DominoGame currentGame;        

    [Header("Layout")]
    public float tileSpacing = 120f;

    private void Awake()
    {
        RectTransform tileRect = dominoFacePrefab.GetComponent<RectTransform>();
        tileWidth = tileRect.rect.width;
        tileHeight = tileRect.rect.height;
    }
    //build table UI by rendering all hands
    public void BuildTable()
    {
        //ensure the game state exist before attempting to render
        if (currentGame == null ||
            currentGame.players == null ||
            currentGame.players.Count == 0)
        {
            Debug.LogWarning(
                "DominoTableView.BuildTable skipped: game not ready (players missing)"
            );
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

        //stop if local player is not found
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

        //render the local player's hand
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
        InitTestBoard();
    }

    // Update is called once per frame
    void Update()
    {
        // Press
        // to spawn a tile on the board
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestSpawnBoardTile();
        }
    }

    //render the domino tiles for a specific player's hand
    private void RenderSeat(RectTransform anchor, DominoPlayer player, bool isLocal)
    {
        //validate inputs
        if (anchor == null || player == null)
        {
            Debug.LogWarning("RenderSeat called with null anchor or player");
            return;
        }

        // Clear old tiles
        for (int i = anchor.childCount - 1; i >= 0; i--)
            Destroy(anchor.GetChild(i).gameObject);

        var hand = player.hand;

        bool verticalSeat = anchor == leftHandAnchor || anchor == rightHandAnchor;
        float spacing = verticalSeat ? tileHeight * 1.1f : tileWidth * 1.1f;

        float totalSize = (hand.Count - 1) * spacing;
        float start = -(totalSize / 2f);

        // Center the row

        //spawn each tile in the player's hand
        for (int i = 0; i < hand.Count; i++)
        {
            //local players sees tile faces, oppenents see tile backs
            GameObject prefab = isLocal ? dominoFacePrefab : dominoBackPrefab;
            GameObject tileObj = Instantiate(prefab, anchor,false);

            RectTransform rt = tileObj.GetComponent<RectTransform>();

            //rotate tiless for vertical seats
            if (verticalSeat)
            {
                rt.anchoredPosition = new Vector2(0f, start + i * spacing);
                rt.localRotation = Quaternion.Euler(0, 0, 90);          
            }
            else {
                rt.anchoredPosition = new Vector2(start + i * spacing, 0f);
            }

            tileObj.name = $"Tile_{player.userId}_{i}";

            //setup
            if (isLocal)
            {
                int left = hand[i][0];
                int right = hand[i][1];

                DominoSpriteDatabase skin = player.selectedSkin;
                Sprite sprite = skin.GetTileSprite(left, right);

                DominoTileUI ui = tileObj.GetComponent<DominoTileUI>();
                if (ui != null)
                    ui.Setup(left, right, player.selectedSkin);
            }
        }
    }

    public void SpawnBoardTile(DominoPlayer owner, int left, int right, Vector2 position)
    {
        GameObject tileObj = Instantiate(dominoFacePrefab, boardAnchor, false);
        RectTransform rt = tileObj.GetComponent<RectTransform>();

        if (rt == null)
        {
            rt = tileObj.AddComponent<RectTransform>();
        }
        rt.anchoredPosition = position;

        rt.localRotation = Quaternion.Euler(0, 0, 90);

        DominoSpriteDatabase skin = owner.selectedSkin;
        Sprite sprite = skin.GetTileSprite(left, right);

        DominoTileUI ui = tileObj.GetComponent<DominoTileUI>();
        ui.Setup(left, right, owner.selectedSkin);
    }

    public void TestSpawnBoardTile()
    {
        // Example test values
        int left = 6;
        int right = 4;

        // Example position inside the board
        Vector2 pos = new Vector2(0, 0); // center of board

        // Fake owner: local player's skin
        DominoPlayer fakeOwner = new DominoPlayer
        {
            selectedSkin = defaultSkin
        };

        SpawnBoardTile(fakeOwner, left, right, pos);

        Debug.Log($"Spawned test board tile [{left}|{right}] at {pos}");
    }

    public void InitTestBoard()
    {
        testBoard.Clear();

        // Example starting board
        testBoard.Add(new int[] { 5, 6 });
        testBoard.Add(new int[] { 6, 3 });
        testBoard.Add(new int[] { 3, 5 });

        Debug.Log("Initialized test board");
        PrintBoard();

        RenderBoard(testBoard);
    }

    void PrintBoard()
    {
        string s = "BOARD: ";
        foreach (var t in testBoard)
            s += $"[{t[0]}|{t[1]}] ";
        Debug.Log(s);
    }

    public void RenderBoard(List<int[]> board)
    {
        if (boardAnchor == null)
        {
            Debug.LogError("BoardAnchor is not set!");
            return;
        }

        // Clear existing board tiles
        for (int i = boardAnchor.childCount - 1; i >= 0; i--)
        {
            Destroy(boardAnchor.GetChild(i).gameObject);
        }

        if (testBoard == null || testBoard.Count == 0)
        {
            Debug.Log("RenderBoard: testBoard empty");
            return;
        }

        // Center the board horizontally
        float spacing = tileHeight;
        int tileCount = board.Count;
        float startX = -(spacing * (tileCount - 1)) / 2f;

        // Fake owner just to get a skin
        DominoPlayer fakeOwner = new DominoPlayer
        {
            selectedSkin = defaultSkin
        };

        for (int i = 0; i < testBoard.Count; i++)
        {
            int left = testBoard[i][0];
            int right = testBoard[i][1];

            Vector2 pos = new Vector2(startX + i * spacing, 0f);
            Debug.Log($"Tile {i} position: {pos}");
            SpawnBoardTile(fakeOwner, left, right, pos);

        }

    }

    public void TestPlaceLeft()
    {
        int[] tile = new int[] { 5, 2 };

        int leftValue = testBoard[0][0];

        if (tile[0] != leftValue && tile[1] != leftValue)
        {
            Debug.Log(" Cannot play [5|2] on LEFT");
            return;
        }

        // Flip if needed
        if (tile[1] == leftValue)
            tile = new int[] { tile[1], tile[0] };

        testBoard.Insert(0, tile);

        Debug.Log(" Played [5|2] on LEFT");
        PrintBoard();
        RenderBoard(testBoard);
    }

    public void TestPlaceRight()
    {
        int[] tile = new int[] { 5, 2 };

        int rightValue = testBoard[testBoard.Count - 1][1];

        if (tile[0] != rightValue && tile[1] != rightValue)
        {
            Debug.Log(" Cannot play [5|2] on RIGHT");
            return;
        }

        // Flip if needed
        if (tile[0] != rightValue)
            tile = new int[] { tile[1], tile[0] };

        testBoard.Add(tile);

        Debug.Log(" Played [5|2] on RIGHT");
        PrintBoard();
        RenderBoard(testBoard);
    }


}
