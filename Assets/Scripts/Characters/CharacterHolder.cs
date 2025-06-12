using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class CharacterHolder : MonoBehaviour
    {
        private Character character;
        public string CharacterID { get; private set; }
        public string CharacterName { get; private set; }
        public bool IsDead { get; private set; }

        public Sprite Portrait { get; private set; }
        public List<CharacterAnimation> Animations { get; private set; }

        public void CreateCharacter(Character characterInfo)
        {
            character = characterInfo;
            CharacterID = characterInfo.CharacterID;
            CharacterName = characterInfo.CharacterName;
            Portrait = characterInfo.Portrait;
            Animations = new List<CharacterAnimation>(characterInfo.animations);
            IsDead = false;
        }

        public AnimationClip GetAnimation(string name)
        {
            return Animations.Find(entry => entry.animationName == name)?.animationClip;
        }
    }
}