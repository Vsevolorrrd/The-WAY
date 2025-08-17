using Subtegral.DialogueSystem.DataContainers;
using System.Collections;
using System.Linq;
using UnityEngine;
using Characters;

namespace Subtegral.DialogueSystem.Runtime
{
    public class D_Manager : Singleton<D_Manager>
    {
        [SerializeField] DialogueContainer dialogueContainer;

        private DialogueNodeData savedDialogueNodeData;
        private Coroutine choiceTimerRoutine;
        private bool awatingImput = false;
        private bool dialogueIsGoing = false;

        // Dialogue managers
        private D_conditionManager conditionManager;
        private D_EventManager eventManager;
        private D_UI UIManager;

        public bool DialogueIsGoing() { return dialogueIsGoing; }
        protected override void OnAwake()
        {
            conditionManager = GetComponent<D_conditionManager>();
            eventManager = GetComponent<D_EventManager>();
            UIManager = GetComponent<D_UI>();
        }

        public void StartDialogue(DialogueContainer dialogue)
        {
            dialogueContainer = dialogue;
            UIManager.OpenDialogueUI();
            var narrativeData = dialogueContainer.NodeLinks.First(); //Entrypoint node
            ProceedToNarrative(narrativeData.TargetNodeGUID);
            dialogueIsGoing = true;
        }

        public void ProceedToNarrative(string narrativeDataGUID)
        {
            var nodeData = dialogueContainer.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID);
            UIManager.ClearButtons();

            switch (nodeData.NodeType)
            {
                case DialogueNodeType.Basic:
                    BasicNode(nodeData);
                    break;

                case DialogueNodeType.Choice:
                    ChoiceNode(nodeData);
                    break;

                case DialogueNodeType.TimedChoice:
                    TimedChoiceNode(nodeData);
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

                case DialogueNodeType.RandomCondition:
                    RandomConditionNode(nodeData);
                    break;

                case DialogueNodeType.CharacterCondition:
                    CharacterConditionNode(nodeData);
                    break;

                case DialogueNodeType.CharacterAttribute:
                    CharacterAttributeNode(nodeData);
                    break;

                case DialogueNodeType.Animation:
                    AnimationNode(nodeData);
                    break;

                case DialogueNodeType.MoveCharacter:
                    MoveCharacterNode(nodeData);
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
            UIManager.CreateText(nodeData);

            savedDialogueNodeData = nodeData;
            awatingImput = true;

            if (nodeData.CheckThisNode == false) return;
        }

        private void ChoiceNode(DialogueNodeData nodeData)
        {
            UIManager.CreateButtons(dialogueContainer, nodeData, this);
        }
        private void TimedChoiceNode(DialogueNodeData nodeData)
        {
            UIManager.CreateTimedButtons(dialogueContainer, nodeData, this);
            StartTimedCountdown(nodeData);
        }
        private void EventNode(DialogueNodeData nodeData)
        {
            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == nodeData.NodeGUID);
            if (nextLink == null) return;

            if (nodeData.EventType == DialogueEventType.Delay) // for delay
            {
                StartCoroutine(Delay(nextLink.TargetNodeGUID, nodeData.EventValue));
                return;
            }

