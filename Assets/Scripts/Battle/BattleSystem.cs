using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

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

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Demon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Demon.Base.Name} appeared.");

        PlayerAction();
    }

    private void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Demons);
        partyScreen.gameObject.SetActive(true);
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
                OpenPartyScreen();
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
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    private void HandleActionSelector()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0)
            {
                // Fight
                PlayerMove();
            } else if (currentAction == 1)
            {
                // Bag
            } else if (currentAction == 2)
            {
                // Demon
                OpenPartyScreen();
            } else if (currentAction == 3)
            {
                // Run
            }
        }
    }

    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Demon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Demon.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        } else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }
    }

    private void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Demons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var selectedMember = playerParty.Demons[currentMember];
            if (selectedMember.HP < 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted Demon");
                return;
            }

            if (selectedMember == playerUnit.Demon)
            {
                partyScreen.SetMessageText("You can't switch with the same Demon");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchDemon(selectedMember));
        } else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }

    IEnumerator SwitchDemon(Demon newDemon)
    {
        if (playerUnit.Demon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Demon.Base.Name}");
            yield return new WaitForSeconds(2);
        }

        playerUnit.Setup(newDemon);
        playerHud.SetData(newDemon);

        dialogBox.SetMoveNames(newDemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newDemon.Base.Name}!");

        StartCoroutine(PerformEnemyMove());
    }
}
