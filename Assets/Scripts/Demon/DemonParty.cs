using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DemonParty : MonoBehaviour
{
    [SerializeField] List<Demon> demons;

    public List<Demon> Demons
    {
        get
        {
            return demons;
        }
    }

    private void Start()
    {
        foreach (Demon demon in demons)
        {
            demon.Init();
        }
    }

    public Demon GetHelthyDemon() { 
        
        return demons.Where(x => x.HP > 0).FirstOrDefault();
    }
}
