using System;
namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public class NodeLinkData
    {
        public string PortName;
        public string BaseNodeGUID;
        public string TargetNodeGUID;

        public string DisplayText;
    }
}