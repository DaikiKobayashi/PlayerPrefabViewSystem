using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public sealed class PlayerPrefasViewEditor : EditorWindow
{
    private static readonly string[] NON_TARGET_KEY = { "unity.cloud_userid", "unity.player_sessionid", "unity.player_session_count", "UnityGraphicsQuality" };

    private List<(string key, string value, ValueType valType)> prefsCaches = new();
    private Dictionary<string, string> caches = new();

    // AddField
    enum ValueType { Int, Float, String }

    private string addKey = string.Empty;
    private string addValue = string.Empty;
    private ValueType addType = ValueType.String;

    [MenuItem("Tools/PlayerPrefasView")]
    private static void Open()
    {
        var window = GetWindow<PlayerPrefasViewEditor>();
        window.Show();
        window.GetPlayerPrefas();
    }

    private void OnFocus()
    {
        GetPlayerPrefas();
    }

    private void OnGUI()
    {
        AddValueView();
        ValuesView();
    }

    private void AddValueView()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            addKey = EditorGUILayout.TextField(new GUIContent("key"), addKey);
            addValue = EditorGUILayout.TextField(new GUIContent("value"), addValue);
            addType = (ValueType)EditorGUILayout.EnumPopup("type", addType);

            if (GUILayout.Button("“o˜^"))
            {
                if (string.IsNullOrEmpty(addKey) || string.IsNullOrEmpty(addValue))
                    goto SkipAddPrefas;

                Debug.Log($"Add PlayerPrefs key: {addKey}, value: {addValue}");
                switch (addType)
                {
                    case ValueType.Int:
                        break;
                    case ValueType.Float:
                        break;
                    case ValueType.String:
                        PlayerPrefs.SetString(addKey, addValue);
                        break;
                    default:
                        break;
                }

                PlayerPrefs.Save();
                GetPlayerPrefas();
            
            SkipAddPrefas:;
            }
        }
    }

    Vector2 scroll = Vector2.zero;
    private void ValuesView()
    {
        const int keyLabelWidth = 100;
        const int keyTypeLabelWidth = 100;

        using (new EditorGUILayout.VerticalScope(GUI.skin.window))
        using (var scrollScope = new EditorGUILayout.ScrollViewScope(scroll))
        {
            scroll = scrollScope.scrollPosition;
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("key", GUILayout.Width(keyLabelWidth));
                EditorGUILayout.LabelField("value", GUILayout.Width(keyTypeLabelWidth));
                EditorGUILayout.LabelField("value type");
            }

            foreach (var (key, value, valType) in prefsCaches)
            {
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField(key, GUILayout.Width(keyLabelWidth));
                    EditorGUILayout.LabelField(valType.ToString(), GUILayout.Width(keyTypeLabelWidth));
                    EditorGUILayout.LabelField(value);
                }
            }
        }
    }

    private void GetPlayerPrefas()
    {
        (string, string, ValueType) GetValue(string x)
        {
            string result = string.Empty;

            result = PlayerPrefs.GetString(x, null);
            if (result != null)
                return (x, result, ValueType.String);

            result = PlayerPrefs.GetInt(x, 0).ToString();
            if (result != "0")
                return (x, result, ValueType.Int);

            result = PlayerPrefs.GetFloat(x, 0).ToString();
            if (result != "0")
                return (x, result, ValueType.Float);

            return (x, null, ValueType.String);
        }

        caches.Clear();

        var keys = GetPlayerPrefasKeys();
        prefsCaches = keys.Select(key => GetValue(key)).ToList();
    }

    private List<string> GetPlayerPrefasKeys()
    {
#if UNITY_STANDALONE_WIN
        // Unity stores prefs in the registry on Windows
        string regKeyPathPattern =
#if UNITY_EDITOR
        @"Software\Unity\UnityEditor\{0}\{1}";
#else
		@"Software\{0}\{1}";
#endif
        ;

        string regKeyPath = string.Format(regKeyPathPattern, UnityEditor.PlayerSettings.companyName, UnityEditor.PlayerSettings.productName);
        Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(regKeyPath);
        if (regKey == null)
        {
            return new List<string>();
        }

        string[] playerPrefKeys = regKey.GetValueNames();
        List<string> result = new List<string>();
        for (int i = 0; i < playerPrefKeys.Length; i++)
        {
            string playerPrefKey = playerPrefKeys[i];

            // Remove the _hXXXXX suffix
            playerPrefKey = playerPrefKey.Substring(0, playerPrefKey.LastIndexOf("_h"));

            var isAdd = true;
            foreach(var nonTargetKey in NON_TARGET_KEY)
                if (playerPrefKey == nonTargetKey) isAdd = false;

            if (isAdd) result.Add(playerPrefKey);
        }
#endif
        return result;
    }
}
