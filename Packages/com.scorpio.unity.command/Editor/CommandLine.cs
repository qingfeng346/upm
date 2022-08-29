using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scorpio.Unity.Command {
    public class CommandLineArgument {
        private List<string> args = new List<string>();
        public CommandLineArgument(string name) {
        }
        public void Add(string arg) {
            args.Add(arg);
        }
        public string[] GetValues() {
            return args.Count > 0 ? args.ToArray() : null;
        }
        public string GetValue(string def) {
            return args.Count > 0 ? args[0] : def;
        }
    }
    public class CommandLine {
        public static CommandLine Parse(string[] args) {
            return new CommandLine().Parser(args);
        }
        private Dictionary<string, CommandLineArgument> arguments = new Dictionary<string, CommandLineArgument>();
        public string Type { get; private set; }
        public CommandLine Parser(string[] args) {
            arguments.Clear();
            Type = "";
            CommandLineArgument argument = null;
            for (int i = 0; i < args.Length; ++i) {
                var arg = args[i];
                if (arg.StartsWith("-")) {
                    if (arguments.ContainsKey(arg)) {
                        argument = arguments[arg];
                    } else {
                        argument = new CommandLineArgument(arg);
                        arguments[arg] = argument;
                    }
                } else if (argument != null) {
                    argument.Add(arg);
                } else {
                    Type = arg;
                }
            }
            return this;
        }
        public bool HadValue(string key) {
            return arguments.ContainsKey(key);
        }
        public bool HadValue(params string[] keys) {
            foreach (var key in keys) {
                if (arguments.ContainsKey(key)) {
                    return true;
                }
            }
            return false;
        }
        public string[] GetValues(string key) {
            return arguments.ContainsKey(key) ? arguments[key].GetValues() : null;
        }
        public string GetValue(string key) {
            return GetValueDefault(key, null);
        }
        public string GetValue(params string[] keys) {
            return GetValueDefault(keys, null);
        }
        public string GetValueDefault(string key, string def) {
            return GetValueDefault(new string[] { key }, def);
        }
        public string GetValueDefault(string[] keys, string def) {
            foreach (var key in keys) {
                if (arguments.ContainsKey(key)) {
                    return arguments[key].GetValue(def);
                }
            }
            return def;
        }
        public T GetValue<T>(string key) {
            return (T)Convert.ChangeType(GetValue(key), typeof(T));
        }
        public T GetValue<T>(params string[] keys) {
            return (T)Convert.ChangeType(GetValue(keys), typeof(T));
        }
        public object GetValue(string key, Type type) {
            return ChangeType(GetValue(key), type);
        }
        public object GetValue(Type type, params string[] keys) {
            return ChangeType(GetValue(keys), type);
        }
        public object ChangeType(string value, Type type) {
            if (type.IsArray) {
                var strs = value.Split(',');
                var ret = Array.CreateInstance(type.GetElementType(), strs.Length);
                for (var i = 0; i < strs.Length;++i) {
                    ret.SetValue(ChangeElementType(strs[i], type.GetElementType()), i);
                }
                return ret;
            } else {
                return ChangeElementType(value, type);
            }
        }
        object ChangeElementType(string value, Type type) {
            if (type == typeof(string)) {
                return value;
            } else if (type == typeof(bool)) {
                value = value.ToLowerInvariant();
                return value == "true" || value == "yes" || value == "1";
            } else if (type == typeof(sbyte) ||
                       type == typeof(byte) ||
                       type == typeof(short) ||
                       type == typeof(ushort) ||
                       type == typeof(int) ||
                       type == typeof(uint) ||
                       type == typeof(long) ||
                       type == typeof(ulong) ||
                       type == typeof(float) ||
                       type == typeof(double) ||
                       type == typeof(decimal)) {
                return Convert.ChangeType(value, type);
            } else if (type.IsEnum) {
                if (int.TryParse(value, out var result)) {
                    return Enum.ToObject(type, result);
                } else {
                    return Enum.Parse(type, value, true);
                }
            } else if (type == typeof(DateTime)) {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(double.Parse(value));
            } else {
                return null;
            }
        }
    }
}