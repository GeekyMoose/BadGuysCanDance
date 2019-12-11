﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    public List<SnapGridAIController> listAIPlayers;
    public GameEvent gameOverEvent;

    private SnapGridCharacter player;
    private int countCharacters = 8; // Hard coded to 8 characters


    private void Start()
    {
        Assert.IsTrue(this.listAIPlayers.Count == countCharacters, "Missing asset (8 characters expected)");

        // Select a random player
        float randValue = Random.Range(0, 10000);
        int indice = (int)(randValue % countCharacters);
        this.player = this.listAIPlayers[indice].GetComponent<SnapGridCharacter>();
        this.player.UsePlayerControls();
        this.listAIPlayers.RemoveAt(indice);
    }

    public void MoveAllAIs()
    {
        foreach(SnapGridAIController currentAI in this.listAIPlayers)
        {
            currentAI.Move();
        }
    }

    public void ChangeAllAIsDirections()
    {
        foreach (SnapGridAIController currentAI in this.listAIPlayers)
        {
            currentAI.ChangeRandomMovementNewDirection();
        }
    }

    public void OnPlayerKilledEvent()
    {
        // There is only one player, so on kill, game is over
        Debug.Log("OnPlayerKilledEvent received");
        countCharacters--;
        this.gameOverEvent.Raise();
    }

    public void OnAIKilledEvent()
    {
        countCharacters--;
        Debug.Log("OnPlayerKilledEvent received");
    }
}
