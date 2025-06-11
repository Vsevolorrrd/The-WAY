using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        private Dictionary<string, Character> characters = new();
        public IReadOnlyDictionary<string, Character> Characters => characters;

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

        public Character GetCharacter(string id)
        {
            if (characters.TryGetValue(id, out var character))
            return character;
            Debug.LogWarning($"Character with ID '{id}' not found.");
            return null;
        }

        public void KillCharacter(string id)
        {
            if (characters.TryGetValue(id, out var character))
            character.isDead = true;
        }

        public bool IsCharacterDead(string id)
        {
            return characters.TryGetValue(id, out var c) && c.isDead;
        }
    }
}