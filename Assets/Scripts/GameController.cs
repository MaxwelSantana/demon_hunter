using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] int minBattlesToFightMaster = 3;
    [SerializeField] GameObject trainerBattlesWon;
    [SerializeField] Text trainerBattlesWonText;
    [SerializeField] int masterTrainerSceneToload = -1;
    [SerializeField] MasterBattle masterBattlePanel;
    [SerializeField] AudioClip homeSceneMusic;
    [SerializeField] AudioClip masterHunterSceneMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip masterTrainerBattleMusic;

    GameState state;
    int battlesWon = 2;

    public static GameController Instance { get; private set; }

    Fader fader;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
        
        playerController.OnEnterTrainersView += (Collider2D trainnerCollider) => 
        {
            var trainner = trainnerCollider.GetComponentInParent<TrainerController>();
            if (trainner != null)
            {
                state = GameState.Cutscene;
                StartCoroutine(trainner.TriggerTrainerBattle(playerController));
            }
        };
        
        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
        };

        fader = FindObjectOfType<Fader>();
        UpdateBattlesWon();
        AudioManager.i.PlayMusic(homeSceneMusic);
    }

    private void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<DemonParty>();
        var wildDemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildDemon();

        battleSystem.StartBattle(playerParty, wildDemon);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainerController)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        trainerBattlesWon.gameObject.SetActive(false);

        this.trainer = trainerController;
        var playerParty = playerController.GetComponent<DemonParty>();
        var trainerParty = trainerController.GetComponent<DemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
        if (this.trainer.IsMasterHunter)
        {
            AudioManager.i.PlayMusic(masterTrainerBattleMusic);
        } else
        {
            AudioManager.i.PlayMusic(trainerBattleMusic);
        }
    }

    private void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            
            trainer.BattleLost();
            if (trainer.IsMasterHunter)
            {
                StartCoroutine(ReceiveMedal());
            } else
            {
                battlesWon++;
                UpdateBattlesWon();
            }
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        trainerBattlesWon.gameObject.SetActive(true);
        AudioManager.i.PlayMusic(homeSceneMusic);
    }

    void UpdateBattlesWon()
    {
        if (battlesWon <= minBattlesToFightMaster)
        {
            trainerBattlesWonText.text = $"Battles: {battlesWon}/{minBattlesToFightMaster}";
        }

        if (battlesWon == minBattlesToFightMaster)
        {
            trainerBattlesWon.gameObject.SetActive(false);
            StartCoroutine(SwitchMasterTrainerScene());
            return;
        }
    }

    IEnumerator ReceiveMedal()
    {
        yield return fader.FadeIn(0.5f);
        yield return fader.FadeOut(0.5f);
        yield return masterBattlePanel.Show();
    }

    IEnumerator SwitchMasterTrainerScene()
    {
        yield return fader.FadeIn(0.5f);
        yield return SceneManager.LoadSceneAsync(masterTrainerSceneToload);
        trainerBattlesWon.gameObject.SetActive(false);

        var destPortal = FindObjectOfType<Portal>();

        playerController.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        
        yield return fader.FadeOut(0.5f);

        AudioManager.i.PlayMusic(masterHunterSceneMusic);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        } else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        } else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }
}
