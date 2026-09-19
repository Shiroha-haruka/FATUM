using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameLanguage
{
    Japanese,
    English
}

public class LocalizationManager : MonoBehaviour
{
    private const string TableResourcePath = "Localization/texts";

    private class LocalizedEntry
    {
        public string Japanese;
        public string English;
    }

    public static LocalizationManager Instance { get; private set; }

    public GameLanguage CurrentLanguage { get; private set; } = GameLanguage.Japanese;

    private readonly Dictionary<string, LocalizedEntry> entries =
        new Dictionary<string, LocalizedEntry>();

    private readonly Dictionary<string, string> sourceToKey =
        new Dictionary<string, string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("LocalizationManager");
        managerObject.AddComponent<LocalizationManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadTable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LocalizeSceneText();
    }

    public void SetLanguage(GameLanguage language)
    {
        CurrentLanguage = language;
        LocalizeSceneText();
    }

    public string Get(string key)
    {
        return Get(key, CurrentLanguage);
    }

    public string Get(string key, GameLanguage language)
    {
        if (!entries.TryGetValue(key, out LocalizedEntry entry))
        {
            Debug.LogWarning($"Localization key was not found: {key}");
            return key;
        }

        return language == GameLanguage.English ? entry.English : entry.Japanese;
    }

    public string GetFormat(string key, params object[] arguments)
    {
        return string.Format(Get(key), arguments);
    }

    public string LocalizeSource(string source)
    {
        if (string.IsNullOrEmpty(source) || !sourceToKey.TryGetValue(source.Trim(), out string key))
        {
            return source;
        }

        return Get(key);
    }

    private void LocalizeSceneText()
    {
        TMP_Text[] textObjects = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include);

        foreach (TMP_Text textObject in textObjects)
        {
            if (textObject != null)
            {
                textObject.text = LocalizeSource(textObject.text);
            }
        }
    }

    private void LoadTable()
    {
        TextAsset table = Resources.Load<TextAsset>(TableResourcePath);

        if (table == null)
        {
            Debug.LogError($"Localization table was not found at Resources/{TableResourcePath}.csv");
            return;
        }

        List<List<string>> rows = ParseCsv(table.text);

        for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            List<string> row = rows[rowIndex];

            if (row.Count < 4 || string.IsNullOrWhiteSpace(row[0]))
            {
                continue;
            }

            string key = row[0].Trim();
            LocalizedEntry entry = new LocalizedEntry
            {
                Japanese = row[2].Replace("\\n", "\n"),
                English = row[3].Replace("\\n", "\n")
            };

            entries[key] = entry;
            AddSource(entry.Japanese, key);
            AddSource(entry.English, key);
        }
    }

    private void AddSource(string source, string key)
    {
        if (!string.IsNullOrWhiteSpace(source))
        {
            sourceToKey[source.Trim()] = key;
        }
    }

    private static List<List<string>> ParseCsv(string csv)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> row = new List<string>();
        StringBuilder field = new StringBuilder();
        bool insideQuotes = false;

        for (int index = 0; index < csv.Length; index++)
        {
            char character = csv[index];

            if (character == '"')
            {
                if (insideQuotes && index + 1 < csv.Length && csv[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (character == ',' && !insideQuotes)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if ((character == '\n' || character == '\r') && !insideQuotes)
            {
                if (character == '\r' && index + 1 < csv.Length && csv[index + 1] == '\n')
                {
                    index++;
                }

                row.Add(field.ToString());
                field.Clear();

                if (row.Count > 1 || !string.IsNullOrWhiteSpace(row[0]))
                {
                    rows.Add(row);
                }

                row = new List<string>();
            }
            else
            {
                field.Append(character);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }
}
