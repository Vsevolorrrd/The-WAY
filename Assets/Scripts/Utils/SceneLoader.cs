using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using Characters;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject obj;
    [SerializeField] GameObject squadHolderPrefab;
    [SerializeField] Vector3 squadSpawnPosition = Vector3.zero;
    [SerializeField] float spacing = 2f; // Distance between companions

    private static SceneLoader _instance;

    #region Singleton
    public static SceneLoader Instance
    {
        get
        {
            // Check if the instance is already created
            if (_instance == null)
            {
                // Try to find an existing SceneLoader in the scene
                _instance = FindAnyObjectByType<SceneLoader>();

                // If no SceneLoader exists, create a new one
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SceneLoader");
                    _instance = singletonObject.AddComponent<SceneLoader>();
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        // If the instance is already set, destroy this duplicate object
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;  // Assign this object as the instance
        }
    }
    #endregion

    private void Start()
    {
        if (obj)
        obj.SetActive(true);
    }
    public void LoadScene(string sceneName, List<string> companionIDs = null)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, companionIDs));
    }
    public void RestartScene()
    {
        //StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().name));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, List<string> companionIDs = null)
    {
        if (anim) anim.SetTrigger("Start");

        yield return new WaitForSeconds(1f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        yield return loadOp;

        if (companionIDs != null && companionIDs.Count > 0)
        OnSceneLoaded(companionIDs);

        if (anim) anim.SetTrigger("End");
    }

    private void OnSceneLoaded(List<string> companionIDs = null)
    {
        ClearSceneCharacters();

        GameObject squadHolder = Instantiate(squadHolderPrefab, squadSpawnPosition, Quaternion.identity);
        squadHolder.name = "SquadHolder";

        float startX = -(companionIDs.Count - 1) * 0.5f * spacing;

        for (int i = 0; i < companionIDs.Count; i++)
        {
            string id = companionIDs[i];
            CompanionInstance instance = CharacterManager.Instance.GetCharacterInstance(id);

            if (instance == null)
            {
                Debug.LogWarning($"CompanionInstance '{id}' not found.");
                continue;
            }

            Character character = CharacterManager.Instance.GetCharacter(id);
            if (character == null)
                continue;

            Vector3 localPos = new Vector3(startX + i * spacing, 0, 0);
            Vector3 worldPos = squadHolder.transform.TransformPoint(localPos);

            GameObject characterGO = CharacterManager.Instance.SpawnCharacter(character, worldPos);
            if (characterGO != null)
            characterGO.transform.SetParent(squadHolder.transform);
        }
    }

    public void ClearSceneCharacters()
    {
        var activeChars = CharacterManager.Instance.ActiveCharacters;
        foreach (var pair in new List<string>(activeChars.Keys))
        {
            CharacterManager.Instance.DespawnCharacter(pair);
        }
    }
}