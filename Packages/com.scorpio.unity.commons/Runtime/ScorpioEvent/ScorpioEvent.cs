using System;
using System.Collections.Generic;
namespace Scorpio.Events {
    //回调  参数, event key, event name, event actionKey
    using EventAction = Action<object, string, string, string>;
    public struct EventData {
        public string actionKey;    //关键字
        public EventAction action;  //回调
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public override bool Equals(object obj) {
            if (obj is EventData) {
                EventData other = (EventData)obj;
                return other.actionKey == actionKey && other.action == action;
            }
            return false;
        }
    }
    //触发器，不支持多线程
    public class ScorpioEvent {

        private Dictionary<string, List<EventData>> mEvents = new Dictionary<string, List<EventData>>();                
        public string Name { get; private set; }        //Event名字
        public ScorpioEvent():this("") {}
        public ScorpioEvent(string name) { Name = name; }
        public void Register(EventAction action) {
            Register("", "", action);
        }
        public void Register(string key, EventAction action) {
            Register(key, "", action);
        }
        public void Register(string key, string actionKey,EventAction action) {
            var data = new EventData() { actionKey = actionKey, action = action};
            List<EventData> events;
            if (mEvents.TryGetValue(key, out events)) {
                if (!events.Contains(data))
                    events.Add(data);
            } else {
                mEvents.Add(key, new List<EventData>() { data });
            }
        }
        
        public void UnRegister(EventAction action) {
            UnRegister("", "", action);
        }
        public void UnRegister(string key, EventAction action) {
            UnRegister(key, "", action);
        }
        public void UnRegister(string key, string actionKey, EventAction action) {
            List<EventData> events;
            if (mEvents.TryGetValue(key, out events)) {
                var index = events.IndexOf(new EventData() { actionKey = actionKey, action = action });
                if (index >= 0) {
                    events.RemoveAt(index);
                }
            }
        }

        public void UnRegisterAll() {
            mEvents.Clear();
        }
        public void UnRegisterAll(string key) {
            List<EventData> events;
            if (mEvents.TryGetValue(key, out events)) {
                events.Clear();
            }
        }
        public void UnRegisterAll(string key, string actionKey) {
            List<EventData> events;
            if (mEvents.TryGetValue(key, out events)) {
                events.RemoveAll((data) => data.actionKey == actionKey);
            }
        }

        public void UnRegisterAllByActionKey(string actionKey) {
            foreach (var pair in mEvents) {
                pair.Value.RemoveAll((data) => data.actionKey == actionKey);
            }
        }
        public void FireEvent() {
            FireEvent("", null);
        }
        public void FireEvent(string key) {
            FireEvent(key, null);
        }
        public void FireEvent(string key, object args) {
            List<EventData> events;
            if (mEvents.TryGetValue(key, out events)) {
                events.ForEach((data) => data.action(args, key, Name, data.actionKey));
            }
        }
        public void FireAllEvent() {
            FireAllEvent(null);
        }
        public void FireAllEvent(object args) {
            foreach (var pair in mEvents) {
                pair.Value.ForEach((data) => data.action(args, pair.Key, Name, data.actionKey));
            }
        }
    }
}