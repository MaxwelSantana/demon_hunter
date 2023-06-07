using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demon
{
    public DemonBase Base { get; set; }
    public int Level { get; set; }

    public int HP { get; private set; }

    public List<Move> Moves {  get; private set; } 

    public Demon(DemonBase demonBase, int demonLevel)
    {
        Base = demonBase;
        Level = demonLevel;
        HP = MaxHp;

        Moves = new List<Move>();

        foreach (var learnableMove in Base.LearnableMoves)
        {
            if (learnableMove.Level <= Level)
            {
                Moves.Add(new Move(learnableMove.Base));
            }

            if (Moves.Count > 4)
            {
                break;
            }
        }
    }

    public int Attack
    {
        get { return formulaLevelStat(Base.Attack, Level); }
    }

    public int Defense
    {
        get { return formulaLevelStat(Base.Defense, Level); }
    }

    public int SpAttack
    {
        get { return formulaLevelStat(Base.SpAttack, Level); }
    }

    public int SpDefense
    {
        get { return formulaLevelStat(Base.SpDefense, Level); }
    }

    public int Speed
    {
        get { return formulaLevelStat(Base.Speed, Level); }
    }

    public int MaxHp
    {
        get { return formulaLevelStat(Base.MaxHP, Level) + 5; }
    }

    private int formulaLevelStat(int stat, int level)
    {
        return Mathf.FloorToInt((stat * level) / 100f) + 5;
    }
}
