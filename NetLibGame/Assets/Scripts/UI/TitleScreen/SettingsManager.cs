using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public TMP_InputField PlayerNameField;
    public TMP_Text LoadedRolesText;
    public TMP_Dropdown WindowModeDropdown;

    // Start is called before the first frame update
    void Start()
    {
        PlayerNameField.text = PlayerSettings.Instance.PlayerName;
        PlayerNameField.onValueChanged.AddListener(PlayerNameChanged);

        string rolesTxt = "";
        for(int i = 0; i < NetworkingGlobal.LoadedRoleTypes.Count; ++i)
        {
            rolesTxt += $"[{NetworkingGlobal.LoadedRoleHashes[i]}] {NetworkingGlobal.LoadedRoleTypes.Keys.ElementAt(i)} ({NetworkingGlobal.LoadedRoleTypes.Values.ElementAt(i).AssemblyQualifiedName}){Environment.NewLine}";
        }
        LoadedRolesText.autoSizeTextContainer = true;
        LoadedRolesText.SetText(rolesTxt);

        WindowModeDropdown.value = (int)Screen.fullScreenMode;
        WindowModeDropdown.onValueChanged.AddListener(ChangeFullscreenMode);
    }

    private void PlayerNameChanged(string newName)
    {
        PlayerSettings.Instance.PlayerName = newName;
    }

    public void ChangeFullscreenMode(int mode)
    {
        Screen.fullScreenMode = (FullScreenMode)mode;
    }

    public void SaveSettings()
        => PlayerSettings.Instance.SaveConfig();
}
