using System.Collections.Generic;
using UnityEngine;
using System;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public class CommentBlockData
    {
        public List<string> ChildNodes = new List<string>();
        public Vector2 Position;
        public string Title= "Comment Block";
    }
}