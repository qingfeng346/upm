﻿using System.IO;
using System.Text;
using System.Reflection;
using Scorpio.Instruction;
using Scorpio.Exception;
using System.Collections.Generic;
using Scorpio.Function;
using Scorpio.Library;
using Scorpio.Runtime;
using Scorpio.Proto;
using Scorpio.Userdata;
using Scorpio.Serialize;
namespace Scorpio {
    public partial class Script {
        //反射获取变量和函数的属性
        public const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public static readonly Encoding UTF8 = Encoding.UTF8;          //默认编码格式
        private const string Undefined = "Undefined";                   //Undefined
        private const string GLOBAL_NAME = "_G";                        //全局对象
        private const string GLOBAL_SCRIPT = "_SCRIPT";                 //Script对象
        private const string GLOBAL_VERSION = "_VERSION";               //版本号
        private List<string> m_SearchPath = new List<string>();         //request所有文件的路径集合
        public ScriptType TypeObject { get; private set; }                      //所有类型的基类
        public ScriptValue TypeObjectValue { get; private set; }                //所有类型的基类

        public ScriptType TypeBoolean { get; private set; }                     //所有bool的基类
        public ScriptValue TypeBooleanValue { get; private set; }               //所有bool的基类

        public ScriptType TypeNumber { get; private set; }                      //所有number的基类
        public ScriptValue TypeNumberValue { get; private set; }                //所有number的基类

        public ScriptType TypeString { get; private set; }                      //所有string的基类
        public ScriptValue TypeStringValue { get; private set; }                //所有string的基类

        public ScriptType TypeArray { get; private set; }                       //所有array的基类
        public ScriptValue TypeArrayValue { get; private set; }                 //所有array的基类

        public ScriptType TypeMap { get; private set; }                         //所有map的基类
        public ScriptValue TypeMapValue { get; private set; }                   //所有map的基类

        public ScriptType TypeFunction { get; private set; }                    //所有function的基类
        public ScriptValue TypeFunctionValue { get; private set; }              //所有function的基类

        public ScriptGlobal Global { get; private set; }                        //全局变量


        public Script() {
            Global = new ScriptGlobal();
            
            TypeObject = new ScriptTypeObject("Object");
            TypeObjectValue = new ScriptValue(TypeObject);
            Global.SetValue(TypeObject.TypeName, TypeObjectValue);

            TypeBoolean = new ScriptBasicType("Bool", TypeObjectValue);
            TypeBooleanValue = new ScriptValue(TypeBoolean);
            Global.SetValue(TypeBoolean.TypeName, TypeBooleanValue);

            TypeNumber = new ScriptBasicType("Number", TypeObjectValue);
            TypeNumberValue = new ScriptValue(TypeNumber);
            Global.SetValue(TypeNumber.TypeName, TypeNumberValue);

            TypeString = new ScriptBasicType("String", TypeObjectValue);
            TypeStringValue = new ScriptValue(TypeString);
            Global.SetValue(TypeString.TypeName, TypeStringValue);

            TypeArray = new ScriptBasicType("Array", TypeObjectValue);
            TypeArrayValue = new ScriptValue(TypeArray);
            Global.SetValue(TypeArray.TypeName, TypeArrayValue);

            TypeMap = new ScriptBasicType("Map", TypeObjectValue);
            TypeMapValue = new ScriptValue(TypeMap);
            Global.SetValue(TypeMap.TypeName, TypeMapValue);

            TypeFunction = new ScriptBasicType("Function", TypeObjectValue);
            TypeFunctionValue = new ScriptValue(TypeFunction);
            Global.SetValue(TypeFunction.TypeName, TypeFunctionValue);

            Global.SetValue(GLOBAL_NAME, new ScriptValue(Global));
            Global.SetValue(GLOBAL_SCRIPT, ScriptValue.CreateValue(this));
            Global.SetValue(GLOBAL_VERSION, ScriptValue.CreateValue(typeof(Version)));

            ProtoObject.Load(this, TypeObject);
            ProtoBoolean.Load(this, TypeBoolean);
            ProtoNumber.Load(this, TypeNumber);
            ProtoString.Load(this, TypeString);
            ProtoArray.Load(this, TypeArray);
            ProtoMap.Load(this, TypeMap);
            ProtoFunction.Load(this, TypeFunction);

            TypeManager.PushAssembly(typeof(object).Assembly);                        //mscorlib.dll
            TypeManager.PushAssembly(typeof(System.Net.Sockets.Socket).Assembly);     //System.dll
            TypeManager.PushAssembly(GetType().Assembly);                             //当前所在的程序集
            LoadLibrary();
        }
        public void LoadLibrary() {
            LibraryBasis.Load(this);
            LibraryJson.Load(this);
            LibraryMath.Load(this);
            LibraryUserdata.Load(this);
            LibraryIO.Load(this);
        }
        ScriptValue Execute(string breviary, SerializeData data) {
            var contexts = new ScriptContext[data.Functions.Length];
            for (int i = 0; i < data.Functions.Length; ++i) {
                contexts[i] = new ScriptContext(this, breviary, data.Functions[i], data.ConstDouble, data.ConstLong, data.ConstString, contexts, data.Classes);
            }
            return new ScriptContext(this, breviary, data.Context, data.ConstDouble, data.ConstLong, data.ConstString, contexts, data.Classes).Execute(ScriptValue.Null, null, 0, null);
        }
        public void PushSearchPath(string path) {
            if (!m_SearchPath.Contains(path))
                m_SearchPath.Add(path);
        }
        public ScriptValue LoadSearchPathFile(string fileName) {
            for (int i = 0; i < m_SearchPath.Count; ++i) {
                string file = m_SearchPath[i] + "/" + fileName;
                if (File.Exists(file))
                    return LoadFile(file);
            }
            throw new ExecutionException("require 找不到文件 : " + fileName);
        }
        public void SetGlobal(string key, ScriptValue value) {
            Global.SetValue(key, value);
        }
        public ScriptValue GetGlobal(string key) {
            return Global.GetValue(key);
        }
        public bool HasGlobal(string key) {
            return Global.HasValue(key);
        }
        public ScriptArray CreateArray() { return new ScriptArray(this); }
        public ScriptMap CreateMap() { return new ScriptMap(this); }
        public ScriptType CreateType(string typeName, ScriptValue parentType) { return new ScriptType(typeName, parentType); }
        public ScriptValue CreateFunction(ScorpioHandle value) { return new ScriptValue(new ScriptHandleFunction(this, value)); }

