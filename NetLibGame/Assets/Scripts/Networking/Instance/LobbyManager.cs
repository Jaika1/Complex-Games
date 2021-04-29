using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Components")]
    public Transform PlayerListContent;
    public ScrollRect ChatScrollView;
    public TMP_Text ChatTextField;
    public RectTransform HostRoleListPanel;
    public RectTransform ClientRoleListPanel;
    public RectTransform LoadedRolesListContent;

    [Header("UI Prefabs")]
    public LobbyPlayerPanelHelper LobbyPlayerPrefab;
    public LobbyHostRoleHelper HostRoleItemPrefab;

    private List<LobbyPlayerPanelHelper> playerPanels = new List<LobbyPlayerPanelHelper>();

    // Start is called before the first frame update
    void Start()
    {
        ClientNetEvents.NetPlayerConnected.AddListener(AddPlayerToList);
        ClientNetEvents.NetPlayerDisconnected.AddListener(RemovePlayerFromList);
        ClientNetEvents.ChatMessageReceived.AddListener(WriteChatMessage);
        ClientNetEvents.UpdateHostBox.AddListener(UpdateHostBox);

        if (NetworkingGlobal.FirstLobby)
        {
            NetworkingGlobal.FirstLobby = false;

            if (NetworkingGlobal.ServerInstance != null)
                NetworkingGlobal.InitializeClientInstance(IPAddress.Loopback, 7235);
            else
                NetworkingGlobal.InitializeClientInstance(NetworkingGlobal.ClientConnectIP, NetworkingGlobal.ClientConnectPort);
        }

        foreach(string rName in NetworkingGlobal.LoadedRoleTypes.Keys)
        {
            LobbyHostRoleHelper h = Instantiate(HostRoleItemPrefab, LoadedRolesListContent);
            h.SetupTextAndEvent(rName, () => { });
        }

        UpdateHostBox(NetworkingGlobal.LocalPlayer.IsHost);
    }

    private void UpdateHostBox(bool isHost)
    {
        InvokerObj.Invoke(() =>
        {
            HostRoleListPanel.gameObject.SetActive(isHost);
            ClientRoleListPanel.gameObject.SetActive(!isHost);
        });
    }

    public void AddPlayerToList(NetWerewolfPlayer player, bool localPlayer)
    {
        InvokerObj.Invoke(() => {
            LobbyPlayerPanelHelper newPlayer = Instantiate(LobbyPlayerPrefab, PlayerListContent);
            newPlayer.SetupText(player, localPlayer);
            playerPanels.Add(newPlayer);
        });
    }

    public void RemovePlayerFromList(uint pid)
    {
        LobbyPlayerPanelHelper pan = playerPanels.Find(p => p.PlayerID == pid);
        playerPanels.Remove(pan);

        InvokerObj.Invoke(() => {
            Destroy(pan.gameObject);
        });
    }

    public void WriteChatMessage(NetWerewolfPlayer player, string message)
    {
        InvokerObj.Invoke(() =>
        {
            //bool autoScrollChat = ChatScrollView.normalizedPosition == Vector2.zero;
            ChatTextField.text += $"[{player.PlayerID}] {player.Name}: {message}{Environment.NewLine}";
            //if (autoScrollChat)
                ChatScrollView.normalizedPosition = Vector2.zero;
        });
    }

    public void SendChatMessage(TMP_InputField field)
    {
        if (!string.IsNullOrWhiteSpace(field.text))
            NetworkingGlobal.ClientInstance.Send(5, field.text); // BroadcastChatMessage(string);

        field.text = string.Empty;
    }
}
