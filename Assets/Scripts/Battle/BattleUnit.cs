using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;

    public Demon Demon { get; private set; }

    public void Setup(Demon demon)
    {
        Demon = demon;

        if (isPlayerUnit)
        {
            GetComponent<Image>().sprite = Demon.Base.BackSprite;
        } else
        {
            GetComponent<Image>().sprite = Demon.Base.FrontSprite;
        }
    }
}
