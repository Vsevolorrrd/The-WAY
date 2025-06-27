[System.Serializable]
public class CompanionInstance
{
    public Companion CharacterData;

    public int Relations_Player;
    public int Relations_Doc;
    public int Relations_Grave;
    public int Relations_Rook;
    public int Relations_Vale;
    public int Relations_Ash;

    public int Morale;
    public int Stamina;

    public bool IsDead;

    public CompanionInstance(Companion character)
    {
        CharacterData = character;

        Morale = 100;
        Stamina = 100;
    }
}