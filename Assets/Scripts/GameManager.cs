using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private GamePhase currentPhase;
    private TravelManager travel;
    private S_Controller scene;
    private CampfireManager campfire;

    protected override void OnAwake()
    {
        travel = GetComponent<TravelManager>();
        scene = GetComponent<S_Controller>();
        campfire = GetComponent<CampfireManager>();
    }

    private void Start()
    {
        StartNewDay();
    }

    public void StartNewDay()
    {
        currentPhase = GamePhase.Travel;
    }

    public void ProceedToNextPhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Travel:
                currentPhase = GamePhase.Encounter;
                break;
            case GamePhase.Encounter:
                currentPhase = GamePhase.Campfire;
                break;
            case GamePhase.Campfire:
                currentPhase = GamePhase.Travel;
                break;
        }
    }
}
public enum GamePhase { Travel, Encounter, Campfire, Scripted }