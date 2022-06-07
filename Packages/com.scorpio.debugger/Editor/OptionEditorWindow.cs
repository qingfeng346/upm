using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace Scorpio.Debugger.Editor {
    public class OptionEditorWindow : EditorWindow {
        [MenuItem("Debugger/DebuggerOptions")]
        static void Open() {
            GetWindow<OptionEditorWindow>("DebuggerOptions").Show();
        }
        Vector2 scroll = Vector2.zero;
        void OnGUI() {
            var options = new List<Tuple<string, List<OptionEntry>>>();
            ScorpioDebugger.Instance.OptionEntries.ForEach(entry => {
                var item = options.Find(_ => _.Item1 == entry.title);
                if (item != null) {
                    item.Item2.Add(entry);
                } else {
                    options.Add(new Tuple<string, List<OptionEntry>>(entry.title, new List<OptionEntry>() { entry }));
                }
            });
            scroll = GUILayout.BeginScrollView(scroll);
            int x = 0, y = 0, maxY = 0;
            float width = EditorGUIUtility.currentViewWidth;
            foreach (var option in options) {
                GUI.Label(new Rect(0, y, width, 20), option.Item1);
                y += 20;
                x = 0;
                maxY = 0;
                bool first = true;
                foreach (var entry in option.Item2) {
                    if (!first && (x + entry.value.preferredWidth) >= width) {
                        y += maxY;
                        x = 0;
                        maxY = 0;
                    }
                    first = false;
                    var rect = new Rect(x, y, entry.value.preferredWidth, entry.value.preferredHeight);
                    maxY = Math.Max(maxY, entry.value.preferredHeight);
                    switch (entry.type) {
                        case OptionType.Button:
                            DrawButton(rect, entry);
                            break;
                        case OptionType.Label:
                            DrawLabel(rect, entry);
                            break;
                        case OptionType.Toggle:
                            DrawToggle(rect, entry);
                            break;
                        case OptionType.Dropdown:
                            DrawDropdown(rect, entry);
                            break;
                        case OptionType.Input:
                            DrawInput(rect, entry);
                            break;
                        case OptionType.Slider:
                            DrawSlider(rect, entry);
                            break;
                        case OptionType.Image:
                            DrawImage(rect, entry);
                            break;
                        case OptionType.RawImage:
                            DrawRawImage(rect, entry);
                            break;
                    }
                    x += entry.value.preferredWidth;
                }
                y += maxY;
            }
            GUILayout.Space(y);
            GUILayout.EndScrollView();
        }
        void DrawButton(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueButton;
            if (GUI.Button(rect, value.label)) {
                value.action();
            }
        }
        void DrawLabel(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueLabel;
            GUI.Label(rect, value.label);
        }
        void DrawToggle(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueToggle;
            var newIsOn = GUI.Toggle(rect, value.isOn, value.label);
            if (newIsOn != value.isOn) {
                value.isOn = newIsOn;
                value.action(newIsOn);
            }
        }
        void DrawDropdown(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueDropdown;
            var newSelect = EditorGUI.Popup(rect, value.value, value.options);
            if (newSelect != value.value) {
                value.value = newSelect;
                value.action(newSelect);
            }
        }
        void DrawInput(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueInput;
            var newText = EditorGUI.TextField(rect, value.value);
            if (newText != value.value) {
                value.value = newText;
                value.action(newText);
            }
        }
        void DrawSlider(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueSlider;
            var format = string.IsNullOrEmpty(value.format) ? "{0:0.##}" : value.format;
            GUI.Label(rect, string.Format(format, value.Value));
            rect.x += 30;
            rect.width -= 30;
            var newValue = GUI.HorizontalSlider(rect, value.value, 0f, 1f);
            if (newValue != value.value) {
                value.value = newValue;
                value.action(newValue);
            }
        }
        void DrawImage(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueImage;
            GUI.DrawTexture(rect, value.sprite.texture);
        }
        void DrawRawImage(Rect rect, OptionEntry entry) {
            var value = entry.value as OptionValueRawImage;
            GUI.DrawTexture(rect, value.texture);
        }
    }

}
