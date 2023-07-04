using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Demon> wildDemons;

    public Demon GetRandomWildDemon()
    {
        var wildDemon= wildDemons[Random.Range(0, wildDemons.Count)];
        wildDemon.Init();
        return wildDemon;
    }
}
