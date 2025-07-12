using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Runtime;
using UnityEngine;

public class D_Interactive : MonoBehaviour, IInteractable
{
    [SerializeField] DialogueContainer dialogue;
    public void Interact()
    {
        D_Manager.Instance.StartDialogue(dialogue);
    }
}