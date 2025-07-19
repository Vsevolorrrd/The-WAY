using System.Collections.Generic;
using System;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    [Serializable]
    public class Resource
    {
        public string name;
        public int amount;
    }

    [Header("Initial Resources")]
    public List<Resource> startingResources;

    private Dictionary<string, int> resources = new();
    public event Action<string, int> OnResourceChanged;

    protected override void OnAwake()
    {
        foreach (var res in startingResources)
        {
            resources[res.name] = res.amount;
        }
    }

    public int Get(string resourceName)
    {
        resources.TryGetValue(resourceName, out int value);
        return value;
    }

    public void Add(string resourceName, int amount)
    {
        if (!resources.ContainsKey(resourceName))
        resources[resourceName] = 0;

        resources[resourceName] += amount;
        OnResourceChanged?.Invoke(resourceName, resources[resourceName]);
    }

    public bool Spend(string resourceName, int amount)
    {
        if (Get(resourceName) < amount)
        return false;

        resources[resourceName] -= amount;
        OnResourceChanged?.Invoke(resourceName, resources[resourceName]);
        return true;
    }

    public bool Has(string resourceName, int amount)
    {
        return Get(resourceName) >= amount;
    }

    public Dictionary<string, int> GetAllResources()
    {
        return new Dictionary<string, int>(resources); // Return a copy
    }
}