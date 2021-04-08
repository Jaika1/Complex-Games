using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnterToMainMenu : MonoBehaviour
{
    public GameObject MainMenuCanvas;
    public GameObject ParentCanvas;

    private PlayerInputActions pia;


    private void Awake()
    {
        pia = new PlayerInputActions();
        pia.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (pia.MenuNavigation.Enter.triggered)
        {
            MainMenuCanvas.SetActive(true);
            ParentCanvas.SetActive(false);
        }
    }
}
