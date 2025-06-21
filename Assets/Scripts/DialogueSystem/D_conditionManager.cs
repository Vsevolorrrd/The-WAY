using Subtegral.DialogueSystem.DataContainers;
using System.Collections.Generic;
using UnityEngine;

public class D_conditionManager : MonoBehaviour
{
    private List<string> stringConditions = new List<string>(24);
    private Dictionary<string, bool> boolConditions = new Dictionary<string, bool>(10);

    // strings
    public bool StringCondition(string condition)
    {
        return stringConditions.Contains(condition);
    }
    public void AddStringCondition(string condition)
    {
        if (!stringConditions.Contains(condition))
        stringConditions.Add(condition);
    }

    // bools
    public bool BoolCondition(string key, bool expected)
    {
        return boolConditions.TryGetValue(key, out var value) && value == expected;
    }

    public void SetBoolCondition(string key, bool value)
    {
        boolConditions[key] = value;
    }

    // ints
    public bool IntCondition(string key, ComparisonType comparison, int target)
    {
        if (!VariablesManager.Instance.TryGetValue(key, out int value)) return false;

        switch (comparison)
        {
            case ComparisonType.Equals: return value == target;
            case ComparisonType.NotEquals: return value != target;
            case ComparisonType.GreaterThan: return value > target;
            case ComparisonType.LessThan: return value < target;
            case ComparisonType.GreaterOrEqual: return value >= target;
            case ComparisonType.LessOrEqual: return value <= target;
            default: return false;
        }
    }

    public void ApplyIntAction(string key, ActionType action, int value)
    {
        switch (action)
        {
            case ActionType.Increase: VariablesManager.Instance.ModifyValue(key, +value); break;
            case ActionType.Decrease: VariablesManager.Instance.ModifyValue(key, -value); break;
        }
    }

    // Random
    public bool RandomCondition(int difficulty)
    {
        int R = Random.Range(0, 101);
        return R <= difficulty;
    }
}