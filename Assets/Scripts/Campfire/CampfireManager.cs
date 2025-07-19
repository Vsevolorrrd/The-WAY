using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Runtime;
using System.Collections.Generic;
using UnityEngine;

public class CampfireManager : Singleton<CampfireManager>
{
    public enum CampfireState
    {
        RationState,
        DialogueState,
        CampState
    }

    [SerializeField] private List<DialogueContainer> allCampfireDialogues;

    private Queue<DialogueContainer> dialogueQueue = new();
    private D_conditionManager conditionManager;
    private D_Manager dialogueManager;

    private bool isRunning = false;
    private DialogueContainer currentDialogue;

    public CampfireState CurrentState { get; private set; } = CampfireState.CampState;

    protected override void OnAwake()
    {
        dialogueManager = D_Manager.Instance;
        conditionManager = D_Manager.Instance.GetComponent<D_conditionManager>();
        StartCampfireSequence();
    }

    public void StartCampfireSequence()
    {
        if (isRunning) return;

        isRunning = true;
        CurrentState = CampfireState.RationState;

        ShowRationUI();
    }
    private void ShowRationUI()
    {
        Debug.Log("Rationing phase started. Choose who to feed...");
    }
    public void ProceedFromRationPhase()
    {
        QueueMatchingDialogues();

        if (dialogueQueue.Count > 0)
        {
            CurrentState = CampfireState.DialogueState;
            AdvanceCampfire();
        }
        else
        {
            EndMemorySequence();
        }
    }
    public void OnConfirmRationing()
    {
        ProceedFromRationPhase();
    }

    private void QueueMatchingDialogues()
    {
        dialogueQueue.Clear();

        var memoryField = typeof(D_conditionManager).
        GetField("memories", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var memoryList = memoryField?.GetValue(conditionManager) as List<string>;
        if (memoryList == null) return;

        foreach (var dialogue in allCampfireDialogues)
        {
            if (memoryList.Contains(dialogue.name))
            dialogueQueue.Enqueue(dialogue);
        }
    }

    public void AdvanceCampfire()
    {
        if (dialogueQueue.Count == 0)
        {
            EndMemorySequence();
            return;
        }

        currentDialogue = dialogueQueue.Dequeue();
        dialogueManager.StartDialogue(currentDialogue);
    }

    public void EndMemorySequence()
    {
        isRunning = false;
        currentDialogue = null;
        CurrentState = CampfireState.CampState;
    }
    public void EndCampfire()
    {

    }

}