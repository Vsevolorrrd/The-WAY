using Subtegral.DialogueSystem.DataContainers;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Subtegral.DialogueSystem.Runtime
{
    public class DialogueManager : Singleton<DialogueManager>
    {
        [SerializeField] private DialogueContainer dialogue;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button choicePrefab;
        [SerializeField] private Transform buttonContainer;
        private DialogueNodeData dialogueNodeData;
        private bool awatingImput = false;

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
                    HandleEndNode(nodeData);
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

        private void StringConditionNode(DialogueNodeData nodeData)
        {

        }
        private void BoolConditionNode(DialogueNodeData nodeData)
        {

        }
        private void IntConditionNode(DialogueNodeData nodeData)
        {

        }

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in dialogue.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }
        private void HandleEndNode(DialogueNodeData nodeData)
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