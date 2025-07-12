using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Runtime;
using UnityEngine;

public class D_StartTrigger : MonoBehaviour
{
    [SerializeField] DialogueContainer dialogue;

    private void Start()
    {
        D_Manager.Instance.StartDialogue(dialogue);
    }
}