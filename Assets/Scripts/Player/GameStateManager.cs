using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    public HashSet<GameObject> pursuersList = new HashSet<GameObject>();

    public enum GameState
    {
        PEACEFUL, // Player is not being pursued
        PURSUED,  // Player is being actively pursed by an enemy
        PURSUED_BY_APEX, // Player is being actively chased by an Apex
    }

    private GameState currentState = GameState.PEACEFUL;

    public GameState getGameState()
    {
        return currentState;
    }

    public bool canSaveGame()
    {
        // If in danger, don't save
        return currentState.Equals(GameState.PEACEFUL);
    }


    /// <summary>
    /// Change the game state and handle the pursuers list
    /// </summary>
    /// <param name="state">GameState to change to</param>
    /// <param name="initiator">Which GameObject called to set the state</param>
    /// <returns></returns>
    public bool setGameState(GameState state, GameObject initiator)
    {
        currentState = state;

        // A way to keep track of all the pursuers

        if (state == GameState.PURSUED || !pursuersList.Contains(initiator))
        {
            pursuersList.Add(initiator);
        }
        else if (state == GameState.PEACEFUL && pursuersList.Contains(initiator)
        {
            pursuersList.Remove(initiator);
        }

        return true;
    }

}
