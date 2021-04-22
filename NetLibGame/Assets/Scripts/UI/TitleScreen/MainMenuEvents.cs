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
        NetworkingGlobal.InitializeServerInstance();
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
                return;
            }
        }

        if (!int.TryParse(PortInputField.text, out NetworkingGlobal.ClientConnectPort))
            return;

        if (NetworkingGlobal.ClientConnectPort < 0 || NetworkingGlobal.ClientConnectPort > 65535)
            return;

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