using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoleSplash : MonoBehaviour
{
    public TMP_Text RoleNameText;
    public TMP_Text RoleDescText;
    public bool AutoSizeDescription = false;

    // Start is called before the first frame update
    void Start()
    {
        RoleNameText.text = NetworkingGlobal.LocalPlayer.Role.Name;
        RoleNameText.color = NetworkingGlobal.LocalPlayer.Role.Alignment.GroupColour.ToUnityColor();

        RoleDescText.autoSizeTextContainer = AutoSizeDescription;
        RoleDescText.SetText(NetworkingGlobal.LocalPlayer.Role.Description);
    }

    public void DestroySelf() => Destroy(gameObject);
}
