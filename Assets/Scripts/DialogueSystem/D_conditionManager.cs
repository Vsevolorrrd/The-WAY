using Characters;
using Subtegral.DialogueSystem.DataContainers;
using UnityEngine;

public class D_conditionManager : MonoBehaviour
{
    private VariablesManager variablesManager;
    private void Start()
    {
        variablesManager = VariablesManager.Instance;
    }
    // memories
    public bool MemoryCondition(string memory)
    {
        return variablesManager.memories.Contains(memory);
    }
    public void AddMemory(string memory)
    {
        if (!variablesManager.memories.Contains(memory))
            variablesManager.memories.Add(memory);
    }

    // strings
    public bool StringCondition(string condition)
    {
        return variablesManager.stringConditions.Contains(condition);
    }
    public void AddStringCondition(string condition)
    {
        if (!variablesManager.stringConditions.Contains(condition))
        variablesManager.stringConditions.Add(condition);
    }

    // bools
    public bool BoolCondition(string key, bool expected)
    {
        return variablesManager.boolConditions.TryGetValue(key, out var value) && value == expected;
    }

    public void SetBoolCondition(string key, bool value)
    {
        variablesManager.boolConditions[key] = value;
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

    // Character
    public bool CharacterCondition(DialogueNodeData nodeData)
    {
        string characterID = nodeData.Actor;

        var character = CharacterManager.Instance.GetCharacter(characterID);
        if (character == null)
        {
            Debug.LogWarning($"CharacterCondition: Character '{characterID}' not found.");
            return false;
        }

        bool result = false;

        switch (nodeData.CharacterAttribute)
        {
            case CharacterAttribute.Relations:
                result = CompareRelations(character, nodeData.CharacterTarget, nodeData.CharacterComparisonValue);
                break;
            case CharacterAttribute.Morale:
                break;
            case CharacterAttribute.Stamina:
                break;
            case CharacterAttribute.IsAlive:
                break;
            case CharacterAttribute.IsInScene:
                result = CharacterManager.Instance.GetCharacterInScene(characterID) != null;
                break;
        }

        if (!result && nodeData.CharacterAttribute == CharacterAttribute.IsInScene && nodeData.CharacterAction != CharacterAction.None)
        {
            HandleCharacterAction(nodeData.CharacterAction, character);
        }

        return result;
    }
    private bool CompareRelations(Character character, CharacterTarget charTarget, int value)
    {
        CompanionInstance instance = CharacterManager.Instance.GetCharacterInstance(character.CharacterID);
        if (instance == null)
        {
            Debug.LogWarning($"CompareRelations: No instance found for character '{character.CharacterID}'");
            return false;
        }

        int relationValue = 0;
        switch (charTarget)
        {
            case CharacterTarget.Player:
                relationValue = instance.Relations_Player;
                break;
            case CharacterTarget.Doc:
                relationValue = instance.Relations_Doc;
                break;
            case CharacterTarget.Gravehound:
                relationValue = instance.Relations_Grave;
                break;
            case CharacterTarget.Rook:
                relationValue = instance.Relations_Rook;
                break;
            case CharacterTarget.Vale:
                relationValue = instance.Relations_Vale;
                break;
            case CharacterTarget.Ash:
                relationValue = instance.Relations_Ash;
                break;
            default:
                Debug.LogWarning($"CompareRelations: Unknown target '{charTarget}'");
                return false;
        }

        return relationValue >= value;
    }
    private void HandleCharacterAction(CharacterAction action, Character character)
    {
        Vector3 spawnPosition = Vector3.zero;

        switch (action)
        {
            case CharacterAction.SpawnOnRight:
                spawnPosition = new Vector3(4, 0, 0);
                break;
            case CharacterAction.SpawnOnLeft:
                spawnPosition = new Vector3(-4, 0, 0);
                break;
        }

        CharacterManager.Instance.SpawnCharacter(character, spawnPosition);
    }
}