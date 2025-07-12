using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Companion")]
public class Companion : Character
{
    public string RealName;
    public int Relations_Player;
    public int Relations_Doc;
    public int Relations_Grave;
    public int Relations_Rook;
    public int Relations_Vale;
    public int Relations_Ash;

    public int Morale;
    public int Stamina;
}