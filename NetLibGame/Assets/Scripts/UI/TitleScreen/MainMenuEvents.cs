using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuEvents : MonoBehaviour
{
    public InputField IPInputField;
    public InputField PortInputField;

    private void Start()
    {
        GetComponentInChildren<Button>().Select();
    }

    public void HostGame()
    {
        NetworkingGlobal.FirstLobby = true;
        if (!NetworkingGlobal.InitializeServerInstance())
        {
            InvokerObj.Instance.ShowError("Failed to start the server! Please make sure you have a valid network adapter availiable to bind to, and that no other programs have bound to it! (Using port 7235 UDP)");
            return;
        }
        SceneManager.LoadScene("LobbyScene");
    }

    public void JoinGame()
    {
        // TODO: failure messages
        if (!IPAddress.TryParse(IPInputField.text, out NetworkingGlobal.ClientConnectIP))
        {
            try
            {
                NetworkingGlobal.ClientConnectIP = Dns.GetHostEntry(IPInputField.text).AddressList[0];
            }
            catch
            {
                InvokerObj.Instance.ShowError("Could not parse the given IP address! Please note that at this time, the program is only able to resolve raw IPv4 addresses.");
                return;
            }
        }

        if (!int.TryParse(PortInputField.text, out NetworkingGlobal.ClientConnectPort))
        {
            InvokerObj.Instance.ShowError("Could not parse the given port! A valid port should be a number within' the range of 0-65535.");
            return;
        }

        if (NetworkingGlobal.ClientConnectPort < 0 || NetworkingGlobal.ClientConnectPort > 65535)
        {
            InvokerObj.Instance.ShowError("Port out of range! A valid port should be a number within' the range of 0-65535.");
            return;
        }

        NetworkingGlobal.FirstLobby = true;
        SceneManager.LoadScene("LobbyScene");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}