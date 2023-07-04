using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start, PlayerAction, PlayerMove, EnemyMove, Busy
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    DemonParty playerParty;
    Demon wildDemon;

    public void StartBattle(DemonParty playerParty, Demon wildDemon)
    {
        this.playerParty = playerParty;
        this.wildDemon = wildDemon;
        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHelthyDemon());
        enemyUnit.Setup(wildDemon);
        playerHud.SetData(playerUnit.Demon);
        enemyHud.SetData(enemyUnit.Demon);

        dialogBox.SetMoveNames(playerUnit.Demon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Demon.Base.Name} appeared.");

        PlayerAction();
    }

    private void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
    }

    private void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    private IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Demon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Demon.Base.Name} used {move.Base.Name}");
        
        var damageDetails = enemyUnit.Demon.TakeDamage(move, playerUnit.Demon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Demon.Base.Name} Fainted");

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        } else
        {
            StartCoroutine(PerformEnemyMove());
        }
    }

    private IEnumerator PerformEnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Demon.GetRandomMove();

        yield return dialogBox.TypeDialog($"{enemyUnit.Demon.Base.Name} used {move.Base.Name}");

        var damageDetails = playerUnit.Demon.TakeDamage(move, enemyUnit.Demon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Demon.Base.Name} Fainted");

            yield return new WaitForSeconds(2f);

            var nextDemon = playerParty.GetHelthyDemon();
            if (nextDemon != null) {
                playerUnit.Setup(nextDemon);
                playerHud.SetData(nextDemon);

                dialogBox.SetMoveNames(nextDemon.Moves);

                yield return dialogBox.TypeDialog($"Go {nextDemon.Base.Name}!");

                PlayerAction();
            } else
            {
                OnBattleOver(false);
            }
        }
        else
        {
            PlayerAction();
        }
    }

    private IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective...");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelector();
        } else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    private void HandleActionSelector()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0)
            {
                PlayerMove();
            } else if (currentAction == 1)
            {

            }
        }
    }

    private void HandleMoveSelection()
    {
        int movesCount = playerUnit.Demon.Moves.Count;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < movesCount - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && movesCount > 2) {
            if (currentMove < movesCount)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && movesCount > 2) {
            if(currentMove > 1)
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Demon.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
