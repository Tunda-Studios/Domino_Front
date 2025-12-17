using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InboxItemUI : MonoBehaviour
{
    public TextMeshProUGUI gameIdText;
    public TextMeshProUGUI opponentsText;
    public TextMeshProUGUI turnStatusText;

    private GameInboxItem linkedGame;
    private GameInboxManager inboxManager;

    public void Setup(GameInboxItem game, GameInboxManager manager)
    {
        linkedGame = game;
        inboxManager = manager;

        gameIdText.text = $"Game: {game._id.Substring(0, 6)}...";

        if (game.opponentNames != null && game.opponentNames.Length > 0)
            opponentsText.text = "Vs: " + string.Join(", ", game.opponentNames);
        else
            opponentsText.text = "Vs: ???";

        turnStatusText.text = game.myTurn ? "YOUR TURN" : "Waiting";

        // Color coding  
        turnStatusText.color = game.myTurn ? Color.green : Color.white;
    }

    // Called when the player's taps the card
    public void OnClick()
    {
        inboxManager.OpenGame(linkedGame);
    }
}
