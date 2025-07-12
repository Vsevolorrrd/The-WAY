using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainBlock;

    public void StartGame()
    {
        mainBlock.SetActive(false);
        SceneLoader.Instance.LoadScene("ActI_Part1");
    }
    public void OpenMainMenu()
    {
        SceneLoader.Instance.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}