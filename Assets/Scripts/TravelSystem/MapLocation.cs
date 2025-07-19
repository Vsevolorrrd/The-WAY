using UnityEngine;

public class MapLocation : MonoBehaviour, IInteractable
{
    [SerializeField] S_Scene scene;

    public void Interact()
    {
        SceneLoader.Instance.LoadScene(scene.SceneName);
    }
}