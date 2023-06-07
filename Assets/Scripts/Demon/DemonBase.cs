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
    Normal,
    Fire,
    Water,
    Grass
}