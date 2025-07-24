using System;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public abstract class DialogueCondition
    {
        public string Key; // what to check
    }

    [Serializable]
    public class StringCondition : DialogueCondition
    {
        public StringConditionType ConditionType;
        public string Value;
    }

    [Serializable]
    public class BoolCondition : DialogueCondition
    {
        public bool ExpectedValue;
    }

    [Serializable]
    public class IntCondition : DialogueCondition
    {
        public ComparisonType Comparison;
        public ActionType Action;
        public int Value;
    }

    [Serializable]
    public class RandomCondition : DialogueCondition
    {
        public int Value;
    }

    [Serializable]
    public class CharacterCondition
    {
        public CharacterAttribute Attribute;
        public CharacterTarget Target;
        public int ComparisonValue;
        public CharacterAction Action;
    }

    public enum ComparisonType
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual
    }
    public enum ActionType
    {
        None,
        Decrease,
        Increase
    }
    public enum CharacterAttribute
    {
        Relations,
        IsInScene,
        IsAlive,
        Morale,
        Stamina
    }
    public enum CharacterTarget
    {
        Player,
        Doc,
        Gravehound,
        Rook,
        Vale,
        Ash
    }
    public enum CharacterAction
    {
        None,
        SpawnOnRight,
        SpawnOnLeft
    }
    public enum StringConditionType
    {
        Default,
        Memory
    }
}