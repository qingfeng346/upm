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
            Table.GetValue(funcName).call(Value, args);
        }
        public object CallRet(string funcName, params object[] args) {
            return Table.GetValue(funcName).call(Value, args).Value;
        }
    }
}
