using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] membersSlots;
    List<Demon> demons;

    public void Init()
    {
        membersSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Demon> demons)
    {
        this.demons = demons;
        for (int i = 0; i < membersSlots.Length; i++)
        {
            if (i < demons.Count)
                membersSlots[i].SetData(this.demons[i]);
            else
                membersSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Choose a demon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0;i < demons.Count;i++)
        {
            if (i == selectedMember)
            {
                membersSlots[i].SetSelected(true);
            } else
            {
                membersSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
