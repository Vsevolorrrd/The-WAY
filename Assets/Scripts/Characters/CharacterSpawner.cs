using Characters;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] CharactersAndPositions[] characters;
    void Start()
    {
        foreach (CharactersAndPositions characterPos in characters)
        {
            CharacterManager.Instance.SpawnCharacter(characterPos.character, characterPos.position);
        }
    }
}
[System.Serializable]
public class CharactersAndPositions
{
    public Character character;
    public Vector3 position;
}