using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyPlayerPanelHelper : MonoBehaviour
{
    public TMP_Text PIDText;
    public TMP_Text NameText;

    [HideInInspector]
    public uint PlayerID;

    public void SetupText(NetWerewolfPlayer player, bool localPlayer)
    {
        PlayerID = player.PlayerID;
        PIDText.text = $"[{PlayerID}]";
        NameText.text = player.Name;
        if (localPlayer)
            PIDText.color = Color.yellow;
    }
}
