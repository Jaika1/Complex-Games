using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

public class InvokerObj : MonoBehaviour
{
    public static List<Invokee> ToInvoke = new List<Invokee>();

    private void Start()
    {
        if (FindObjectsOfType(typeof(InvokerObj)).Length > 1)
            Destroy(this);
        else
            DontDestroyOnLoad(this);
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

    public static void Invoke(Action a)
    {
        _ = ~new Invokee(a.Target, a.Method);
    }

    public static TOut Invoke<TOut>(Func<TOut> f)
    {
        return (TOut)~new Invokee(f.Target, f.Method);
    }
}

public sealed class Invokee
{
    public bool Done = false;
    public object MethodInstance;
    public MethodInfo Method;
    public object Output = null;

    public Invokee(object instance, MethodInfo a)
    {
        MethodInstance = instance;
        Method = a;
        InvokerObj.ToInvoke.Add(this);
    }

    public static object operator ~(Invokee self)
    {
        while (!self.Done)
            Thread.Sleep(1);
        return self.Output;
    }
}