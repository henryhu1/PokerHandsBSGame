using UnityEngine;

public static class GameRulesFactory
{
    public static GameRulesSO CreateRuntime(GameRulesSO baseRules)
    {
        return Object.Instantiate(baseRules);
    }
}
