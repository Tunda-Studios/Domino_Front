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
        game.board = new List<int[]>();

        // Fake Hands
        game.players.Add(new DominoPlayer
        {
            userId = "u1",
            hand = new List<int[]> {
            new []{6,6}, new []{6,5}, new []{1,3}
        }
        });

        game.players.Add(new DominoPlayer
        {
            userId = "u2",
            hand = new List<int[]> {
            new []{4,4}, new []{3,0}
        }
        });

        game.players.Add(new DominoPlayer
        {
            userId = "u3",
            hand = new List<int[]> {
            new []{2,2}, new []{5,1}, new []{6,1}
        }
        });

        game.players.Add(new DominoPlayer
        {
            userId = "u4",
            hand = new List<int[]> {
            new []{3,3}
        }
        });

        table.currentGame = game;
        table.BuildTable();
    }
}
