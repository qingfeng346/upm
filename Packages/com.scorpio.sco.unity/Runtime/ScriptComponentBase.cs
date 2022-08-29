using UnityEngine;
namespace Scorpio.Unity {
    public class ScriptComponentBase : MonoBehaviour {
        public string Name = "";            //脚本Table名称，删除脚本时根据此字段做关键字
        public ScriptInstance Table { get; protected set; }
        public ScriptValue Value { get; protected set; }
        public virtual ScriptInstance Initialize(ScriptInstance table, string name) {
            Table = table;
            Name = name;
            Value = new ScriptValue(table);
            return table;
        }
        public void Call(string funcName, params object[] args) {
            var func = Table == null ? ScriptValue.Null : Table.GetValue(funcName);
            if (func.valueType == ScriptValue.scriptValueType) {
                try {
                    Table.GetValue(funcName).call(Value, args);
                } catch (System.Exception e) {
                    Debug.LogError($"{name}({Name})[{funcName}] is error func:{e}");
                }
            }
        }
        public object CallRet(string funcName, params object[] args) {
            var func = Table == null ? ScriptValue.Null : Table.GetValue(funcName);
            if (func.valueType == ScriptValue.scriptValueType) {
                try {
                    return Table.GetValue(funcName).call(Value, args).Value;
                } catch (System.Exception e) {
                    Debug.LogError($"{name}({Name})[{funcName}] is error func:{e}");
                }
            }
            return null;
        }
    }
}
