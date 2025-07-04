using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Nakshatra.Plugins.Editor
{
    /// <summary>
    /// Popup window for selecting a KeyCode with search + scroll
    /// </summary>
    public class KeyCodePopupWindow : PopupWindowContent
    {
        SerializedProperty _property;
        string[] _allNames;
        string _search = "";
        Vector2 _scroll;

        public KeyCodePopupWindow(SerializedProperty property)
        {
            _property = property;
            _allNames = Enum.GetNames(typeof(KeyCode));
        }

        public override Vector2 GetWindowSize() => new Vector2(250, 350);

        public override void OnGUI(Rect rect)
        {
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));

            var filtered = _allNames.Where(n => string.IsNullOrEmpty(_search)
                || n.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var name in filtered)
            {
                if (GUILayout.Button(name, EditorStyles.label))
                {
                    int idx = Array.IndexOf(_allNames, name);
                    _property.enumValueIndex = idx;
                    _property.serializedObject.ApplyModifiedProperties();
                    editorWindow.Close();
                    return;
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// Property drawer to show KeyCode fields as a searchable popup
    /// </summary>
    [CustomPropertyDrawer(typeof(KeyCode))]
    public class KeyCodePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var current = property.enumDisplayNames[property.enumValueIndex];
            if (EditorGUI.DropdownButton(position, new GUIContent(current), FocusType.Keyboard))
                UnityEditor.PopupWindow.Show(position, new KeyCodePopupWindow(property));
            EditorGUI.EndProperty();
        }

    #if UNITY_2021_2_OR_NEWER
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.Add(new IMGUIContainer(() =>
            {
                // Draw popup via IMGUI inside UIElements
                EditorGUI.BeginProperty(new Rect(), new GUIContent(property.displayName), property);
                var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
                OnGUI(rect, property, new GUIContent(property.displayName));
                EditorGUI.EndProperty();
            }));
            return container;
        }
    #endif
    }
}