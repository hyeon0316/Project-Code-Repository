using UnityEngine;

[CreateAssetMenu(fileName = "Stat", menuName = "StatData/Stat")]
public class StatObj : ScriptableObject
{
    public int Id;
    public int Hp;
    public int Power;
    public float Speed;
}

[System.Serializable]
public struct Stat
{
    public int Id;
    public int Hp;
    public int Power;
    public float Speed;
}

[System.Serializable]
public class StatWrapper
{
    public Stat[] Stats;
}
