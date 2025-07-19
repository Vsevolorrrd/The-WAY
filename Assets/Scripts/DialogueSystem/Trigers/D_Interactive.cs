using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Runtime;
using UnityEngine;

public class D_Interactive : MonoBehaviour, IInteractable
{
    [SerializeField] DialogueContainer dialogue;
    private bool interacted = false;
    public void Interact()
    {
        if (interacted) return;
        D_Manager.Instance.StartDialogue(dialogue);
        interacted = true;
    }
}