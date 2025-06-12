using System;
using UnityEngine;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public class DialogueCameraNode
    {
        public CameraActionType CameraActionType;
        public float CameraActionDuration = 1f;
        public Vector3 CameraActionPosition = Vector3.zero;
    }

    public enum CameraActionType
    {
        MoveBy,
        MoveToPosition,
        TrackCharacter,
        MoveToCharacter,
        ZoomOut,
        ZoomIn
    }
}