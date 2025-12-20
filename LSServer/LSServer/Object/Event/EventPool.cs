using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace LSServer.Utils
{
    public interface IEventObject { }

    public class EventObject<T> : IEventObject
    {
        public Action<T> e;
        public EventObject(Action<T> action) => e += action;
    }

    public static class EventPool
    {
        private static Dictionary<string, IEventObject> eventDic = new Dictionary<string, IEventObject>();

        public static void Register<T>(string name, Action<T> action)
        {
            if (eventDic.ContainsKey(name))
            {
                if (eventDic[name] is EventObject<T> args)
                    args.e += action;
            }
            else
                eventDic.Add(name, new EventObject<T>(action));
        }

        public static void Remove<T>(string name, Action<T> action)
        {
            if (eventDic.ContainsKey(name))
            {
                if (eventDic[name] is EventObject<T> args)
                    args.e -= action;
            }
        }

        public static void Fire<T>(string name, T param)
        {
            if (eventDic.ContainsKey(name))
                (eventDic[name] as EventObject<T>)?.e.Invoke(param);
        }

        public static void Clear()
        {
            eventDic.Clear();
        }
    }
}


