using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud { get { return hud; } }

    public Demon Demon { get; private set; }

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Setup(Demon demon)
    {
        Demon = demon;

        if (isPlayerUnit)
        {
            image.sprite = Demon.Base.BackSprite;
        } else
        {
            image.sprite = Demon.Base.FrontSprite;
        }

        hud.gameObject.SetActive(true);
        hud.SetData(demon);
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }
}
