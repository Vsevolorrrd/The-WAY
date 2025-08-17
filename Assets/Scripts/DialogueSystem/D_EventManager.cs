using Subtegral.DialogueSystem.DataContainers;
using UnityEngine;
using Characters;

public class D_EventManager : MonoBehaviour
{
    public void DialogueEvent(DialogueNodeData nodeData, D_conditionManager conditionManager, D_UI UI)
    {
        switch (nodeData.EventType)
        {
            case DialogueEventType.Custom:
                // add
                break;

            case DialogueEventType.SetStringCondition:
                conditionManager.AddStringCondition(nodeData.EventName.ToLowerInvariant());
                break;

            case DialogueEventType.SetBooleanCondition:
                if (nodeData.EventValue > 0)
                conditionManager.SetBoolCondition(nodeData.EventName.ToLowerInvariant(), true);
                else
                conditionManager.SetBoolCondition(nodeData.EventName.ToLowerInvariant(), false);
                break;

            case DialogueEventType.ChangeInteger:
                VariablesManager.Instance.ModifyValue(nodeData.EventName.ToLowerInvariant(), Mathf.RoundToInt(nodeData.EventValue));
                break;

            case DialogueEventType.PlaySound:
                AudioClip clip = AudioManager.Instance.GetSoundByName(nodeData.EventName);
                if (clip != null) AudioManager.Instance.PlaySound(clip, nodeData.EventValue);
                else Debug.LogWarning($"Sound '{nodeData.EventName}' not found in AudioManager.");
                break;

            case DialogueEventType.PlayMusic:
                if(MusicManager.Instance)
                MusicManager.Instance.StartMusic(nodeData.EventValue);
                break;

            case DialogueEventType.StopAllMusic:
                if (MusicManager.Instance)
                MusicManager.Instance.StopMusic(nodeData.EventValue);
                break;

            case DialogueEventType.ScreenShake:

                switch (nodeData.EventName.ToLowerInvariant())
                {
                    case "light":
                        CameraShake.Instance.ShakeCamera(1f, nodeData.EventValue);
                        break;
                    case "heavy":
                        CameraShake.Instance.ShakeCamera(5f, nodeData.EventValue);
                        break;
                    default:
                        CameraShake.Instance.ShakeCamera(3f, nodeData.EventValue);
                        break;

                }
                break;

            case DialogueEventType.PostEffect:
                PostFXManager.Instance.DialoguePostEffect(nodeData.EventName, nodeData.EventValue);
                break;

            case DialogueEventType.AddMemory:
                UI.AddedMemory();
                conditionManager.AddMemory(nodeData.EventName);
                break;

            case DialogueEventType.SetPlayerName:
                // change player name
                break;

            default:
                Debug.LogWarning($"Invalid event: {nodeData.NodeType}");
                break;
        }
    }
    public void ChangeAttribute(DialogueNodeData nodeData)
    {
        string characterID = nodeData.Actor;
        int value = nodeData.AttributeValue;

        var character = CharacterManager.Instance.GetCharacter(characterID);
        if (character == null)
        {
            Debug.LogWarning($"ChangeAttribute: Character '{characterID}' not found.");
            return;
        }

        CompanionInstance instance = CharacterManager.Instance.GetCharacterInstance(characterID);
        if (instance == null)
        {
            Debug.LogWarning($"ChangeAttribute: Instance for character '{characterID}' not found.");
            return;
        }

        switch (nodeData.CharacterAttribute)
        {
            case CharacterAttribute.Relations:
                ApplyRelationChange(instance, nodeData.CharacterTarget, value);
                break;

            case CharacterAttribute.Morale:
                instance.Morale = ClampStat(instance.Morale + value, 0, 100);
                Debug.Log($"Morale of '{characterID}' changed to {instance.Morale}");
                break;

            case CharacterAttribute.Stamina:
                instance.Stamina = ClampStat(instance.Stamina + value, 0, 100);
                Debug.Log($"Stamina of '{characterID}' changed to {instance.Stamina}");
                break;

            case CharacterAttribute.IsAlive:
                instance.IsDead = value <= 0;
                Debug.Log($"'{characterID}' IsAlive set to {!instance.IsDead}");
                break;

            default:
                Debug.LogWarning($"ChangeAttribute: Unknown attribute '{nodeData.CharacterAttribute}'");
                break;
        }
    }
    private void ApplyRelationChange(CompanionInstance instance, CharacterTarget target, int value)
    {
        switch (target)
        {
            case CharacterTarget.Player:
                instance.Relations_Player = ClampStat(instance.Relations_Player + value, 0, 100);
                Debug.Log($"Relations with Player changed to {instance.Relations_Player}");
                break;

            case CharacterTarget.Doc:
                instance.Relations_Doc = ClampStat(instance.Relations_Doc + value, 0, 100);
                Debug.Log($"Relations with Doc changed to {instance.Relations_Doc}");
                break;

            case CharacterTarget.Gravehound:
                instance.Relations_Grave = ClampStat(instance.Relations_Grave + value, 0, 100);
                Debug.Log($"Relations with Gravehound changed to {instance.Relations_Grave}");
                break;

            case CharacterTarget.Rook:
                instance.Relations_Rook = ClampStat(instance.Relations_Rook + value, 0, 100);
                Debug.Log($"Relations with Rook changed to {instance.Relations_Rook}");
                break;

            case CharacterTarget.Vale:
                instance.Relations_Vale = ClampStat(instance.Relations_Vale + value, 0, 100);
                Debug.Log($"Relations with Vale changed to {instance.Relations_Vale}");
                break;

            case CharacterTarget.Ash:
                instance.Relations_Ash = ClampStat(instance.Relations_Ash + value, 0, 100);
                Debug.Log($"Relations with Ash changed to {instance.Relations_Ash}");
                break;

            default:
                Debug.LogWarning($"ApplyRelationChange: Unknown target '{target}'");
                break;
        }
    }
    private int ClampStat(int value, int min, int max)
    {
        return Mathf.Clamp(value, min, max);
    }
}