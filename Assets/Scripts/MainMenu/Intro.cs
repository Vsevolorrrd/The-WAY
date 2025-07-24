using UnityEngine;

public class Intro : MonoBehaviour
{
    [SerializeField] float IntroTime;
    private void Start()
    {
        Invoke("LoadScene", IntroTime);
    }
    private void LoadScene()
    {
        SceneLoader.Instance.LoadScene("ActI_Part1");
    }
}
