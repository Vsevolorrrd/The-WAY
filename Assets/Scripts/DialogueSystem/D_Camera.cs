using Subtegral.DialogueSystem.DataContainers;
using UnityEngine;

public class D_Camera : Singleton<D_Camera>
{
    [SerializeField] AnimationCurve cameraEasing = AnimationCurve.Linear(0, 0, 1, 1);
    public void MoveDialogueCamera(CameraActionType action, float duration, Vector3 vector)
    {
        switch (action)
        {
            case CameraActionType.MoveBy: 
                MoveBy(duration, vector); 
                break;

            case CameraActionType.MoveToPosition:
                MoveToPosition(duration, vector);
                break;

            case CameraActionType.MoveToCharacter:
                MoveToCharacter(duration);
                break;

            case CameraActionType.TrackCharacter:
                TrackCharacter(duration);
                break;

            case CameraActionType.ZoomIn:
                ZoomIn(duration);
                break;

            case CameraActionType.ZoomOut:
                ZoomOut(duration);
                break;
        }
    }

    private void MoveBy(float duration, Vector3 vector)
    {

    }
    private void MoveToPosition(float duration, Vector3 vector)
    {

    }
    private void MoveToCharacter(float duration)
    {

    }
    private void TrackCharacter(float duration)
    {

    }
    private void ZoomIn(float duration)
    {

    }
    private void ZoomOut(float duration)
    {

    }
}