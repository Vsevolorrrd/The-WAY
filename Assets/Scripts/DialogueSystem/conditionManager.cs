using Subtegral.DialogueSystem.DataContainers;
using System.Collections.Generic;
using UnityEngine;

public class conditionManager : MonoBehaviour
{
    private List<string> stringConditions = new List<string>(24);
    private Dictionary<string, bool> boolConditions = new Dictionary<string, bool>(10);
    private Dictionary<string, int> intConditions = new Dictionary<string, int>(8);

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
        if (!intConditions.TryGetValue(key, out int value)) return false;

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

    public void SetIntCondition(string key, int value)
    {
        intConditions[key] = value;
    }

    public void ApplyIntAction(string key, ActionType action, int value)
    {
        if (!intConditions.ContainsKey(key)) return;

        switch (action)
        {
            case ActionType.Increase: intConditions[key] += value; break;
            case ActionType.Decrease: intConditions[key] -= value; break;
        }
    }
}