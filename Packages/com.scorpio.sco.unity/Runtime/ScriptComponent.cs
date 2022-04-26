using System;
using UnityEngine;
namespace Scorpio.Unity {
    public class ScriptComponent : ScriptComponentBase {
        public override ScriptInstance Initialize(ScriptInstance table, string name) {
            base.Initialize(table, name);
            if (table.GetValue(ScriptComponentUpdate.StringUpdate).IsScriptObject) {
                gameObject.AddComponent<ScriptComponentUpdate>().Initialize(table, name);
            }
            if (table.GetValue(ScriptComponentFixedUpdate.StringFixedUpdate).IsScriptObject) {
                gameObject.AddComponent<ScriptComponentFixedUpdate>().Initialize(table, name);
            }
            if (table.GetValue(ScriptComponentLateUpdate.StringLateUpdate).IsScriptObject) {
                gameObject.AddComponent<ScriptComponentLateUpdate>().Initialize(table, name);
            }
            if (table.GetValue(ScriptComponentOnGUI.StringOnGUI).IsScriptObject) {
                gameObject.AddComponent<ScriptComponentOnGUI>().Initialize(table, name);
            }
            if (table.GetValue(ScriptComponentFrameUpdate.StringFrameUpdate).IsScriptObject) {
                var com = gameObject.AddComponent<ScriptComponentFrameUpdate>();
                com.Initialize(table, name);
                if (table.HasValue(ScriptComponentFrameUpdate.StringFrameUpdateValue)) {
                    com.frame = (uint)table.GetValue(ScriptComponentFrameUpdate.StringFrameUpdateValue).ToInt32();
                }
            }
            if (table.GetValue(ScriptComponentTimeUpdate.StringTimeUpdate).IsScriptObject) {
                var com = gameObject.AddComponent<ScriptComponentTimeUpdate>();
                com.Initialize(table, name);
                if (table.HasValue(ScriptComponentTimeUpdate.StringTimeUpdateValue)) {
                    com.time = Convert.ToSingle(table.GetValue(ScriptComponentTimeUpdate.StringTimeUpdateValue).doubleValue);
                }
            }
            table.SetValue("gameObject", ScriptValue.CreateValue(this.gameObject));
            table.SetValue("transform", ScriptValue.CreateValue(this.transform));
            table.SetValue("com", ScriptValue.CreateValue(this));
            SetObjects(gameObject.GetComponent<ScriptComponentObjects>());
            FindChild(Table.GetValue("Objects").Get<ScriptMap>());
            RegisterButtonClick(Table.GetValue("Buttons").Get<ScriptArray>());
            OnStart();
            return table;
        }
        void SetObjects(ScriptComponentObjects objects) {
            if (objects == null) { return; }
            foreach (var value in objects.values) {
                if (!string.IsNullOrEmpty(value.name) && value.value != null) {
                    Table.SetValue(value.name, ScriptValue.CreateValue(value.value));
                }
            }
            foreach (var click in objects.clicks) {
                if (!string.IsNullOrEmpty(click.click) && click.button != null) {
                    var clickFunc = Table.GetValue(click.click).Get<ScriptFunction>();
                    if (clickFunc == null) { continue; }
                    click.button.RegisterOnClick(clickFunc.SetBindObject(Value));
                }
            }
        }
        void FindChild(ScriptMap objects) {
            if (objects == null) { return; }
            foreach (var info in objects) {
                var component = info.Value.GetValue("Component").Get<ScriptObject>();
                var path = info.Value.GetValue("Path").ToString();
                if (component == null) {
                    Table.SetValue(info.Key.ToString(), ScriptValue.CreateValue(gameObject.FindChild(path)));
                } else {
                    Table.SetValue(info.Key.ToString(), ScriptValue.CreateValue(gameObject.FindChild(path, component.Type)));
                }
            }
        }
        void RegisterButtonClick(ScriptArray buttons) {
            if (buttons == null) { return; }
            foreach (var pair in buttons) {
                var info = pair.Get<ScriptMap>();
                if (info == null) { continue; }
                var click = Table.GetValue(info.GetValue("Click").ToString()).Get<ScriptFunction>();
                if (click == null) { continue; }
                gameObject.FindChild(info.GetValue("Path").ToString()).RegisterOnClick(click.SetBindObject(Value));
            }
        }
        void OnStart() {
            Call("Awake");
        }
        void OnTriggerEnter(Collider collider) {
            Call("OnTriggerEnter", collider);
        }
        void OnTriggerExit(Collider collider) {
            Call("OnTriggerExit", collider);
        }
        void OnTriggerEnter2D(Collider2D collider) {
            Call("OnTriggerEnter2D", collider);
        }
        void OnTriggerExit2D(Collider2D collider) {
            Call("OnTriggerExit2D", collider);
        }
        void OnEnable() {
            Call("OnEnable");
        }
        void OnDisable() {
            Call("OnDisable");
        }
        void OnDestroy() {
            Call("OnDestroy");
        }
        void OnSpawn() {
            Call("OnSpawn");
        }
        void OnDespawn() {
            Call("OnDespawn");
        }
        void OnBecameVisible() {
            Call("OnBecameVisible");
        }
        void OnBecameInvisible() {
            Call("OnBecameInvisible");
        }
    }
}