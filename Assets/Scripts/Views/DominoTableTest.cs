using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoTableTest : MonoBehaviour
{
    public DominoTableView table;

    void Start()
    {
        DominoGame game = new DominoGame();
        game.players = new List<DominoPlayer>();
        game.board = new LinkedList<Domino>();

        // Fake Hands
        // Player 1
        var p1 = new DominoPlayer
        {
            userId = "u1",
            hand = new PlayerHand()
        };
        p1.hand.AddTile(new Domino(6, 6));
        p1.hand.AddTile(new Domino(6, 5));
        p1.hand.AddTile(new Domino(1, 3));

        // Player 2
        var p2 = new DominoPlayer
        {
            userId = "u2",
            hand = new PlayerHand()
        };
        p2.hand.AddTile(new Domino(4, 4));
        p2.hand.AddTile(new Domino(3, 0));

        // Player 3
        var p3 = new DominoPlayer
        {
            userId = "u3",
            hand = new PlayerHand()
        };
        p3.hand.AddTile(new Domino(2, 2));
        p3.hand.AddTile(new Domino(5, 1));
        p3.hand.AddTile(new Domino(6, 1));

        // Player 4
        var p4 = new DominoPlayer
        {
            userId = "u4",
            hand = new PlayerHand()
        };
        p4.hand.AddTile(new Domino(3, 3));

        game.players.Add(p1);
        game.players.Add(p2);
        game.players.Add(p3);
        game.players.Add(p4);

        table.currentGame = game;
        table.BuildTable();
    }
}
