using System;

namespace Subtegral.DialogueSystem.DataContainers
{

    [Serializable]
    public class ChangeCharacterAttribute
    {
        public TargetAttribute Attribute;
        public CharacterTarget Target;
        public int Value;
    }
    public enum TargetAttribute
    {
        Relations,
        IsAlive,
        Morale,
        Stamina
    }
}