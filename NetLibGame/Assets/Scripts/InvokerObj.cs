using NetworkingLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class InvokerObj : MonoBehaviour
{
    public static List<Invokee> ToInvoke = new List<Invokee>();

    public GameObject DebugCanvasRef;
    public Text DebugTextRef;

    private void Start()
    {
        if (FindObjectsOfType(typeof(InvokerObj)).Length > 1)
            Destroy(this);
        else
        {
            DontDestroyOnLoad(this);
#if !UNITY_EDITOR
            Destroy(DebugCanvasRef);
#else
            NetBase.DebugInfoReceived += NetBase_DebugInfoReceived;
#endif
            Application.wantsToQuit += Application_quitting;
        }
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