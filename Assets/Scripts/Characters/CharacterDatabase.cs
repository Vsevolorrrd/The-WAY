using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
    public static class CharacterDatabase
    {
        public static List<Character> LoadAllCharacters()
        {
            var assets = Resources.LoadAll<Character>("Characters"); // Adjust path as needed
            return assets.ToList();
        }

        public static List<string> GetCharacterIDs()
        {
            return LoadAllCharacters().Select(c => c.CharacterID).ToList();
        }
        public static Character GetCharacterByID(string id)
        {
            return LoadAllCharacters().FirstOrDefault(c => c.CharacterID == id);
        }

        public static List<string> GetCharacterNames()
        {
            return LoadAllCharacters().Select(c => c.CharacterName).ToList();
        }

        public static string GetCharacterIDFromName(string name)
        {
            if (name == "Narrator") return "narrator_id";  // Special case for Narrator
            var character = LoadAllCharacters().FirstOrDefault(c => c.CharacterName == name);
            return character != null ? character.CharacterID : "Unknown";
        }

        public static string GetCharacterNameFromID(string id)
        {
            if (id == "narrator_id") return "Narrator";  // Special case for Narrator
            var character = LoadAllCharacters().FirstOrDefault(c => c.CharacterID == id);
            return character != null ? character.CharacterName : "Unknown";
        }

        public static List<string> GetAnimationsForCharacter(string characterName)
        {
            var character = LoadAllCharacters().FirstOrDefault(c => c.CharacterName == characterName);
            return character != null ? character.GetAnimationNames() : new List<string>();
        }
        public static AnimationClip GetAnimationByID(string id, string animationName)
        {
            var character = LoadAllCharacters().FirstOrDefault(c => c.CharacterID == id);
            return character != null ? character.GetAnimationByName(animationName) : null;
        }
    }
}