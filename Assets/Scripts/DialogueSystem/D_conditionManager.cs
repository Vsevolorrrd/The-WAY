using Characters;
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
                //result = CompareRelations(character, nodeData.CharacterTarget, nodeData.CharacterComparisonValue);
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
    /*
    private bool CompareRelations(Character character, CharacterTarget charTarget, int value)
    {
        var target;

        switch (charTarget)
        {
            case CharacterTarget.Player:
                target = CharacterManager.Instance.GetCharacter("Player");
                if (target == null)
                {
                    Debug.LogWarning("CharacterCondition: Character Player not found.");
                    return false;
                }
                break;
            case CharacterTarget.Doc:
                target = CharacterManager.Instance.GetCharacter("Player");
                if (target == null)
                {
                    Debug.LogWarning("CharacterCondition: Character Player not found.");
                    return false;
                }
                break;
            case CharacterTarget.Gravehound:
                target = CharacterManager.Instance.GetCharacter("Player");
                if (target == null)
                {
                    Debug.LogWarning("CharacterCondition: Character Player not found.");
                    return false;
                }
                break;
            case CharacterTarget.Rook:
                result = character.IsAlive;
                break;
            case CharacterTarget.Vale:
                result = CharacterManager.Instance.GetCharacterInScene(characterID) != null;
                break;
            case CharacterTarget.Ash:
                result = CharacterManager.Instance.GetCharacterInScene(characterID) != null;
                break;
        }
    }
    */
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