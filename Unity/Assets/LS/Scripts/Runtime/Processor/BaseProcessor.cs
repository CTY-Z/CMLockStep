using FrameSync;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct ProtocolMessage
{
    public string Key { get; set; }
    public string Raw { get; set; }
    public Dictionary<string, string> Args { get; set; }
    public byte[] RawBytes { get; set; }
    public DateTime ReceivedAt { get; set; }
}

public class BaseProcessor
{
    protected Dictionary<int, Action> dic_key_handler;

    public BaseProcessor()
    {
        dic_key_handler = new();
    }

    protected void Add(int key, Action handler)
    {
        if (dic_key_handler.ContainsKey(key))
            Debug.LogError("协议注册重复");

        dic_key_handler[key] = handler;
    }
}
