using Subtegral.DialogueSystem.DataContainers;
using UnityEditor.Experimental.GraphView;

namespace Subtegral.DialogueSystem.Editor
{
    public class DialogueNode : Node
    {
        public string DialogueText;
        public string GUID;
        public bool EntyPoint = false;

        public string DisplayText;
        public DialogueNodeType NodeType = DialogueNodeType.Basic;
        public string Actor = "Unknown";
        public string AnimationName;

        // Conditions
        public BoolCondition BoolCondition;
        public IntCondition IntCondition;
        public StringCondition StringCondition;

        // Events
        public DialogueEvent Event;
    }
}