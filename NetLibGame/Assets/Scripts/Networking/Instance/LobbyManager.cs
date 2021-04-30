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
    public RectTransform HostRolesListContent;
    public RectTransform ClientRolesListContent;

    [Header("UI Prefabs")]
    public LobbyPlayerPanelHelper LobbyPlayerPrefab;
    public LobbyHostRoleHelper HostRoleItemPrefab;
    public LobbyHostRoleHelper ClientRoleItemPrefab;

    private List<LobbyPlayerPanelHelper> playerPanels = new List<LobbyPlayerPanelHelper>();
    private List<LobbyHostRoleHelper> hostRolePanels = new List<LobbyHostRoleHelper>();
    private List<LobbyHostRoleHelper> clientRolePanels = new List<LobbyHostRoleHelper>();

    // Start is called before the first frame update
    void Start()
    {
        ClientNetEvents.NetPlayerConnected.AddListener(AddPlayerToList);
        ClientNetEvents.NetPlayerDisconnected.AddListener(RemovePlayerFromList);
        ClientNetEvents.ChatMessageReceived.AddListener(WriteChatMessage);
        ClientNetEvents.UpdateHostBox.AddListener(UpdateHostBox);
        ClientNetEvents.UpdateActiveRoleList.AddListener(UpdateActiveRoleList);

        if (NetworkingGlobal.FirstLobby)
        {
            NetworkingGlobal.FirstLobby = false;

            if (NetworkingGlobal.ServerInstance != null)
                NetworkingGlobal.InitializeClientInstance(IPAddress.Loopback, 7235);
            else
                NetworkingGlobal.InitializeClientInstance(NetworkingGlobal.ClientConnectIP, NetworkingGlobal.ClientConnectPort);
        }

        foreach(string rHash in NetworkingGlobal.LoadedRoleHashes)
        {
            LobbyHostRoleHelper h = Instantiate(HostRoleItemPrefab, LoadedRolesListContent);
            h.SetupTextAndEvent(rHash, false);
        }

        ChatTextField.autoSizeTextContainer = true;

        UpdateHostBox(NetworkingGlobal.LocalPlayer.IsHost);
    }

    private void FixedUpdate()
    {
        Debug.Log(ChatScrollView.verticalNormalizedPosition);
    }

    private void UpdateActiveRoleList(string roleHash, bool remove)
    {
        // TODO: non-host role list

        if (remove)
        {
            int activeIndex = hostRolePanels.FindIndex(x => x.RoleHash == roleHash);

            if (activeIndex == -1)
                return;

            InvokerObj.Invoke(() =>
            {
                Destroy(hostRolePanels[activeIndex].gameObject);
                hostRolePanels.RemoveAt(activeIndex);

                Destroy(clientRolePanels[activeIndex].gameObject);
                clientRolePanels.RemoveAt(activeIndex);
            });
        }
        else
        {
            InvokerObj.Invoke(() =>
            {
                LobbyHostRoleHelper rp = Instantiate(HostRoleItemPrefab, HostRolesListContent);
                rp.SetupTextAndEvent(roleHash, true);
                hostRolePanels.Add(rp);

                LobbyHostRoleHelper rpc = Instantiate(ClientRoleItemPrefab, ClientRolesListContent);
                rpc.SetupTextAndEvent(roleHash, false);
                clientRolePanels.Add(rpc);
            });
        }
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
            float oldPos = ChatScrollView.normalizedPosition.y;

            if (player != null)
                ChatTextField.text += $"[{player.PlayerID}] {player.Name}: {message}{Environment.NewLine}";
            else
                ChatTextField.text += $"SYSTEM: {message}{Environment.NewLine}";
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ChatScrollView.transform);

            if (oldPos <= 0.0001f)
                ChatScrollView.normalizedPosition = Vector2.zero;
        });
    }

    public void SendChatMessage(TMP_InputField field)
    {
        if (!string.IsNullOrWhiteSpace(field.text))
            NetworkingGlobal.ClientInstance.Send(5, field.text); // BroadcastChatMessage(string);

        field.text = string.Empty;
    }

    public void SendTryStartGame()
    {
        NetworkingGlobal.ClientInstance.Send(191);
    }
}
