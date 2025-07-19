using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public PlayerInteraction playerInteraction;
    public Transform CameraAnchor;

    protected override void OnAwake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }
    public void BlockPlayerMovement(bool state)
    {
        playerInteraction.BlockMovement(state);
    }
}