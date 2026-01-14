using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Rules")]
public class GameRulesSO : ScriptableObject
{
    public GameType selectedGameType;
    public TimeForTurnType timeForTurn;
}
