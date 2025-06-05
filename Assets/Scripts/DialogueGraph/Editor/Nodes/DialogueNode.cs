using Subtegral.DialogueSystem.DataContainers;
using UnityEditor.Experimental.GraphView;
using Characters;

namespace Subtegral.DialogueSystem.Editor
{
    public class DialogueNode : Node
    {
        public string DialogueText;
        public string GUID;
        public bool EntyPoint = false;

        public string DisplayText;
        public DialogueNodeType NodeType = DialogueNodeType.Basic;
        public CharacterID Actor = CharacterID.Unknown;

        // Conditions
        public BoolCondition BoolCondition;
        public IntCondition IntCondition;
        public StringCondition StringCondition;

        // Events
        public DialogueEvent Event;
    }
}