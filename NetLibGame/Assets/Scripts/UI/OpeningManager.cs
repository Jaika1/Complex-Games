using NetworkingLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    PlayerInputActions input;
    public Animator OpeningAnimation;

    // Start is called before the first frame update
    void Start()
    {
        input = new PlayerInputActions();
        input.Enable();

        PlayerSettings.LoadConfig();
        NetworkingGlobal.LoadAllRoles();
        NetBase.DebugInfoReceived += s => Debug.Log(s);
        
        // We must shut down the server and/or client when the editor stops, since that doesn't happen automatically for us.
        Application.wantsToQuit += NetworkingGlobal.Application_quitting;

        StartCoroutine("AutoFadeRoutine");
    }

    // Update is called once per frame
    void Update()
    {
        if (input.MenuNavigation.Enter.ReadValue<float>() == 1.0f)
        {
            OpeningAnimation.SetTrigger("FadeOut");
        }
    }

    void ToMenu()
        => SceneManager.LoadScene("TitleScreen");

    private IEnumerator AutoFadeRoutine()
    {
        yield return new WaitForSeconds(6.0f);

        OpeningAnimation.SetTrigger("FadeOut");
    }
}
