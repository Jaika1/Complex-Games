using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashCanvasScript : MonoBehaviour
{
    public GameObject EnterSplashRef;

    public void DisableAndAllowEnter()
    {
        EnterSplashRef.SetActive(true);
        gameObject.SetActive(false);
    }
}
