using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;

    public Transform SpawnPoint => spawnPoint;
}