        public ScriptValue call(string name, params object[] args) {
            return Global.GetValue(name).call(ScriptValue.Null, args);
        }
        public ScriptValue Call(string name, ScriptValue[] args, int length) {
            return Global.GetValue(name).Call(ScriptValue.Null, args, length);
        }
        public ScriptValue LoadFile(string fileName) {
            return LoadFile(fileName, UTF8);
        }
        public ScriptValue LoadFile(string fileName, Encoding encoding) {
            using (var stream = File.OpenRead(fileName)) {
                var length = stream.Length;
                var buffer = new byte[length];
                stream.Read(buffer, 0, buffer.Length);
                return LoadBuffer(fileName, buffer, encoding);
            }
        }
        public ScriptValue LoadString(string buffer) {
            return LoadString(Undefined, buffer);
        }
        public ScriptValue LoadString(string breviary, string buffer) {
            if (buffer == null || buffer.Length == 0) { return ScriptValue.Null; }
            return Execute(breviary, Serializer.Serialize(breviary, buffer));
        }
        public ScriptValue LoadBuffer(byte[] buffer) {
            return LoadBuffer(Undefined, buffer, UTF8);
        }
        public ScriptValue LoadBuffer(string breviary, byte[] buffer) {
            return LoadBuffer(breviary, buffer, UTF8);
        }
        public ScriptValue LoadBuffer(string breviary, byte[] buffer, Encoding encoding) {
            if (buffer == null || buffer.Length == 0) { return ScriptValue.Null; }
            if (buffer[0] == 0)
                return Execute(breviary, Deserializer.Deserialize(buffer));
            else
                return Execute(breviary, Serializer.Serialize(breviary, encoding.GetString(buffer, 0, buffer.Length)));
        }


#if SCORPIO_DEBUG
        private Stack<StackInfo> m_StackInfos = new Stack<StackInfo>(); //堆栈信息
        internal void PushStackInfo(string breviary, int line) {
            m_StackInfos.Push(new StackInfo() { Breviary = breviary, Line = line });
        }
        internal void PopStackInfo() {
            m_StackInfos.Pop();
        }
        public StackInfo GetStackInfo() {
            return m_StackInfos.Count > 0 ? m_StackInfos.Peek() : default;
        }
        public StackInfo[] GetStackInfos() {
            return m_StackInfos.ToArray();
        }
#else
        private readonly static StackInfo[] EmptyStackInfos = new StackInfo[0];
        public StackInfo GetStackInfo() {
            return default;
        }
        public StackInfo[] GetStackInfos() {
            return EmptyStackInfos;
        }
#endif
    }
}
