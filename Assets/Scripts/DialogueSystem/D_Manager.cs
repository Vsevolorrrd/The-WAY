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
        private conditionManager conditionManager;
        private D_EventManager eventManager;

        protected override void OnAwake()
        {
            conditionManager = GetComponent<conditionManager>();
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
            Debug.Log("Basic node");
            awatingImput = true;
        }

        private void ChoiceNode(DialogueNodeData nodeData)
        {
            dialogueText.text = ProcessProperties(nodeData.DialogueText);

            // Display choices
            var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID);
            foreach (var choice in choices)
            {
                var button = Instantiate(choicePrefab, buttonContainer);
                button.GetComponentInChildren<Text>().text = ProcessProperties(choice.DisplayText);
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
                    conditionManager.AddStringCondition(nodeData.EventName);
                    break;

                case DialogueEventType.SetBooleanCondition:
                    conditionManager.SetBoolCondition(nodeData.EventName, true);
                    break;

                case DialogueEventType.PostEffect:
                    // fire event
                    break;

                case DialogueEventType.PlaySound:
                    SoundData soundData = AudioManager.Instance.GetSoundByName(nodeData.EventName);
                    if (soundData != null) AudioManager.Instance.PlaySound(soundData.Clip, soundData.Volume);
                    else Debug.LogWarning($"Sound '{nodeData.EventName}' not found in AudioManager.");
                    break;

                case DialogueEventType.ScreenShake:

                    switch (nodeData.EventName)
                    {
                        case "light":
                            // shake
                            break;
                        case "medium":
                            // shake
                            break;
                        case "heavy":
                            // shake
                            break;

                    }
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

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in dialogue.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }
        private void EndNode(DialogueNodeData nodeData)
        {
            dialogueText.text = ProcessProperties(nodeData.DialogueText);
            Debug.Log("Dialogue has ended.");
            // trigger an event
        }
        private void Update()
        {
            if (!awatingImput) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var nextLink = dialogue.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == dialogueNodeData.NodeGUID);
                if (nextLink != null)
                {
                    ProceedToNarrative(nextLink.TargetNodeGUID);
                    dialogueNodeData = null;
                }
            }
        }
    }
}