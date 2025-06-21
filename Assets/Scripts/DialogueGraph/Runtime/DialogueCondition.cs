using System;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public abstract class DialogueCondition
    {
        public string Key; // what ot check
    }

    [Serializable]
    public class StringCondition : DialogueCondition
    {
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
}