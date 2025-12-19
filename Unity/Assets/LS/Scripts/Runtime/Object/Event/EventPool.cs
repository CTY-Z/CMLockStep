using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Events;

namespace LS.Utils
{
    public interface IEventObject { }

    public class EventObject<T> : IEventObject
    {
        public UnityAction<T> e;
        public EventObject(UnityAction<T> action) => e += action;
    }

    public class EventPool
    {
        private Dictionary<string, IEventObject> eventDic = new Dictionary<string, IEventObject>();

        public void Register<T>(string name, UnityAction<T> action)
        {
            if (eventDic.ContainsKey(name))
            {
                if (eventDic[name] is EventObject<T> args)
                    args.e += action;
            }
            else
                eventDic.Add(name, new EventObject<T>(action));
        }

        public void Remove<T>(string name, UnityAction<T> action)
        {
            if (eventDic.ContainsKey(name))
            {
                if (eventDic[name] is EventObject<T> args)
                    args.e -= action;
            }
        }

        public void Fire<T>(string name, T param)
        {
            if (eventDic.ContainsKey(name))
                (eventDic[name] as EventObject<T>)?.e.Invoke(param);
        }

        public void Clear()
        {
            eventDic.Clear();
        }
    }
}


