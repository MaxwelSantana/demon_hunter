using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCSate state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)
    {
        Debug.Log("passou: state: " + state);

        if (state == NPCSate.Idle || state == NPCSate.Walking)
        {
            state = NPCSate.Dialog;

            character.LookTowards(initiator.position);
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                idleTimer = 0;
                state = NPCSate.Idle;
            }));
        }
    }

    private void Update()
    {
        if (state == NPCSate.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0 )
                {
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCSate.Walking;

        var oldPosition = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if(transform.position != oldPosition)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }

        state = NPCSate.Idle;
    }
}

public enum NPCSate { Idle, Walking, Dialog }