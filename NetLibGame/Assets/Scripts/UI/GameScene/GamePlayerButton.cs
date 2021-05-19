using Jaika1.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WerewolfDataLib;

public class GamePlayerButton : MonoBehaviour
{
    public TMP_Text PlayerNameText;
    public Image PlayerStatusImage;
    public Button PlayerSelectorButton;

    [HideInInspector]
    public uint PlayerID;
    [HideInInspector]
    public WerewolfPlayer Player;

    public void SetupText(NetWerewolfPlayer player, bool localPlayer)
    {
        PlayerID = player.PlayerID;
        Player = player;
        PlayerNameText.text = $"[{PlayerID}] {player.Name}";
        if (localPlayer)
            PlayerNameText.color = Color.yellow;
    }

    public void SetAlive(bool alive) => 
        PlayerStatusImage.color = alive ? new Color(0.0f, 1.0f, 0.3826256f) : Color.grey;

    public void SetSelectable(bool canSelect) =>
        PlayerSelectorButton.interactable = canSelect;

    public void PerformActionAgainst() =>
        NetworkingGlobal.ClientInstance.SendF(20, PacketFlags.Reliable, PlayerID);
}
