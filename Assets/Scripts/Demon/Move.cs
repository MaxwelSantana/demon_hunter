using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase Base { get; private set; }

    public int PP { get; private set; }

    public Move(MoveBase moveBase)
    {
        Base = moveBase;
        PP = moveBase.PP;
    }
}
