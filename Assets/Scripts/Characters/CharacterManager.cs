using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField] GameObject characterPrefab;
        public static CharacterManager Instance { get; private set; }

        private Dictionary<string, Character> characters = new();
        public IReadOnlyDictionary<string, Character> Characters => characters;

        private Dictionary<string, GameObject> activeCharacters = new();
        public IReadOnlyDictionary<string, GameObject> ActiveCharacters => activeCharacters;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAllCharacters();
        }

        void LoadAllCharacters()
        {
            Character[] loadedCharacters = Resources.LoadAll<Character>("Characters");

            foreach (var character in loadedCharacters)
            {
                if (!characters.ContainsKey(character.CharacterID))
                characters.Add(character.CharacterID, character);
                else
                Debug.LogWarning($"Duplicate character ID: {character.CharacterID}");
            }
        }
        public GameObject SpawnCharacter(Character character, Vector3 position)
        {
            if (activeCharacters.ContainsKey(character.CharacterID))
            {
                Debug.LogWarning($"Character '{character.CharacterID}' is already spawned.");
                return activeCharacters[character.CharacterID];
            }

            var instance = Instantiate(characterPrefab, position, Quaternion.identity);
            var characterHolder = instance.GetComponent<CharacterHolder>();

            if (characterHolder != null)
            characterHolder.CreateCharacter(character);

            activeCharacters[character.CharacterID] = instance;
            return instance;
        }
        public void DespawnCharacter(string id)
        {
            if (activeCharacters.TryGetValue(id, out var character))
            {
                Destroy(character);
                activeCharacters.Remove(id);
            }
        }

        public Character GetCharacter(string id)
        {
            if (characters.TryGetValue(id, out var character))
            return character;
            Debug.LogWarning($"Character with ID '{id}' not found.");
            return null;
        }

        public GameObject GetCharacterInScene(string id)
        {
            if (activeCharacters.TryGetValue(id, out var characterObj))
            return characterObj;

            Debug.LogWarning($"Character with ID '{id}' is not currently in the scene.");
            return null;
        }
        /*
        public void KillCharacter(string id)
        {
            if (characters.TryGetValue(id, out var character))
            character.isDead = true;
        }

        public bool IsCharacterDead(string id)
        {
            return characters.TryGetValue(id, out var c) && c.isDead;
        }
        */
    }
}