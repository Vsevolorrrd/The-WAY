using Subtegral.DialogueSystem.DataContainers;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class D_Camera : Singleton<D_Camera>
{
    [SerializeField] AnimationCurve cameraEasing = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform targetTransform;

    private PlayerCamera playerCam;
    private Vector3 originalOffset;
    private Coroutine currentCoroutine;

    protected override void OnAwake()
    {
        playerCam = GetComponent<PlayerCamera>();
        if (virtualCamera != null && virtualCamera.Follow != null)
        originalOffset = virtualCamera.transform.position - virtualCamera.Follow.position;
    }
    public void MoveDialogueCamera(CameraActionType action, float duration, Vector3 vector)
    {
        if (currentCoroutine != null)
        StopCoroutine(currentCoroutine);

        switch (action)
        {
            case CameraActionType.MoveBy:
                currentCoroutine = StartCoroutine(MoveBy(duration, vector));
                break;

            case CameraActionType.MoveToPosition:
                currentCoroutine = StartCoroutine(MoveToPosition(duration, vector));
                break;

            case CameraActionType.MoveToCharacter:
                currentCoroutine = StartCoroutine(MoveToCharacter(duration));
                break;

            case CameraActionType.TrackCharacter:
                TrackCharacter(duration);
                break;

            case CameraActionType.ZoomIn:
                currentCoroutine = StartCoroutine(ZoomTo(duration, 6f));
                break;

            case CameraActionType.ZoomOut:
                currentCoroutine = StartCoroutine(ZoomTo(duration, 10f));
                break;
        }
    }

    private IEnumerator MoveBy(float duration, Vector3 offset)
    {
        Vector3 start = virtualCamera.transform.position;
        Vector3 end = start + offset;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = cameraEasing.Evaluate(elapsed / duration);
            virtualCamera.transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        virtualCamera.transform.position = end;
    }

    private IEnumerator MoveToPosition(float duration, Vector3 targetPos)
    {
        Vector3 start = virtualCamera.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = cameraEasing.Evaluate(elapsed / duration);
            virtualCamera.transform.position = Vector3.Lerp(start, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        virtualCamera.transform.position = targetPos;
    }

    private IEnumerator MoveToCharacter(float duration)
    {
        if (targetTransform == null)
        yield break;

        Vector3 target = targetTransform.position + originalOffset;
        yield return MoveToPosition(duration, target);
    }

    private void TrackCharacter(float duration)
    {
        if (targetTransform == null)
        return;

        virtualCamera.Follow = targetTransform;
        virtualCamera.LookAt = targetTransform;
    }

    private IEnumerator ZoomTo(float duration, float targetFOV)
    {
        float startFOV = virtualCamera.Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = cameraEasing.Evaluate(elapsed / duration);
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startFOV, targetFOV, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        virtualCamera.Lens.OrthographicSize = targetFOV;
    }
}