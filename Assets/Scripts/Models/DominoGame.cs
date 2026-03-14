using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoGame 
{
    public string _id;
    public string mode;
    public string status;       // waiting, in_progress, finished

    public string ownerUserId;
    public int maxPlayers;

    public int currentTurnIndex;
    public List<DominoPlayer> players;

    public LinkedList<Domino> board;
    public List<int[]> boneyard;

    public int roundNumber;
    public int targetWins;

    public string lastRoundWinnerUserId;
    public string matchWinnerUserId;
    public List<string> sixedPlayerIds;

    public string createdAt;
    public string updatedAt;

    // NEW FIELDS FOR ASYNC PLAY:
    public string lastMoveAt;

    // Move history (optional for replay)
    public List<DominoMove> moves;

    // Temp round fields (backend uses these)
    public string _roundWinnerUserId;
    public string _roundWinnerReason;


    public bool CanPlay(Domino tile)
    {
        if (board.Count == 0)
            return true;

        int leftEnd = board.First.Value.left;
        int rightEnd = board.Last.Value.right;

        return tile.left == leftEnd ||
           tile.right == leftEnd ||
           tile.left == rightEnd ||
           tile.right == rightEnd;
    }

    public void PlaceTile(Domino tile, BoardEnd end)
    {

        if (!CanPlay(tile))
        {
            Debug.Log("Illegal move");
            return;
        }

        if (board.Count == 0)
        {
            board.AddFirst(tile);
            return;
        }

        int leftEnd = board.First.Value.left;
        int rightEnd = board.Last.Value.right;

        if (end == BoardEnd.LEFT)
        {
            if (tile.right == leftEnd)
            {
                board.AddFirst(tile);
            }
            else if (tile.left == leftEnd)
            {
                board.AddFirst(tile.Flipped());
            }
            else
            {
                throw new Exception("Invalid move");
            }
        }
        else if(end == BoardEnd.RIGHT)
        {
            if (tile.left == rightEnd)
            {
                board.AddLast(tile);
            }
            else if (tile.right == rightEnd)
            {
                board.AddLast(tile.Flipped());
            }
            else
            {
                throw new Exception("Invalid move");
            }
        }
    }
}
