﻿using System;
using System.Text;

namespace Scorpio.Proto {
    public class ProtoString {
        public static ScriptType Load(Script script, ScriptType ret) {
            ret.SetValue("format", script.CreateFunction(new format()));
            ret.SetValue("csFormat", script.CreateFunction(new csFormat()));
            ret.SetValue("isNullOrEmpty", script.CreateFunction(new isNullOrEmpty()));
            ret.SetValue("join", script.CreateFunction(new join()));

            ret.SetValue("length", script.CreateFunction(new length()));
            ret.SetValue("count", script.CreateFunction(new length()));
            ret.SetValue("at", script.CreateFunction(new at()));
            ret.SetValue("insert", script.CreateFunction(new insert()));
            ret.SetValue("remove", script.CreateFunction(new remove()));
            ret.SetValue("toLower", script.CreateFunction(new toLower()));
            ret.SetValue("toUpper", script.CreateFunction(new toUpper()));
            ret.SetValue("trim", script.CreateFunction(new trim()));
            ret.SetValue("trimStart", script.CreateFunction(new trimStart()));
            ret.SetValue("trimEnd", script.CreateFunction(new trimEnd()));
            ret.SetValue("replace", script.CreateFunction(new replace()));
            ret.SetValue("indexOf", script.CreateFunction(new indexOf()));
            ret.SetValue("lastIndexOf", script.CreateFunction(new lastIndexOf()));
            ret.SetValue("startsWith", script.CreateFunction(new startsWith()));
            ret.SetValue("endsWith", script.CreateFunction(new endsWith()));
            ret.SetValue("contains", script.CreateFunction(new contains()));
            ret.SetValue("sub", script.CreateFunction(new sub()));
            ret.SetValue("split", script.CreateFunction(new split(script)));
            return ret;
        }
        private class length : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue((double)thisObject.stringValue.Length);
            }
        }
        private class at : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue((double)thisObject.stringValue[args[0].ToInt32()]);
            }
        }
        private class insert : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                if (length == 2) {
                    return new ScriptValue(thisObject.stringValue.Insert(args[0].ToInt32(), args[1].ToString()));
                }
                return thisObject;
            }
        }
        private class remove : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                if (length == 1) {
                    return new ScriptValue(thisObject.stringValue.Remove(args[0].ToInt32()));
                } else if (length == 2) {
                    return new ScriptValue(thisObject.stringValue.Remove(args[0].ToInt32(), args[0].ToInt32()));
                }
                return thisObject;
            }
        }
        private class toLower : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue(thisObject.stringValue.ToLower());
            }
        }
        private class toUpper : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue(thisObject.stringValue.ToUpper());
            }
        }
        private class trim : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue(thisObject.stringValue.Trim());
            }
        }
        private class trimStart : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue(thisObject.stringValue.TrimStart());
            }
        }
        private class trimEnd : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue(thisObject.stringValue.TrimEnd());
            }
        }
        private class replace : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return new ScriptValue(thisObject.stringValue.Replace(args[0].ToString(), args[1].ToString()));
            }
        }
        private class indexOf : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                if (length == 3) {
                    return new ScriptValue((double)thisObject.stringValue.IndexOf(args[0].ToString(), args[1].ToInt32(), args[2].ToInt32()));
                } else if (length == 2) {
                    return new ScriptValue((double)thisObject.stringValue.IndexOf(args[0].ToString(), args[1].ToInt32()));
                } else {
                    return new ScriptValue((double)thisObject.stringValue.IndexOf(args[0].ToString()));
                }
            }
        }
        private class lastIndexOf : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                if (length == 3) {
                    return new ScriptValue((double)thisObject.stringValue.LastIndexOf(args[0].ToString(), args[1].ToInt32(), args[2].ToInt32()));
                } else if (length == 2) {
                    return new ScriptValue((double)thisObject.stringValue.LastIndexOf(args[0].ToString(), args[1].ToInt32()));
                } else {
                    return new ScriptValue((double)thisObject.stringValue.LastIndexOf(args[0].ToString()));
                }
            }
        }
        private class startsWith : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return thisObject.stringValue.StartsWith(args[0].ToString()) ? ScriptValue.True : ScriptValue.False;
            }
        }
        private class endsWith : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return thisObject.stringValue.EndsWith(args[0].ToString()) ? ScriptValue.True : ScriptValue.False;
            }
        }
        private class contains : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                return thisObject.stringValue.Contains(args[0].ToString()) ? ScriptValue.True : ScriptValue.False;
            }
        }
        private class sub : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                if (length == 0)
                    return thisObject;
                else if (length == 1)
                    return new ScriptValue(thisObject.ToString().Substring(args[0].ToInt32()));
                else
                    return new ScriptValue(thisObject.ToString().Substring(args[0].ToInt32(), args[1].ToInt32()));
            }
        }
        private class split : ScorpioHandle {
            private Script m_script;
            public split(Script script) {
                this.m_script = script;
            }
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                var splits = new string[length];
                for (var i = 0; i < length; ++i) { splits[i] = args[i].ToString(); }
                var strs = thisObject.stringValue.Split(splits, StringSplitOptions.RemoveEmptyEntries);
                var ret = new ScriptArray(m_script);
                foreach (string str in strs) {
                    ret.Add(new ScriptValue(str));
                }
                return new ScriptValue(ret);
            }
        }

        public class format : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                var format = args[0].ToString();
                var index = 1;
                var strLength = format.Length;
                var strLength1 = strLength - 1;
                var builder = new StringBuilder();
                for (var i = 0; i < strLength;) {
                    var c = format[i];
                    if (c == '{' && i < strLength1 && format[i + 1] == '}') {
                        i += 2;
                        builder.Append(args[index++]);
                    } else {
                        builder.Append(c);
                        ++i;
                    }
                }
                return new ScriptValue(builder.ToString());
            }
        }
        public class csFormat : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                if (length == 0) return ScriptValue.Null;
                var format = args[0].ToString();
                if (length == 1) return new ScriptValue(format);
                var objs = new object[length - 1];
                for (var i = 1; i < length; ++i) {
                    objs[i - 1] = args[i].Value;
                }
                return new ScriptValue(string.Format(format, objs));
            }
        }
        private class isNullOrEmpty : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                switch (args[0].valueType) {
                    case ScriptValue.nullValueType: return ScriptValue.True;
                    case ScriptValue.stringValueType: return args[0].stringValue.Length == 0 ? ScriptValue.True : ScriptValue.False;
                    default: return ScriptValue.False;
                }
            }
        }
        private class join : ScorpioHandle {
            public ScriptValue Call(ScriptValue thisObject, ScriptValue[] args, int length) {
                var separator = args[0].ToString();
                var value = args[1].Get<ScriptArray>();
                var count = value.Length();
                var values = new string[count];
                for (int i = 0; i < count; ++i) { values[i] = value[i].ToString(); }
                return new ScriptValue(string.Join(separator, values));
            }
        }
    }
}
