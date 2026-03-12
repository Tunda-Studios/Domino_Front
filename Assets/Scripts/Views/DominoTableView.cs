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

    [Header("Board Scaling")]
    public int shrinkStart = 5;
    public float minBoardScale = 0.6f;

    [Header("Board Layout")]
    public float rowSpacing = 1.2f; // vertical distance between rows

    [Header("Tiles turning")]
    int horizontalLength = 6;  // tiles before turning
    int horizontalCount = 0;

    int verticalLength = 2;
    int verticalCount = 0;

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
        float spacing = verticalSeat ? tileHeight * 0.8f : tileWidth * 1.1f;

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

    public void SpawnBoardTile(
    DominoPlayer owner,
    int left,
    int right,
    Vector2 position,
    float scale,
    Direction direction)
    {
        GameObject tileObj = Instantiate(dominoFacePrefab, boardAnchor, false);
        RectTransform rt = tileObj.GetComponent<RectTransform>();

        rt.anchoredPosition = position;
        rt.localScale = Vector3.one * scale;

        bool isDouble = left == right;

        if (direction == Direction.Right || direction == Direction.Left)
        {
            // Horizontal chain
            rt.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (direction == Direction.Down)
        {
            // Vertical chain
            rt.localRotation = Quaternion.Euler(0, 0, 0);
        }

        if (isDouble)
        {
            rt.localRotation = Quaternion.Euler(0, 0, 0);
        }

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

        SpawnBoardTile(fakeOwner, left, right, pos,1f, Direction.Right);

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
        if (boardAnchor == null) return;

        for (int i = boardAnchor.childCount - 1; i >= 0; i--)
            Destroy(boardAnchor.GetChild(i).gameObject);

        if (board.Count == 0) return;

        float scale = 1f;

        float horizontalStep = tileHeight * scale;
        float verticalStep = tileHeight * scale;

        float boardHalfWidth = boardAnchor.rect.width / 2f;

        float edgeMargin = horizontalStep;
        float rightLimit = boardHalfWidth - edgeMargin;
        float leftLimit = -boardHalfWidth + edgeMargin;

        DominoPlayer fakeOwner = new DominoPlayer
        {
            selectedSkin = defaultSkin
        };

        int center = board.Count / 2;

        // RIGHT SIDE STATE
        Vector2 pos = Vector2.zero;
        Direction direction = Direction.Right;
        int verticalCount = 0;
        int verticalLength = 2;

        // LEFT SIDE STATE
        Vector2 leftPos = Vector2.zero;
        Direction leftDirection = Direction.Left;
        int leftVerticalCount = 0;

        // center tile
        SpawnBoardTile(
            fakeOwner,
            board[center][0],
            board[center][1],
            Vector2.zero,
            scale,
            Direction.Right
        );

        // render outward
        for (int step = 1; step <= center || center + step < board.Count; step++)
        {

            // RIGHT SIDE 
            if (center + step < board.Count)
            {
                switch (direction)
                {
                    case Direction.Right:
                        if (pos.x + horizontalStep > rightLimit)
                        {
                            direction = Direction.Down;
                            verticalCount = 0;
                            pos.y -= verticalStep;
                            verticalCount++;
                            break;
                        }

                        pos.x += horizontalStep;
                        break;

                    case Direction.Left:
                        if (pos.x - horizontalStep < leftLimit)
                        {
                            direction = Direction.Down;
                            verticalCount = 0;
                            pos.y -= verticalStep;
                            verticalCount++;
                            break;
                        }

                        pos.x -= horizontalStep;
                        break;

                    case Direction.Down:
                        pos.y -= verticalStep;
                        verticalCount++;
                        break;
                }

                SpawnBoardTile(
                    fakeOwner,
                    board[center + step][0],
                    board[center + step][1],
                    pos,
                    scale,
                    direction
                );

                // TURN LOGIC
                if (direction == Direction.Right && pos.x >= rightLimit)
                {
                    direction = Direction.Down;
                    verticalCount = 0;
                }
                else if (direction == Direction.Left && pos.x <= leftLimit)
                {
                    direction = Direction.Down;
                    verticalCount = 0;
                }
                else if (direction == Direction.Down && verticalCount >= verticalLength)
                {
                    verticalCount = 0;
                    direction = (pos.x > 0) ? Direction.Left : Direction.Right;
                }
            }

            // LEFT SIDE
            if (center - step >= 0)
            {
                switch (leftDirection)
                {
                    case Direction.Left:
                        leftPos.x -= horizontalStep;
                        break;

                    case Direction.Right:
                        leftPos.x += horizontalStep;
                        break;

                    case Direction.Down:
                        leftPos.y += verticalStep;
                        leftVerticalCount++;
                        break;
                }

                SpawnBoardTile(
                    fakeOwner,
                    board[center - step][0],
                    board[center - step][1],
                    leftPos,
                    scale,
                    leftDirection
                );

                // TURN LOGIC (mirror of right side)
                if (leftDirection == Direction.Left && leftPos.x <= leftLimit)
                {
                    leftDirection = Direction.Down;
                    leftVerticalCount = 0;
                }
                else if (leftDirection == Direction.Right && leftPos.x >= rightLimit)
                {
                    leftDirection = Direction.Down;
                    leftVerticalCount = 0;
                }
                else if (leftDirection == Direction.Down && leftVerticalCount >= verticalLength)
                {
                    leftVerticalCount = 0;
                    leftDirection = (leftPos.x > 0) ? Direction.Left : Direction.Right;
                }
            }
        }
    }

    public void TestPlaceLeft()
    {
        int[] tile = new int[] { 5, 2 };

        int leftValue = testBoard[0][0];

        if (tile[0] != leftValue && tile[1] != leftValue)
            return;

        // Ensure RIGHT side matches board
        if (tile[0] == leftValue)
            tile = new int[] { tile[1], tile[0] };

        testBoard.Insert(0, tile);

        PrintBoard();
        RenderBoard(testBoard);
    }

    public void TestPlaceRight()
    {
        int[] tile = new int[] { 5, 2 };

        int rightValue = testBoard[testBoard.Count - 1][1];

        if (tile[0] != rightValue && tile[1] != rightValue)
        {
            //Debug.Log(" Cannot play [5|2] on RIGHT");
            return;
        }

        // Flip if needed
        if (tile[0] != rightValue)
            tile = new int[] { tile[1], tile[0] };

        testBoard.Add(tile);

        //Debug.Log(" Played [5|2] on RIGHT");
        PrintBoard();
        RenderBoard(testBoard);
    }


}
