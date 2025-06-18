using System.Collections.Generic;
using UnityEngine;

public class VariablesManager : Singleton<VariablesManager>
{
    private Dictionary<string, int> integerValues = new();

    public void AddValue(string key, int value)
    {
        integerValues.Add(key, value);
    }

    public int GetValue(string key)
    {
        integerValues.TryGetValue(key, out var value);
        return value;
    }

    public void SetValue(string key, int value)
    {
        integerValues[key] = value;
    }

    public void ModifyValue(string key, int value)
    {
        if (!integerValues.ContainsKey(key))
        {
            Debug.LogWarning($"Integer values does not contain '{key}'");
            return;
        }

        integerValues[key] += value;
    }

    public bool TryGetValue(string key, out int value)
    {
        return integerValues.TryGetValue(key, out value);
    }
}
