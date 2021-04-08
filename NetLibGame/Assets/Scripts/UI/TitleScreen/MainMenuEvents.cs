using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuEvents : MonoBehaviour
{
    private void Start()
    {
        GetComponentInChildren<Button>().Select();
    }

    public void HostGame()
    {
        NetworkingGlobal.InitializeServerInstance();
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