            ProceedToNarrative(nextLink.TargetNodeGUID);
            eventManager.DialogueEvent(nodeData, conditionManager, UIManager);
        }
        private void StringConditionNode(DialogueNodeData nodeData)
        {
            bool result = false;

            if (nodeData.ConditionType == StringConditionType.Default)
            result = conditionManager.StringCondition(nodeData.StringConditionKey);
            else result = conditionManager.MemoryCondition(nodeData.StringConditionKey);

            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x =>
            x.BaseNodeGUID == nodeData.NodeGUID &&
            x.PortName == (result ? "True" : "False"));

            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);
        }
        private void BoolConditionNode(DialogueNodeData nodeData)
        {
            bool result = conditionManager.BoolCondition(nodeData.BoolConditionKey, nodeData.BoolConditionExpectedValue);

            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x =>
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

            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x =>
            x.BaseNodeGUID == nodeData.NodeGUID &&
            x.PortName == (result ? "True" : "False"));

            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);
        }
        private void RandomConditionNode(DialogueNodeData nodeData)
        {
            bool result = conditionManager.RandomCondition(nodeData.RandomConditionValue);

            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x =>
            x.BaseNodeGUID == nodeData.NodeGUID &&
            x.PortName == (result ? "True" : "False"));

            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);
        }
        private void CharacterConditionNode(DialogueNodeData nodeData)
        {
            bool result = conditionManager.CharacterCondition(nodeData);

            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x =>
            x.BaseNodeGUID == nodeData.NodeGUID &&
            x.PortName == (result ? "True" : "False"));

            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);
        }
        private void CharacterAttributeNode(DialogueNodeData nodeData)
        {
            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == nodeData.NodeGUID);
            if (nextLink == null) return;

            ProceedToNarrative(nextLink.TargetNodeGUID);
            eventManager.ChangeAttribute(nodeData);
        }
        private void AnimationNode(DialogueNodeData nodeData)
        {
            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == nodeData.NodeGUID);
            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);

            GameObject character = CharacterManager.Instance.GetCharacterInScene(nodeData.Actor);
            if (character)
            {
                var holder = character.GetComponent<CharacterHolder>();

                if (holder)
                holder.PlayAnimation(nodeData.AnimationName, nodeData.LoopAnimation);
            }
        }
        private void MoveCharacterNode(DialogueNodeData nodeData)
        {
            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == nodeData.NodeGUID);
            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);

            GameObject character = CharacterManager.Instance.GetCharacterInScene(nodeData.Actor);
            if (character)
            {
                var holder = character.GetComponent<CharacterHolder>();
                if (holder)
                holder.MoveToPosition(nodeData.MoveTo);
            }
        }

        private void CameraNode(DialogueNodeData nodeData)
        {
            var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == nodeData.NodeGUID);
            if (nextLink != null)
            ProceedToNarrative(nextLink.TargetNodeGUID);

            D_Camera.Instance.MoveDialogueCamera
            (nodeData.CameraActionType, nodeData.CameraActionDuration, nodeData.CameraActionPosition);
        }

        private void EndNode(DialogueNodeData nodeData)
        {
            Debug.Log("Dialogue has ended.");

            UIManager.CloseDialogueUI();
            dialogueIsGoing = false;
            if (CampfireManager.Instance != null)
            CampfireManager.Instance.AdvanceCampfire();
            if (nodeData.EndAction == EndAction.LoadScene)
            SceneLoader.Instance.LoadScene(nodeData.DialogueText);
            if (nodeData.EndAction == EndAction.StartDialogue)
            {
                var dialogueFiles = Resources.LoadAll<DialogueContainer>("Dialogues");
                var dialogueNames = dialogueFiles.Select(file => file.name).ToList();

                foreach (var dialogueName in dialogueNames)
                {
                    if (dialogueName == nodeData.DialogueText)
                    {
                        StartDialogue(Resources.Load<DialogueContainer>($"Dialogues/{dialogueName}"));
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            if (!awatingImput || savedDialogueNodeData == null) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (UIManager.SkipTypewriterIfActive()) return;

                var nextLink = dialogueContainer.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == savedDialogueNodeData.NodeGUID);
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
        public void StartTimedCountdown(DialogueNodeData nodeData)
        {
            var links = dialogueContainer.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID).ToList();
            var failLink = links.FirstOrDefault(x => x.PortName == "Fail");

            if (nodeData.FailTime > 0 && failLink != null)
            {
                if (choiceTimerRoutine != null)
                StopCoroutine(choiceTimerRoutine);

                choiceTimerRoutine = StartCoroutine(TimedFailCountdown(failLink.TargetNodeGUID, nodeData.FailTime));
            }
        }
        public void StopTimedCountDown()
        {
            if (choiceTimerRoutine != null)
            StopCoroutine(choiceTimerRoutine);
        }
        private IEnumerator TimedFailCountdown(string failTargetNodeGUID, float time)
        {
            float timer = 0f;

            while (timer < time)
            {
                timer += Time.deltaTime;
                float progress = timer / time;
                UIManager.UpdateTimedChoiceBar(progress);

                yield return null;
            }

            UIManager.HideChoiceBar();
            ProceedToNarrative(failTargetNodeGUID);
        }
        private IEnumerator Delay(string targetNodeGUID, float time)
        {
            float timer = 0f;

            while (timer < time)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            ProceedToNarrative(targetNodeGUID);
        }
    }
}