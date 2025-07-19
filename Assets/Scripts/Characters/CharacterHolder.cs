using System.Collections;
using UnityEngine;

namespace Characters
{
    public class CharacterHolder : MonoBehaviour
    {
        [SerializeField] Character character;
        private Animator animator;

        public string CharacterID { get; private set; }
        public string CharacterName { get; private set; }
        public bool IsDead { get; private set; }

        public Sprite Portrait { get; private set; }

        private void Start()
        {
            if (character == null) return;
            CreateCharacter(character);
            CharacterManager.Instance.RegisterCharacterInScene(CharacterID, gameObject);
        }
        public void CreateCharacter(Character characterInfo)
        {
            character = characterInfo;
            CharacterID = characterInfo.CharacterID;
            CharacterName = characterInfo.CharacterName;
            Portrait = characterInfo.Portrait;
            IsDead = false;
            animator = GetComponent<Animator>();
        }
        public void PlayAnimation(string animationName, bool loop = false)
        {
            if (character == null || animator == null) return;

            AnimationClip clip = character.GetAnimationByName(animationName);
            if (clip == null)
            {
                Debug.LogWarning($"Animation '{animationName}' not found for character '{CharacterName}'");
                return;
            }

            animator.Play(clip.name); // Assumes clip is on Animator's controller, or...
        }
        public void MoveToPosition(Vector3 position)
        {
            StopAllCoroutines();
            StartCoroutine(Move(position));
        }
        public void MoveBy(Vector3 position)
        {
            StopAllCoroutines();
            StartCoroutine(Move(position + transform.position));
        }
        private IEnumerator Move(Vector3 targetPosition)
        {
            PlayAnimation("Walk");

            float speed = 2f;
            float distanceThreshold = 0.05f;

            while (Vector3.Distance(transform.position, targetPosition) > distanceThreshold)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPosition;
            PlayAnimation("Idle");
        }

    }
}