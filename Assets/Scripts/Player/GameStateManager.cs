using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        NULL,
        PEACEFUL, // Player is not being pursued
        PURSUED,  // Player is being actively pursed by an enemy
        PURSUED_BY_APEX, // Player is being actively chased by an Apex
    }

    private GameState currentState = GameState.NULL;

    public GameState getGameState()
    {
        return currentState;
    }

    public bool setGameState(GameState state)
    {
        currentState = state;
        return true;
    }

}
