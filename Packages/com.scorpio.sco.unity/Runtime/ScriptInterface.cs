namespace Scorpio.Unity {
    public abstract class ScriptInterface {
        private ScriptInstance mTable;
        public ScriptInstance Table {
            get { return mTable; }
            set {
                mTable = value;
                Value = new ScriptValue(value);
            }
        }
        public ScriptValue Value { get; protected set; }
        public ScriptValue Call(string funcName, params object[] args) {
            return Table.GetValue(funcName).call(Value, args);
        }
    }
}