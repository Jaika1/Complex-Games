using Jaika1.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InvokerObj : MonoBehaviour
{
    public static List<Invokee> ToInvoke = new List<Invokee>();
    private static InvokerObj instance;

    [Header("Debug UI")]
    public GameObject DebugCanvasRef;
    public Text DebugTextRef;

    [Header("Error UI")]
    public GameObject ErrorCanvasRef;
    public TMP_Text ErrorReasonText;

    public static InvokerObj Instance => instance;


    private void Start()
    {
        if (FindObjectsOfType(typeof(InvokerObj)).Length > 1)
            Destroy(this);
        else
        {
            DontDestroyOnLoad(this);
            instance = this;
#if !UNITY_EDITOR
            Destroy(DebugCanvasRef);
#else
            NetBase.DebugInfoReceived += NetBase_DebugInfoReceived;
#endif
            Application.wantsToQuit += Application_quitting;
        }

        ErrorReasonText.autoSizeTextContainer = true;
        ErrorCanvasRef.SetActive(false);
    }

    private bool Application_quitting()
    {
        NetBase.DebugInfoReceived -= NetBase_DebugInfoReceived;
        return true;
    }

    private void NetBase_DebugInfoReceived(string msg)
    {
        try
        {
            //DebugTextRef.text += $"{msg}{Environment.NewLine}";
            Debug.Log(msg);
        }
        catch
        {
            Invoke(() =>
            {
                //DebugTextRef.text += $"{msg}{Environment.NewLine}";
                Debug.Log(msg);
            });
        }
    }

    public void CopyLogToClipboard()
    {
        GUIUtility.systemCopyBuffer = DebugTextRef.text;
    }

    public void ShowError(string reason)
    {
        ErrorReasonText.SetText(reason);
        ErrorCanvasRef.SetActive(true);
    }

    public void CloseErrorCanvas()
    {
        ErrorCanvasRef.SetActive(false);

        NetworkingGlobal.FirstLobby = true;
        NetworkingGlobal.ActiveRoleHashes = new List<string>();
        NetworkingGlobal.CloseClientInstance();
        NetworkingGlobal.CloseServerInstance();

        if (SceneManager.GetActiveScene().name != "TitleScreen")
            SceneManager.LoadScene("TitleScreen");
    }

    // Update is called once per frame
    void Update()
    {
        while (ToInvoke.Count > 0)
        {
            Invokee i = ToInvoke[0];
            i.Output = i.Method.Invoke(i.MethodInstance, null);
            i.Done = true;
            ToInvoke.RemoveAt(0);
        }
    }

    public static void Invoke(Action a, bool waitForCompletion = false)
    {
        _ = ~new Invokee(a.Target, a.Method, waitForCompletion);
    }

    public static TOut Invoke<TOut>(Func<TOut> f)
    {
        return (TOut)~new Invokee(f.Target, f.Method, true);
    }

}

public sealed class Invokee
{
    public bool Done = false;
    public object MethodInstance;
    public MethodInfo Method;
    public object Output = null;
    private bool wfc;

    public Invokee(object instance, MethodInfo a, bool waitForCompletion = false)
    {
        wfc = waitForCompletion;
        MethodInstance = instance;
        Method = a;
        InvokerObj.ToInvoke.Add(this);
    }

    public static object operator ~(Invokee self)
    {
        if (self.wfc || self.Method.ReturnType != typeof(void))
        {
            while (!self.Done)
                Thread.Sleep(1);
            return self.Output;
        }
        return null;
    }
}