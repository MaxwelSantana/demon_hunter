using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState
{
    Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, AboutToUse, BattleOver
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUsechoice = true;

    DemonParty playerParty;
    DemonParty trainerParty;
    Demon wildDemon;

    bool isTrainerBattle = false;

    PlayerController player;
    TrainerController trainer;

    public void StartBattle(DemonParty playerParty, Demon wildDemon)
    {
        this.playerParty = playerParty;
        this.wildDemon = wildDemon;
        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(DemonParty playerParty, DemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            // Wild Pokemon Battle
            playerUnit.Setup(playerParty.GetHelthyDemon());
            enemyUnit.Setup(wildDemon);

            dialogBox.SetMoveNames(playerUnit.Demon.Moves);

            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Demon.Base.Name} appeared.");
        } else
        {
            // Trainer Battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle.");

            // send out the first demon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            var enemyDemon = trainerParty.GetHelthyDemon();
            enemyUnit.Setup(enemyDemon);

            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyDemon.Base.Name}.");

            // send out the first demon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            var playerDemon = playerParty.GetHelthyDemon();
            playerUnit.Setup(playerDemon);

            yield return dialogBox.TypeDialog($"Go {trainer.Name} {playerDemon.Base.Name}.");

            dialogBox.SetMoveNames(playerUnit.Demon.Moves);
        }


        partyScreen.Init();
        ActionSelection();
    }

    private void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        OnBattleOver(won);
    }

    private void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Demons);
        partyScreen.gameObject.SetActive(true);
    }

    private void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Demon demon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {demon.Base.Name}. Do you want to change pokemon?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    private IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        var move = playerUnit.Demon.Moves[currentMove];

        yield return RunMove(playerUnit, enemyUnit, move);

        // If the battle stat was not changed by RunMove, then go to next step.
        if (state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());
    }

    private IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Demon.GetRandomMove();

        yield return RunMove(enemyUnit, playerUnit, move);

        // If the battle stat was not changed by RunMove, then go to next step.
        if (state == BattleState.PerformMove)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Demon.Base.Name} used {move.Base.Name}");

        var damageDetails = targetUnit.Demon.TakeDamage(move, sourceUnit.Demon);
        yield return targetUnit.Hud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Demon.Base.Name} Fainted");

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextDemon = playerParty.GetHelthyDemon();
            if (nextDemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        } else
        {
            if(!isTrainerBattle)
            {
                BattleOver(true);
            }else
            {
                var nextDemon= trainerParty.GetHelthyDemon();
                if (nextDemon != null)
                {
                    StartCoroutine(AboutToUse(nextDemon));
                } else
                {
                    BattleOver(true);
                }
            }

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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelector();
        } else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        } else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
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
                MoveSelection();
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
            StartCoroutine(PlayerMove());
        } else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
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
            if (selectedMember.HP <= 0)
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
        } else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerUnit.Demon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a demon to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerDemon());
            } else
            {
                ActionSelection();
            }

        }
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUsechoice = !aboutToUsechoice;
        }

        dialogBox.UpdateChoicebox(aboutToUsechoice);

        if(Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUsechoice == true)
            {
                prevState = BattleState.AboutToUse;
                // yes option
                OpenPartyScreen();
            } else
            {
                // no option
                StartCoroutine(SendNextTrainerDemon());
            }
        } else if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerDemon());
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

        dialogBox.SetMoveNames(newDemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newDemon.Base.Name}!");

        StartCoroutine(EnemyMove());
    }

    IEnumerator SendNextTrainerDemon()
    {
        state = BattleState.Busy;

        var nextDemon = trainerParty.GetHelthyDemon();

        enemyUnit.Setup(nextDemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextDemon.Base.Name}.");

        StartCoroutine(EnemyMove());
    }
}
