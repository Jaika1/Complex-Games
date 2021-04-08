using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (NetworkingGlobal.FirstLobby)
        {
            NetworkingGlobal.FirstLobby = false;
            NetworkingGlobal.InitializeClientInstance(IPAddress.Loopback, 7235);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
