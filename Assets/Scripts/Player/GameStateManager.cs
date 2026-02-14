//#define DODEBUG
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    public HashSet<GameObject> pursuersList = new HashSet<GameObject>();

    public enum GameState
    {
        PEACEFUL,           // Player is not being pursued
        PURSUED,            // Player is being actively pursed by an enemy
        PURSUED_BY_APEX,    // Player is being actively chased by an Apex
    }

    private GameState currentState = GameState.PEACEFUL;
    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }

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
    /// Change the game state and handle the pursuers list. If pursuer tries to set state to PEACEFUL, it is
    /// removed from the pursuers list, but the state is only changed to PEACEFUL if there are no pursuers.
    /// </summary>
    /// <param name="state">GameState to change to</param>
    /// <param name="initiator">Which GameObject called to set the state</param>
    /// <returns></returns>
    public bool attemptSetState(GameState state, GameObject initiator)
    {
        // A way to keep track of all the pursuers

        switch (state)
        {
            // Peaceful
            case GameState.PEACEFUL:
                if (pursuersList.Contains(initiator))
                {
                    pursuersList.Remove(initiator);
#if DODEBUG
                    Debug.Log("removed pursuer " + pursuersList.Count + " " + initiator);
#endif

                    if (pursuersList.Count == 0) {
#if DODEBUG
                        Debug.Log("set to peaceful by " + initiator);
#endif
                        currentState = GameState.PEACEFUL;
                    }
                }
                else if (initiator == PlayerID.Instance.gameObject)
                {
#if DODEBUG
                    Debug.Log("set to peaceful by" + initiator);
#endif
                    pursuersList.Clear();
                    currentState = state; // Occurs when player died
                }
                break;

            // Pursued and Pursued by Apex

            case GameState.PURSUED:
            case GameState.PURSUED_BY_APEX:
                // If we already aren't being pursued by this predator
                if (!pursuersList.Contains(initiator))
                {
                    currentState = state;
                    pursuersList.Add(initiator);
#if DODEBUG
                    Debug.Log("added pursuer " + pursuersList.Count);
#endif
                }
                break;
        }

        return true;
    }

}
