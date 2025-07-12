using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character")]
public class Character : ScriptableObject
{
    public string CharacterID;
    public string CharacterName;
    public Sprite Portrait; 

    public List<CharacterAnimation> animations;

    public AnimationClip GetAnimationByName(string name)
    {
        return animations.Find(entry => entry.animationName == name)?.animationClip;
    }

    public List<string> GetAnimationNames()
    {
        List<string> names = new();
        foreach (var entry in animations)
        {
            names.Add(entry.animationName);
        }
        return names;
    }

}
[System.Serializable]
public class CharacterAnimation
{
    public string animationName;
    public AnimationClip animationClip;
}