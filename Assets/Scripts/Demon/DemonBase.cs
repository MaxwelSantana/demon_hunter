using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Demon", menuName = "Demon/Create new demon")]
public class DemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] DemonType demonType;

    // Base stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMoves> learnableMoves;

    public string Name { get { return name; } }
    public string Description { get { return description; } }

    public Sprite FrontSprite { get { return frontSprite; } }
    public Sprite BackSprite { get { return backSprite; } }

    public DemonType Type { get { return demonType; } }

    public int MaxHP { get { return maxHp; } }
    public int Attack { get { return attack; } }
    public int Defense { get {  return defense; } }
    public int SpAttack { get {  return spAttack; } }
    public int SpDefense { get { return spDefense; } }
    public int Speed { get { return speed; } }

    public List<LearnableMoves> LearnableMoves { get { return learnableMoves; } }

}

[Serializable]
public class LearnableMoves 
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base { get { return moveBase; } }
    public int Level { get { return level; } }

}

public enum DemonType
{
    None,
    Normal,
    Fire,
    Water,
    Eletric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

public class TypeChart
{
    static float[][] chart =
    {
        //                    NOR   FIR   WAT   ELE   GRA   ICE   FIG   POI
        /*NOR*/ new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*FIR*/ new float[] { 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f },
        /*WAT*/ new float[] { 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*ELE*/ new float[] { 1.0f, 2.0f, 2.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f },
        /*GRA*/ new float[] { 1.0f, 0.5f, 2.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f },
        /*ICE*/ new float[] { 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*FIG*/ new float[] { 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*POS*/ new float[] { 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f }
    };

    public static float GetEffectiveness(DemonType attackType, DemonType defenseType)
    {
        if (attackType == DemonType.None || defenseType == DemonType.None) return 1.0f;

        int row = (int)attackType - 1;
        int column = (int)defenseType - 1;

        return chart[row][column];
    }
}