using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.Arm;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelext;
    [SerializeField] HPBar hpBar;

    Demon _demon;

    public void SetData(Demon demon)
    {
        _demon = demon;

        nameText.text = demon.Base.Name;
        levelext.text = "Lvl " + demon.Level;
        hpBar.SetHP((float)demon.HP / demon.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)_demon.HP / _demon.MaxHp);
    }
}
