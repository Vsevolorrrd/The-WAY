using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public PlayerInteraction playerInteraction;

    protected override void OnAwake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }
}