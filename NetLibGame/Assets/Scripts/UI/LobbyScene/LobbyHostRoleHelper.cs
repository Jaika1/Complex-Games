using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHostRoleHelper : MonoBehaviour
{
    public TMP_Text NameText;

    [HideInInspector]
    public string RoleHash;

    private Button button;

    public void SetupTextAndEvent(string roleHash, bool eventRemove)
    {
        button = GetComponent<Button>();
        RoleHash = roleHash;
        NameText.text = NetworkingGlobal.GetRoleNameFromHash(roleHash);
        button?.onClick.AddListener(() => SendEditRoleList(roleHash, eventRemove));
    }

    public static void SendEditRoleList(string roleHash, bool remove)
    {
        NetworkingGlobal.ClientInstance.Send(190, roleHash, remove); // ModifyActiveRoleList(string, bool);
    }
}
