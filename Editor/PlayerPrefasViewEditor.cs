using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Cysharp.Threading.Tasks;

public sealed class PlayerPrefasViewEditor : EditorWindow
{
    private static readonly string[] NON_TARGET_KEY = { "unity.cloud_userid", "unity.player_sessionid", "unity.player_session_count", "UnityGraphicsQuality" };

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
        AddKeyWindow();

        using (new EditorGUILayout.VerticalScope())
        {
            foreach(var (key, value) in caches)
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(key);
                    EditorGUILayout.LabelField(value);
                }
        }
    }

    private void AddKeyWindow()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            addKey = EditorGUILayout.TextField(new GUIContent("key"), addKey);
            addValue = EditorGUILayout.TextField(new GUIContent("value"), addValue);
            addType = (ValueType)EditorGUILayout.EnumPopup("type", addType);

            if (GUILayout.Button("“o˜^"))
            {
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
            }
        }
    }

    private void GetPlayerPrefas()
    {
        string GetValue(string x)
        {
            string result = string.Empty;

            result = PlayerPrefs.GetString(x, null);
            if (result != null)
                return result;

            result = PlayerPrefs.GetInt(x, 0).ToString();
            if (result != "0")
                return result;

            result = PlayerPrefs.GetFloat(x, 0).ToString();
            if (result != "0")
                return result;

            return "value is not alive";
        }

        caches.Clear();

        var keys = GetPlayerPrefasKeys();
        foreach(var key in keys)
        {
            if (!caches.TryAdd(key, GetValue(key)))
                Debug.LogWarning($"Deplicate key as {key}");
        }
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
