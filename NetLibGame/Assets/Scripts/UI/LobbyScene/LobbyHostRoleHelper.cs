using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyHostRoleHelper : MonoBehaviour
{
    public TMP_Text NameText;

    public void SetupTextAndEvent(string roleName, Action ev)
    {
        NameText.text = roleName;
    }
}
