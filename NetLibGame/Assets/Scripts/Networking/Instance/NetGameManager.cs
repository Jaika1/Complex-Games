using Jaika1.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetGameManager : MonoBehaviour
{
    [Header("UI Components")]
    public ScrollRect ChatScrollView;
    public TMP_Text ChatTextField;
    public TMP_InputField ChatBoxInputField;
    public Transform PlayerListContentTransform;
    public TMP_Text TimerText;

    [Header("UI Prefabs")]
    public GamePlayerButton GamePlayerPrefab;

    private List<GamePlayerButton> playerPanels = new List<GamePlayerButton>();

    // Start is called before the first frame update
    void Start()
    {
        ClientNetEvents.ChatMessageReceived.AddListener(WriteChatMessage);
        ClientNetEvents.UpdatePlayerList.AddListener(PlayerListUpdated);
        ClientNetEvents.UpdateTimer.AddListener(UpdateTimer);
        ClientNetEvents.FlipGameScene.AddListener(BackToLobby);

        ChatBoxInputField.onSubmit.AddListener(SendChatMessage);

        for (int i = 0; i < NetworkingGlobal.ConnectedPlayers.Count; ++i)
            PlayerListUpdated(NetworkingGlobal.ConnectedPlayers[i].PlayerID, false);
    }

    private void PlayerListUpdated(uint pid, bool dead)
    {
        if (dead)
        {
            GamePlayerButton pan = playerPanels.Find(p => p.PlayerID == pid);
            
            InvokerObj.Invoke(() => {
                pan.SetAlive(false);
                pan.SetSelectable(false);
            });
        }
        else
        {
          InvokerObj.Invoke(() => {
              GamePlayerButton newPlayer = Instantiate(GamePlayerPrefab, PlayerListContentTransform);
              newPlayer.SetupText(NetworkingGlobal.ConnectedPlayers.Find(p => p.PlayerID == pid), NetworkingGlobal.LocalPlayer.PlayerID == pid);
              playerPanels.Add(newPlayer);
          });
        }
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

    public void SendChatMessage(string submission)
    {
        if (!string.IsNullOrWhiteSpace(submission))
            NetworkingGlobal.ClientInstance.SendF(5, PacketFlags.Reliable, submission); // BroadcastChatMessage(string);

        ChatBoxInputField.text = string.Empty;
        ChatBoxInputField.Select();
        ChatBoxInputField.ActivateInputField();
    }

    public void UpdateTimer(int timeSeconds)
    {
        InvokerObj.Invoke(() => {
            TimerText.SetText(TimeSpan.FromSeconds(timeSeconds).ToString(@"m\:ss"));
        });
    }

    private void BackToLobby()
    {
        ClientNetEvents.ChatMessageReceived.RemoveListener(WriteChatMessage);
        ClientNetEvents.UpdatePlayerList.RemoveListener(PlayerListUpdated);
        ClientNetEvents.UpdateTimer.RemoveListener(UpdateTimer);
        ClientNetEvents.FlipGameScene.RemoveListener(BackToLobby);

        InvokerObj.Invoke(() =>
        {
            SceneManager.LoadScene("LobbyScene");
        });
    }
}
