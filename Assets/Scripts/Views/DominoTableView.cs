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

    [Header("Glow Hints")]
    public RectTransform leftGlow;
    public RectTransform rightGlow;
    public float glowScaleMultiplier = 1.15f;

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

    [Header("Testing")]
    public bool enableLocalPlayTesting = true;

    // Add these fields to DominoTableView:
    public int boardCenterIndex = -1;
    private GameObject leftEndTile;
    private GameObject rightEndTile;

    private Direction leftEndDirection;
    private Direction rightEndDirection;

    private bool isLeftGlowActive = false;
    private bool isRightGlowActive = false;

    public Direction? currentHoverDirection = null;

    private void Awake()
    {
        RectTransform tileRect = dominoFacePrefab.GetComponent<RectTransform>();
        tileWidth = tileRect.rect.width;
        tileHeight = tileRect.rect.height;
    }
    private void Start()
    {
        HideDropHints();
    }

    public void HideDropHints()
    {
        if (leftGlow != null)
            leftGlow.gameObject.SetActive(false);

        if (rightGlow != null)
            rightGlow.gameObject.SetActive(false);
    }

    public void HideLeftGlow()
    {
        if (leftGlow != null)
            leftGlow.gameObject.SetActive(false);
    }

    public void HideRightGlow()
    {
        if (rightGlow != null)
            rightGlow.gameObject.SetActive(false);
    }

    public void ShowLeftGlow(Vector2 anchoredPosition, float boardScale, Domino tile)
    {
        if (leftGlow == null || currentGame == null || currentGame.board.Count == 0)
            return;

        int leftConnect = currentGame.board.First.Value.left;

        RectTransform leftRT = leftEndTile.GetComponent<RectTransform>();

        Direction previewDir = GetNextDirection(
           leftEndDirection,
           leftRT.anchoredPosition
       );

        Debug.Log($"[LEFT_GLOW] -------------------------");
        Debug.Log($"[LEFT_GLOW] Drag tile = [{tile.left}|{tile.right}]");
        Debug.Log($"[LEFT_GLOW] leftEndDirection = {leftEndDirection}");
        Debug.Log($"[LEFT_GLOW] leftConnect = {leftConnect}");
        Debug.Log($"[LEFT_GLOW] anchoredPosition = {anchoredPosition}");

        GetPreviewTile(
        tile.left,
        tile.right,
        previewDir,
        leftConnect,
        out int previewLeft,
        out int previewRight
        );

        Quaternion rotation = GetTileRotation(previewLeft, previewRight, previewDir);

        Debug.Log($"[LEFT_GLOW] preview tile = [{previewLeft}|{previewRight}]");
        Debug.Log($"[LEFT_GLOW] final rotation z = {rotation.eulerAngles.z}");

        leftGlow.anchoredPosition = anchoredPosition;
        leftGlow.localScale = Vector3.one * boardScale * glowScaleMultiplier;
        leftGlow.localRotation = rotation;
        leftGlow.gameObject.SetActive(true);
    }

    public void ShowRightGlow(Vector2 anchoredPosition, float boardScale, Domino tile)
    {
        if (rightGlow == null || currentGame == null || currentGame.board.Count == 0)
            return;

        // Get board connection value
        int rightConnect = currentGame.board.Last.Value.right;

        Debug.Log($"[RIGHT_GLOW] -------------------------");
        Debug.Log($"[RIGHT_GLOW] Drag tile = [{tile.left}|{tile.right}]");
        Debug.Log($"[RIGHT_GLOW] rightEndDirection = {rightEndDirection}");
        Debug.Log($"[RIGHT_GLOW] rightConnect = {rightConnect}");
        Debug.Log($"[RIGHT_GLOW] anchoredPosition = {anchoredPosition}");

        RectTransform rightRT = rightEndTile.GetComponent<RectTransform>();


        Direction previewDir = GetNextDirection(
             rightEndDirection,
             rightRT.anchoredPosition
         );
        // Compute preview tile (AFTER flip)
        GetPreviewTile(
       tile.left,
       tile.right,
       previewDir,
       rightConnect,
       out int previewLeft,
       out int previewRight
   );

        Quaternion rotation = GetTileRotation(previewLeft, previewRight, previewDir);

        Debug.Log($"[RIGHT_GLOW] preview tile = [{previewLeft}|{previewRight}]");
        Debug.Log($"[RIGHT_GLOW] final rotation z = {rotation.eulerAngles.z}");

        rightGlow.anchoredPosition = anchoredPosition;
        rightGlow.localScale = Vector3.one * boardScale * glowScaleMultiplier;
        rightGlow.localRotation = rotation;
        rightGlow.gameObject.SetActive(true);
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

       

        RenderBoard(currentGame.board);

        LogBoard(currentGame.board);
    }

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
            GameObject tileObj = Instantiate(prefab, anchor, false);

            RectTransform rt = tileObj.GetComponent<RectTransform>();

            //rotate tiless for vertical seats
            if (verticalSeat)
            {
                rt.anchoredPosition = new Vector2(0f, start + i * spacing);
                rt.localRotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
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
                //remove later?
                ui.table = this;
            }
        }
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
    public void RenderBoard(LinkedList<Domino> board)
    {
        var list = new List<Domino>(board);

        if (boardAnchor == null) return;

        for (int i = boardAnchor.childCount - 1; i >= 0; i--)
        {
            Transform child = boardAnchor.GetChild(i);

            if (child.GetComponent<DominoTileUI>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        leftEndTile = null;
        rightEndTile = null;

        if (board.Count == 0) return;

        int center = (boardCenterIndex >= 0 && boardCenterIndex < list.Count)
            ? boardCenterIndex
            : list.Count / 2;

        float scale = 1f;
        float horizontalStep = tileHeight * scale;
        float verticalStep = tileHeight * scale;

        float boardHalfWidth = boardAnchor.rect.width / 2f;
        float edgeMargin = horizontalStep;
        float rightLimit = boardHalfWidth - edgeMargin;
        float leftLimit = -boardHalfWidth + edgeMargin;

        DominoPlayer fakeOwner = new DominoPlayer { selectedSkin = defaultSkin };

        // RIGHT SIDE STATE
        Vector2 pos = Vector2.zero;
        Direction direction = Direction.Right;
        int rightVerticalCount = 0;
        int rightVerticalLength = 2;

        // LEFT SIDE STATE
        Vector2 leftPos = Vector2.zero;
        Direction leftDirection = Direction.Left;
        int leftVerticalCount = 0;

        // Center tile: connect to left neighbor if one exists
        int centerConnectValue = (center > 0) ? list[center - 1].right : -1;

        GameObject centerObj = SpawnBoardTile(
            fakeOwner,
            list[center].left,
            list[center].right,
            Vector2.zero,
            scale,
            Direction.Right,
            centerConnectValue
        );

        leftEndTile = centerObj;
        rightEndTile = centerObj;

        leftEndDirection = Direction.Left;
        rightEndDirection = Direction.Right;

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
                            rightVerticalCount = 0;
                            pos.y -= verticalStep;
                            rightVerticalCount++;
                            break;
                        }
                        pos.x += horizontalStep;
                        break;

                    case Direction.Left:
                        if (pos.x - horizontalStep < leftLimit)
                        {
                            direction = Direction.Down;
                            rightVerticalCount = 0;
                            pos.y -= verticalStep;
                            rightVerticalCount++;
                            break;
                        }
                        pos.x -= horizontalStep;
                        break;

                    case Direction.Down:
                        pos.y -= verticalStep;
                        rightVerticalCount++;
                        break;
                }

                // connectValue = right side of the previous tile in the snapshot
                int rightConnectValue = list[center + step - 1].right;

                GameObject rightObj = SpawnBoardTile(
                    fakeOwner,
                    list[center + step].left,
                    list[center + step].right,
                    pos,
                    scale,
                    direction,
                    rightConnectValue
                );

                rightEndTile = rightObj;
                rightEndDirection = direction;

                if (direction == Direction.Right && pos.x >= rightLimit)
                {
                    direction = Direction.Down;
                    rightVerticalCount = 0;
                }
                else if (direction == Direction.Left && pos.x <= leftLimit)
                {
                    direction = Direction.Down;
                    rightVerticalCount = 0;
                }
                else if (direction == Direction.Down && rightVerticalCount >= rightVerticalLength)
                {
                    rightVerticalCount = 0;
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

                // connectValue = left side of the tile to its right in the snapshot
                int leftConnectValue = list[center - step + 1].left;

                GameObject leftObj = SpawnBoardTile(
                    fakeOwner,
                    list[center - step].left,
                    list[center - step].right,
                    leftPos,
                    scale,
                    leftDirection,
                    leftConnectValue
                );

                leftEndTile = leftObj;
                leftEndDirection = leftDirection;

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
                else if (leftDirection == Direction.Down && leftVerticalCount >= 2)
                {
                    leftVerticalCount = 0;
                    leftDirection = (leftPos.x > 0) ? Direction.Left : Direction.Right;
                }
            }
        }
    }

    Vector2 GetNextPositionOffset(Direction direction, float step)
    {
        switch (direction)
        {
            case Direction.Right:
                return new Vector2(step, 0);
            case Direction.Left:
                return new Vector2(-step, 0);
            case Direction.Down:
                return new Vector2(0, -step);
            default:
                return Vector2.zero;
        }
    }

    public GameObject SpawnBoardTile(
        DominoPlayer owner,
        int left,
        int right,
        Vector2 position,
        float scale,
        Direction direction,
        int connectValue = -1)
    {
        GameObject tileObj = Instantiate(dominoFacePrefab, boardAnchor, false);
        tileObj.tag = "BoardTile";
        RectTransform rt = tileObj.GetComponent<RectTransform>();

        rt.anchoredPosition = position;
        rt.localScale = Vector3.one * scale;

        bool isDouble = left == right;
        int originalLeft = left;
        int originalRight = right;

        Debug.Log($"[RENDER] -------------------------");
        Debug.Log($"[RENDER] Input tile = [{originalLeft}|{originalRight}]");
        Debug.Log($"[RENDER] Direction = {direction}");
        Debug.Log($"[RENDER] Position = {position}");
        Debug.Log($"[RENDER] ConnectValue = {connectValue}");

        rt.localRotation = GetTileRotation(left, right, direction);

        // Simple call — no flip params needed
        DominoTileUI ui = tileObj.GetComponent<DominoTileUI>();
        ui.Setup(left, right, owner.selectedSkin);
        rt.localScale = Vector3.one * scale;
        return tileObj;
    }


    public void LogBoard(LinkedList<Domino> board)
    {
        if (board == null || board.Count == 0)
        {
            Debug.Log("BOARD: (empty)");
            return;
        }

        string s = "BOARD: ";
        foreach (var t in board)
        {
            s += $"[{t.left}|{t.right}] ";
        }

        Debug.Log(s);
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
        if (currentGame == null)
        {
            currentGame = new DominoGame();
            currentGame.board = new LinkedList<Domino>();
        }

        currentGame.board.Clear();

        currentGame.board.AddLast(new Domino { left = 5, right = 6 });
        currentGame.board.AddLast(new Domino { left = 6, right = 3 });
        currentGame.board.AddLast(new Domino { left = 3, right = 5 });

        Debug.Log("Initialized test board");

        RenderBoard(currentGame.board);
    }

    public void HandleTileDragging(Vector2 screenPosition, Domino tile)
    {
        Debug.Log($"[DRAG] -------------------------");
        Debug.Log($"[DRAG] Tile = [{tile.left}|{tile.right}]");

        if (currentGame == null || currentGame.board == null || currentGame.board.Count == 0)
        {
            Debug.Log("[DRAG] Board invalid");
            HideDropHints();
            currentHoverDirection = null;
            return;
        }

        int leftEnd = currentGame.board.First.Value.left;
        int rightEnd = currentGame.board.Last.Value.right;

        bool matchLeft = tile.left == leftEnd || tile.right == leftEnd;
        bool matchRight = tile.left == rightEnd || tile.right == rightEnd;

        Debug.Log($"[DRAG] matchLeft={matchLeft}, matchRight={matchRight}");

        Vector2 leftPos = Vector2.zero;
        Vector2 rightPos = Vector2.zero;

        if (leftEndTile != null)
        {
            RectTransform leftRT = leftEndTile.GetComponent<RectTransform>();

            Direction previewDir = GetNextDirection(
                leftEndDirection,
                leftRT.anchoredPosition
            );

            Vector2 offset = GetDirectionalOffset(previewDir);

            if (previewDir == Direction.Left)
            {
                leftPos = leftRT.anchoredPosition + offset; 
            }
            else
            {
                leftPos = leftRT.anchoredPosition - offset;
            }


            Debug.Log($"[POS FIX LEFT] currentDir={leftEndDirection} → previewDir={previewDir}");
            Debug.Log($"[POS FIX LEFT] basePos={leftRT.anchoredPosition} → glowPos={leftPos}");
        }

        if (rightEndTile != null)
        {
            RectTransform rightRT = rightEndTile.GetComponent<RectTransform>();

            Direction previewDir = GetNextDirection(
                rightEndDirection,
                rightRT.anchoredPosition
            );

            rightPos = rightRT.anchoredPosition + GetDirectionalOffset(previewDir);

            Debug.Log($"[POS FIX RIGHT] currentDir={rightEndDirection} → previewDir={previewDir}");
            Debug.Log($"[POS FIX RIGHT] basePos={rightRT.anchoredPosition} → glowPos={rightPos}");
        }

        // Convert pointer to LOCAL space (CRITICAL)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardAnchor,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        Debug.Log($"[DRAG] localPoint={localPoint}");
        Debug.Log($"[DRAG] leftPos={leftPos}, rightPos={rightPos}");

        // RESET STATE
        HideLeftGlow();
        HideRightGlow();
        isLeftGlowActive = false;
        isRightGlowActive = false;
        currentHoverDirection = null;

        // Compute distance to BOTH ends
        float distLeft = Vector2.Distance(localPoint, leftPos);
        float distRight = Vector2.Distance(localPoint, rightPos);

        Debug.Log($"[DRAG] distLeft={distLeft}, distRight={distRight}");

        // Decide based on closest VALID side
        if (matchLeft && matchRight)
        {
            // Show BOTH glows — let proximity decide on drop
            Debug.Log("[DRAG] SHOW BOTH (double match)");
            ShowLeftGlow(leftPos, 1f, tile);
            ShowRightGlow(rightPos, 1f, tile);
            isLeftGlowActive = true;
            isRightGlowActive = true;

            // Set hover direction to whichever side is closer RIGHT NOW
            currentHoverDirection = (distLeft < distRight) ? Direction.Left : Direction.Right;
        }
        else if (matchLeft)
        {
            Debug.Log("[DRAG] SHOW LEFT");
            ShowLeftGlow(leftPos, 1f,tile);
            isLeftGlowActive = true;
            currentHoverDirection = Direction.Left;
        }
        else if (matchRight)
        {
            Debug.Log("[DRAG] SHOW RIGHT");
            ShowRightGlow(rightPos, 1f,tile);
            isRightGlowActive = true;
            currentHoverDirection = Direction.Right;
        }
        else
        {
            Debug.Log("[DRAG] No valid side");
            currentHoverDirection = null;
        }
    }

    public Direction GetClosestGlowDirection(Vector2 screenPosition)
    {
        Debug.Log($"[GLOW_DIR] -------------------------");
        Debug.Log($"[GLOW_DIR] screenPosition = {screenPosition}");
        Debug.Log($"[GLOW_DIR] leftGlow = {(leftGlow == null ? "NULL" : leftGlow.name)}");
        Debug.Log($"[GLOW_DIR] rightGlow = {(rightGlow == null ? "NULL" : rightGlow.name)}");

        bool leftActive = leftGlow != null && leftGlow.gameObject.activeSelf;
        bool rightActive = rightGlow != null && rightGlow.gameObject.activeSelf;

        Debug.Log($"[GLOW_DIR] leftActive = {leftActive}");
        Debug.Log($"[GLOW_DIR] rightActive = {rightActive}");

        if (leftActive && !rightActive)
        {
            Debug.Log("[GLOW_DIR] Only left active -> LEFT");
            return Direction.Left;
        }

        if (rightActive && !leftActive)
        {
            Debug.Log("[GLOW_DIR] Only right active -> RIGHT");
            return Direction.Right;
        }

        if (!leftActive && !rightActive)
        {
            Debug.Log("[GLOW_DIR] BOTH inactive -> INVALID");
            return Direction.Invalid;
        }

        // Both active — find closest
        Canvas canvas = boardAnchor.GetComponentInParent<Canvas>();
        Camera cam = null;

        if (canvas != null)
        {
            Debug.Log($"[GLOW_DIR] Canvas renderMode = {canvas.renderMode}");
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;
        }
        else
        {
            Debug.Log("[GLOW_DIR] Canvas is NULL");
        }

        bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardAnchor,
            screenPosition,
            cam,
            out Vector2 localPoint
        );

        Debug.Log($"[GLOW_DIR] converted = {converted}");
        Debug.Log($"[GLOW_DIR] localPoint = {localPoint}");
        Debug.Log($"[GLOW_DIR] leftGlow.anchoredPosition = {leftGlow.anchoredPosition}");
        Debug.Log($"[GLOW_DIR] rightGlow.anchoredPosition = {rightGlow.anchoredPosition}");

        if (!converted)
        {
            Debug.Log("[GLOW_DIR] Conversion failed -> INVALID");
            return Direction.Invalid;
        }

        float distLeft = Vector2.Distance(localPoint, leftGlow.anchoredPosition);
        float distRight = Vector2.Distance(localPoint, rightGlow.anchoredPosition);

        Debug.Log($"[GLOW_DIR] distLeft = {distLeft}, distRight = {distRight}");

        Direction result = distLeft < distRight ? Direction.Left : Direction.Right;
        Debug.Log($"[GLOW_DIR] Result = {result}");
        return result;
    }   

    public void ClearGlow()
    {
      //  leftGlow.SetActive(false);
       // rightGlow.SetActive(false);
    }

    void PrintBoard()
    {
        string s = "BOARD: ";
        foreach (var t in testBoard)
            s += $"[{t[0]}|{t[1]}] ";
        Debug.Log(s);
    }


    /*
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
    */

    public Vector3 GetLeftEndWorldPosition()
    {
        if (leftEndTile != null)
            return leftEndTile.GetComponent<RectTransform>().position;

        return boardAnchor.position;
    }

    public Vector3 GetRightEndWorldPosition()
    {
        if (rightEndTile != null)
            return rightEndTile.GetComponent<RectTransform>().position;

        return boardAnchor.position;
    }

    public Vector2 GetDirectionalOffset(Direction dir)
    {
        switch (dir)
        {
            case Direction.Right:
                    return new Vector2(tileHeight, 0);
            case Direction.Left:
                return new Vector2(-tileHeight, 0);
            case Direction.Down:
                return new Vector2(0, -tileHeight);

            default:
                return Vector2.zero;
        }
    }

    Quaternion GetTileRotation(int left, int right, Direction direction)
    {
        bool isDouble = left == right;
        Quaternion result = Quaternion.identity;

        if (isDouble)
        {
            result = Quaternion.Euler(0, 0, 0);
            Debug.Log($"[ROTATION] Tile [{left}|{right}] is double -> {result.eulerAngles}");
            return result;
        }

        if (direction == Direction.Right)
        {
            bool highOnRight = (left > right);
            result = Quaternion.Euler(0, 0, highOnRight ? -90 : 90);
        }
        else if (direction == Direction.Left)
        {
            bool highOnLeft = (right > left);
            return Quaternion.Euler(0, 0, highOnLeft ? 90 : -90);
        }
        else if (direction == Direction.Down)
        {
            bool highOnTop = (left > right);
            result = Quaternion.Euler(0, 0, highOnTop ? 180 : 0);
        }

        Debug.Log($"[ROTATION] Tile [{left}|{right}] direction={direction} -> z={result.eulerAngles.z}");
        return result;
    }

    void GetPreviewTile(
    int left,
    int right,
    Direction direction,
    int connectValue,
    out int finalLeft,
    out int finalRight
)
    {
        finalLeft = left;
        finalRight = right;

        if (connectValue != -1)
        {
            if (direction == Direction.Right && finalLeft != connectValue)
            {
                (finalLeft, finalRight) = (finalRight, finalLeft);
            }
            else if (direction == Direction.Left && finalRight != connectValue)
            {
                (finalLeft, finalRight) = (finalRight, finalLeft);
            }
        }
    }

    Direction GetNextDirection(Direction currentDir, Vector2 currentPos)
    {
        float boardHalfWidth = boardAnchor.rect.width / 2f;
        float step = tileHeight;

        if (currentDir == Direction.Right && currentPos.x + step > boardHalfWidth - step)
        {
            Debug.Log("[DIR FIX] Right → Down");
            return Direction.Down;
        }

        if (currentDir == Direction.Left && currentPos.x <= -boardHalfWidth + step)
        {
            Debug.Log("[DIR FIX] Left → Down");
            return Direction.Down;
        }

        return currentDir;
    }
}
