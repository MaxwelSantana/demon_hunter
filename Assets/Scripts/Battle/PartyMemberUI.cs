using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelext;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highlightedColor;

    Demon _demon;

    public void SetData(Demon demon)
    {
        _demon = demon;

        nameText.text = demon.Base.Name;
        levelext.text = "Lvl " + demon.Level;
        hpBar.SetHP((float)demon.HP / demon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = highlightedColor;
        } else
        {
            nameText.color = Color.black;
        }
    }
}
