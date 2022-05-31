﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;

namespace Scorpio.Ini {
    /// <summary> 读取ini文件 </summary>
    public class ScorpioIni {
        public const string HintString = "注释使用 # ; //  数据格式为 key=value, 不支持回车";
        public const string DefaultSection = "";

        /// <summary> 所有数据 </summary>
        private Dictionary<string, ScorpioIniSection> m_ConfigData = new Dictionary<string, ScorpioIniSection>();
        public string File { get; set; }
        /// <summary> 构造函数 </summary>
        public ScorpioIni() { }
        /// <summary> 构造函数 </summary>
        public ScorpioIni(byte[] bytes, Encoding encoding) {
            InitFormBuffer(bytes, encoding);
        }
        public ScorpioIni(string file, Encoding encoding) {
            InitFormFile(file, encoding);
        }
        /// <summary> 构造函数 </summary>
        public ScorpioIni(string data) {
            InitFormString(data);
        }
        /// <summary> 根据BYTE[]初始化数据 </summary>
        public void InitFormBuffer(byte[] buffer, Encoding encoding) {
            InitFormString(encoding.GetString(buffer, 0, buffer.Length));
        }
        public void InitFormFile(string file, Encoding encoding) {
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate)) {
                File = file;
                long length = fs.Length;
                byte[] buffer = new byte[length];
                fs.Read(buffer, 0, (int)length);
                InitFormString(encoding.GetString(buffer));
            }
        }
        /// <summary> 根据string初始化数据 </summary>
        public void InitFormString(string buffer) {
            try {
                m_ConfigData.Clear();
                string[] datas = buffer.Split('\n');    //所有行数据
                string section = DefaultSection;        //区间
                string comment = "";                    //注释
                int count = datas.Length;
                for (int i = 0; i < count; ++i) {
                    string data = datas[i].Trim();
                    if (string.IsNullOrWhiteSpace(data)) { continue; }
                    // [#] [;] [//] 开头都可以注释单行注释
                    if (data.StartsWith("//") || data.StartsWith("#") || data.StartsWith(";")) {
                        comment = data;
                        continue;
                    }
                    if (data.StartsWith("[") && data.EndsWith("]")) {
                        section = data.Substring(1, data.Length - 2).Trim();
                        if (string.IsNullOrWhiteSpace(section)) {
                            throw new System.Exception($"{i+1}行填写错误,{data}, 区间值不能为空,{buffer}");
                        }
                    } else {
                        int index = data.IndexOf("=");
                        if (index >= 0) {
                            string key = data.Substring(0, index).Trim();
                            string value = data.Substring(index + 1).Trim().Replace("\\n", "\n");
                            Set(section, key, value, comment.ToString());
                        } else {
                            throw new System.Exception($"{i+1}行填写错误,{data}, {HintString},{buffer}");
                        }
                    }
                }
            } catch (System.Exception e) {
                throw new System.Exception("initialize is error : " + e.ToString());
            }
        }
        /// <summary> 返回所有数据 </summary>
        public ReadOnlyDictionary<string, ScorpioIniSection> Datas {
            get {
                return new ReadOnlyDictionary<string, ScorpioIniSection>(m_ConfigData);
            }
        }
        /// <summary> 返回单个模块的数据 </summary>
        public ScorpioIniSection GetSection() { return GetSection(DefaultSection); }
        /// <summary> 返回单个模块的数据 </summary>
        public ScorpioIniSection GetSection(string section) { return m_ConfigData.ContainsKey(section) ? m_ConfigData[section] : null; }
        /// <summary> 获得Value </summary>
        public string Get(string key) { return Get(DefaultSection, key); }
        /// <summary> 设置Value </summary>
        public string Get(string section, string key) { return GetDef(section, key, null); }
        public string GetDef(string key, string def) { return GetDef(DefaultSection, key, def); }
        public string GetDef(string section, string key, string def) {
            var value = GetValue(section, key);
            return value != null ? value.value : def;
        }
        public T Get<T>(string key) { return Get<T>(DefaultSection, key); }
        /// <summary> 设置Value </summary>
        public T Get<T>(string section, string key) { return GetDef<T>(section, key, default(T)); }
        public T GetDef<T>(string key, T def) { return GetDef(DefaultSection, key, def); }
        public T GetDef<T>(string section, string key, T def) {
            var value = GetValue(section, key);
            if (value == null || string.IsNullOrEmpty(value.value))
                return def;
            try {
                return (T)Convert.ChangeType(value.value, typeof(T));
            } catch (System.Exception) {
                return def;
            }
        }

        public ScorpioIniValue GetValue(string key) { return GetValue("", key); }
        /// <summary> 设置Value </summary>
        public ScorpioIniValue GetValue(string section, string key) {
            if (m_ConfigData.Count == 0) return null;
            if (section == null) section = "";
            return m_ConfigData.ContainsKey(section) ? m_ConfigData[section].Get(key) : null;
        }
        /// <summary> 设置Value </summary>
        public void Set(string key, string value) { Set(DefaultSection, key, value); }
        /// <summary> 设置Value </summary>
        public void Set(string section, string key, string value) { Set(section, key, value, null); }
        /// <summary> 设置Value </summary>
        public void Set(string section, string key, string value, string comment) {
            if (!string.IsNullOrEmpty(section) && section.IndexOf('\n') >= 0) {
                throw new System.Exception($"section:{section} key:{key} comment:{comment} section 包含回车");
            }
            if (!string.IsNullOrEmpty(key) && key.IndexOf('\n') >= 0) {
                throw new System.Exception($"section:{section} key:{key} comment:{comment} key 包含回车");
            }
            if (!string.IsNullOrEmpty(comment) && comment.IndexOf('\n') >= 0) {
                throw new System.Exception($"section:{section} key:{key} comment:{comment} comment 包含回车");
            }
            if (!m_ConfigData.ContainsKey(section))
                m_ConfigData.Add(section, new ScorpioIniSection(section));
            m_ConfigData[section].Set(key, value, comment);
        }
        /// <summary> 判断Key是否存在 </summary>
        public bool Has(string key) { return Has(DefaultSection, key); }
        /// <summary> 判断Key是否存在 </summary>
        public bool Has(string section, string key) { return Get(section, key) != null; }
        public bool HasSection() { return HasSection(DefaultSection); }
        public bool HasSection(string section) { return m_ConfigData.ContainsKey(section); }
        /// <summary> 删除Key </summary>
        public bool Remove(string key) { return Remove(DefaultSection, key); }
        /// <summary> 删除Key </summary>
        public bool Remove(string section, string key) {
            if (m_ConfigData.ContainsKey(section)) {
                return m_ConfigData[section].Remove(key);
            }
            return false;
        }
        /// <summary> 清空一个模块 </summary>
        public bool RemoveSection() { return RemoveSection(DefaultSection); }
        /// <summary> 清空一个模块 </summary>
        public bool RemoveSection(string section) {
            if (m_ConfigData.ContainsKey(section)) {
                m_ConfigData.Remove(section);
                return true;
            }
            return false;
        }
        /// <summary> 清空所有数据 </summary>
        public void Clear() {
            m_ConfigData.Clear();
        }
        /// <summary> 返回数据字符串 </summary>
        public string BuilderString() {
            var datas = new SortedDictionary<string, ScorpioIniSection>(m_ConfigData);
            StringBuilder builder = new StringBuilder();
            if (datas.ContainsKey(DefaultSection)) {
                Content(builder, datas[DefaultSection]);
            }
            foreach (var pair in datas) {
                if (string.IsNullOrEmpty(pair.Key))
                    continue;
                Content(builder, pair.Value);
            }
            return builder.ToString();
        }
        void Content(StringBuilder builder, ScorpioIniSection section) {
            if (section.section != DefaultSection)
                builder.AppendFormat("[{0}]\n", section.section);
            foreach (var data in section.Datas) {
                var value = data.Value;
                if (!string.IsNullOrEmpty(value.comment))
                    builder.AppendLine(value.comment);
                builder.AppendFormat("{0}={1}\n", data.Key, value.value.Replace("\n", "\\n"));
            }
        }
        public void SaveToFile() {
            if (!string.IsNullOrEmpty(File)) {
                System.IO.File.WriteAllBytes(File, Encoding.UTF8.GetBytes(BuilderString()));
            }
        }
    }
}