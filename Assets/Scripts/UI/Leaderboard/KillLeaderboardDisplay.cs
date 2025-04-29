using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class KillLeaderboardDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayTextKills;
    [SerializeField] private Color myColor;

    private FixedString32Bytes playerName;

    public ulong ClientId {get; private set;}
    public int TotalKills {get; private set;}

    public void Initialise(ulong clientId, FixedString32Bytes playerName, int totalKills)
    {
        this.playerName = playerName;
        ClientId = clientId;


        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            displayTextKills.color = myColor;
        }

        UpdateKills(totalKills);

    }

    public void UpdateKills(int totalKills)
    {
        TotalKills = totalKills;
        UpdateText();
    }

    public void UpdateText()
    {
        displayTextKills.text = $"{transform.GetSiblingIndex() + 1}. {playerName} -> {TotalKills} Kills";
    }
}
