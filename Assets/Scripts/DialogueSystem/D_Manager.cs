using Subtegral.DialogueSystem.DataContainers;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Subtegral.DialogueSystem.Runtime
{
    public class D_Manager : Singleton<D_Manager>
    {
        [SerializeField] private DialogueContainer dialogue;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button choicePrefab;
        [SerializeField] private Transform buttonContainer;
        private DialogueNodeData dialogueNodeData;
        private bool awatingImput = false;

        // Dialogue managers
        private D_conditionManager conditionManager;
        private D_EventManager eventManager;

        protected override void OnAwake()
        {
            conditionManager = GetComponent<D_conditionManager>();
            eventManager = GetComponent<D_EventManager>();
        }

        private void Start()
        {
            var narrativeData = dialogue.NodeLinks.First(); //Entrypoint node
            ProceedToNarrative(narrativeData.TargetNodeGUID);
        }

        private void ProceedToNarrative(string narrativeDataGUID)
        {
            var nodeData = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID);

            dialogueText.text = null;
            var buttons = buttonContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i].gameObject);
            }

            switch (nodeData.NodeType)
            {
                case DialogueNodeType.Basic:
                    BasicNode(nodeData);
                    break;

                case DialogueNodeType.Choice:
                    ChoiceNode(nodeData);
                    break;

                case DialogueNodeType.Event:
                    EventNode(nodeData);
                    break;

                case DialogueNodeType.StringCondition:
                    StringConditionNode(nodeData);
                    break;

                case DialogueNodeType.BoolCondition:
                    BoolConditionNode(nodeData);
                    break;

                case DialogueNodeType.IntCondition:
                    IntConditionNode(nodeData);
                    break;

                case DialogueNodeType.Animation:
                    AnimationNode(nodeData);
                    break;

                case DialogueNodeType.Camera:
                    CameraNode(nodeData);
                    break;

                case DialogueNodeType.End:
                    EndNode(nodeData);
                    break;

                default:
                    Debug.LogWarning($"Invalid node type: {nodeData.NodeType}");
                    break;
            }
        }

        private void BasicNode(DialogueNodeData nodeData)
        {
            dialogueNodeData = nodeData;
            dialogueText.text = ProcessProperties(nodeData.DialogueText);
            awatingImput = true;
        }

        private void ChoiceNode(DialogueNodeData nodeData)
        {
            // Display choices
            var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID);
            foreach (var choice in choices)
            {
                var button = Instantiate(choicePrefab, buttonContainer);
                button.GetComponentInChildren<TextMeshProUGUI>().text = ProcessProperties(choice.DisplayText);
                button.onClick.AddListener(() => ProceedToNarrative(choice.TargetNodeGUID));
            }
        }
        private void EventNode(DialogueNodeData nodeData)
        {
            var nextLink = dialogue.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == nodeData.NodeGUID);
            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);

            switch (nodeData.EventType)
            {
                case DialogueEventType.Custom:
                    // fire event
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
                    // fire event
                    break;

                case DialogueEventType.PlaySound:
                    AudioClip clip = AudioManager.Instance.GetSoundByName(nodeData.EventName.ToLowerInvariant());
                    if (clip != null) AudioManager.Instance.PlaySound(clip, nodeData.EventValue);
                    else Debug.LogWarning($"Sound '{nodeData.EventName}' not found in AudioManager.");
                    break;

                case DialogueEventType.PlayMusic:
                    AudioClip music = AudioManager.Instance.GetSoundByName(nodeData.EventName.ToLowerInvariant());
                    if (music != null)
                    AudioManager.Instance.PlaySound(music, nodeData.EventValue, null, true);
                    else Debug.LogWarning($"Music '{nodeData.EventName}' not found in AudioManager.");
                    break;

                case DialogueEventType.StopAllMusic:
                    AudioManager.Instance.StopAllLoopSources(nodeData.EventValue);
                    break;

                case DialogueEventType.ScreenShake:

                    switch (nodeData.EventName.ToLowerInvariant())
                    {
                        case "light":
                            CameraShake.Instance.ShakeCamera(1f, nodeData.EventValue);
                            break;
                        case "medium":
                            CameraShake.Instance.ShakeCamera(3f, nodeData.EventValue);
                            break;
                        case "heavy":
                            CameraShake.Instance.ShakeCamera(5f, nodeData.EventValue);
                            break;

                    }
                    break;

                case DialogueEventType.PostEffect:
                    PostFXManager.Instance.DialoguePostEffect(nodeData.EventName.ToLowerInvariant(), nodeData.EventValue);
                    break;

                case DialogueEventType.SetPlayerName:
                    // change player name
                    break;


                default:
                    Debug.LogWarning($"Invalid event: {nodeData.NodeType}");
                    break;
            }
        }
        private void StringConditionNode(DialogueNodeData nodeData)
        {
            bool result = conditionManager.StringCondition(nodeData.StringConditionKey);

            var nextLink = dialogue.NodeLinks.FirstOrDefault(x =>
            x.BaseNodeGUID == nodeData.NodeGUID &&
            x.PortName == (result ? "True" : "False"));

            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);
        }
        private void BoolConditionNode(DialogueNodeData nodeData)
        {
            bool result = conditionManager.BoolCondition(nodeData.BoolConditionKey, nodeData.BoolConditionExpectedValue);

            var nextLink = dialogue.NodeLinks.FirstOrDefault(x =>
            x.BaseNodeGUID == nodeData.NodeGUID &&
            x.PortName == (result ? "True" : "False"));

            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);
        }
        private void IntConditionNode(DialogueNodeData nodeData)
        {
            bool result = conditionManager.IntCondition(
            nodeData.IntConditionKey,
            nodeData.IntConditionComparison,
            nodeData.IntConditionValue);

            if (result)
            {
                conditionManager.ApplyIntAction
                (nodeData.IntConditionKey, nodeData.IntActionType, nodeData.IntConditionValue);
            }

            var nextLink = dialogue.NodeLinks.FirstOrDefault(x =>
            x.BaseNodeGUID == nodeData.NodeGUID &&
            x.PortName == (result ? "True" : "False"));

            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);
        }
        private void AnimationNode(DialogueNodeData nodeData)
        {

        }
        private void CameraNode(DialogueNodeData nodeData)
        {
            D_Camera.Instance.MoveDialogueCamera
            (nodeData.CameraActionType, nodeData.CameraActionDuration, nodeData.CameraActionPosition);
        }

        private void EndNode(DialogueNodeData nodeData)
        {
            dialogueText.text = ProcessProperties(nodeData.DialogueText);
            Debug.Log("Dialogue has ended.");
            // trigger an event
        }

        private void Update()
        {
            if (!awatingImput || dialogueNodeData == null) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var nextLink = dialogue.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == dialogueNodeData.NodeGUID);
                if (nextLink != null)
                {
                    awatingImput = false;
                    ProceedToNarrative(nextLink.TargetNodeGUID);
                }
                else
                {
                    Debug.LogWarning("No next link found");
                }
            }
        }

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in dialogue.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }
    }
}