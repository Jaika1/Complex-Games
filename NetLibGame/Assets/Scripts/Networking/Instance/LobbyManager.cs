using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Components")]
    public Transform PlayerListContent;
    public TMP_Text ChatTextField;

    [Header("UI Prefabs")]
    public LobbyPlayerPanelHelper LobbyPlayerPrefab;

    private List<LobbyPlayerPanelHelper> playerPanels = new List<LobbyPlayerPanelHelper>();

    // Start is called before the first frame update
    void Start()
    {
        ClientNetEvents.NetPlayerConnected.AddListener(AddPlayerToList);
        ClientNetEvents.NetPlayerDisconnected.AddListener(RemovePlayerFromList);

        if (NetworkingGlobal.FirstLobby)
        {
            NetworkingGlobal.FirstLobby = false;

            if (NetworkingGlobal.ServerInstance != null)
                NetworkingGlobal.InitializeClientInstance(IPAddress.Loopback, 7235);
            else
                NetworkingGlobal.InitializeClientInstance(NetworkingGlobal.ClientConnectIP, NetworkingGlobal.ClientConnectPort);
        }
    }

    public void AddPlayerToList(NetWerewolfPlayer player, bool localPlayer)
    {
        InvokerObj.Invoke(() => {
            LobbyPlayerPanelHelper newPlayer = Instantiate(LobbyPlayerPrefab, PlayerListContent);
            newPlayer.SetupText(player, localPlayer);
            playerPanels.Add(newPlayer);
        });
    }

    public void RemovePlayerFromList(NetWerewolfPlayer player)
    {
        LobbyPlayerPanelHelper pan = playerPanels.Find(p => p.PlayerID == player.PlayerID);
        playerPanels.Remove(pan);

        InvokerObj.Invoke(() => {
            Destroy(pan);
        });
    }

    public void WriteNetMessage(NetWerewolfPlayer player, string message)
    {
        ChatTextField.text += $"{player.Name}: {message}{Environment.NewLine}";
    }
}
