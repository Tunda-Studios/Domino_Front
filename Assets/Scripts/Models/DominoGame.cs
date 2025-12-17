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

    public List<int[]> board;
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
}
