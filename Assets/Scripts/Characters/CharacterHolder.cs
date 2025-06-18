using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class CharacterHolder : MonoBehaviour
    {
        private Character character;
        private Animator animator;
        private Coroutine currentLoop;
        public string CharacterID { get; private set; }
        public string CharacterName { get; private set; }
        public bool IsDead { get; private set; }

        public Sprite Portrait { get; private set; }

        public void CreateCharacter(Character characterInfo)
        {
            character = characterInfo;
            CharacterID = characterInfo.CharacterID;
            CharacterName = characterInfo.CharacterName;
            Portrait = characterInfo.Portrait;
            IsDead = false;
            animator = GetComponent<Animator>();
        }
        public void PlayAnimation(string animationName, bool loop)
        {
            if (character == null || animator == null) return;

            AnimationClip clip = character.GetAnimationByName(animationName);
            if (clip == null)
            {
                Debug.LogWarning($"Animation '{animationName}' not found for character '{CharacterName}'");
                return;
            }

            animator.Play(clip.name); // Assumes clip is on Animator's controller, or...

            if (loop)
            {
                if (currentLoop != null) StopCoroutine(currentLoop);
                currentLoop = StartCoroutine(LoopAnimation(clip));
            }
        }

        private IEnumerator LoopAnimation(AnimationClip clip)
        {
            while (true)
            {
                animator.Play(clip.name);
                yield return new WaitForSeconds(clip.length);
            }
        }

        public void StopAnimationLoop()
        {
            if (currentLoop != null)
            {
                StopCoroutine(currentLoop);
                currentLoop = null;
            }
        }
    }
}