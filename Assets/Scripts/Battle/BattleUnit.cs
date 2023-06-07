using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] DemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Demon Demon { get; private set; }

    public void Setup()
    {
        Demon = new Demon(_base, level);

        if (isPlayerUnit)
        {
            GetComponent<Image>().sprite = Demon.Base.BackSprite;
        } else
        {
            GetComponent<Image>().sprite = Demon.Base.FrontSprite;
        }
    }
}
