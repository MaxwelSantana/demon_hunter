using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelext;
    [SerializeField] HPBar hpBar;

    public void SetData(Demon demon)
    {
        nameText.text = demon.Base.Name;
        levelext.text = "Lvl " + demon.Level;
        hpBar.SetHP((float)demon.HP / demon.MaxHp);
    }
}
