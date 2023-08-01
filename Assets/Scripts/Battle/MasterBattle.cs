using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MasterBattle : MonoBehaviour
{
    [SerializeField] Transform fixedMedalPos;
    [SerializeField] Text messageText;
    [SerializeField] Image medal;

    private void Awake()
    {
        gameObject.SetActive(false);
    }
    public IEnumerator Show()
    {
        gameObject.SetActive(true);

        yield return new WaitForSeconds(2);

        messageText.gameObject.SetActive(false);

        yield return gameObject.transform.DOMove(fixedMedalPos.position, 1); ;

        yield return gameObject.transform.DOScale(new Vector3(0.5f, 0.5f, 0f), 1);

        yield return medal.transform.DOLocalMove(new Vector3(0, 0, 0), 1);
    }
}
