using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class Localize
{
    public const string PREFS_LANG_KEY = "localize.lang";
    private const string TAG = nameof(Localize);
    private const string SYSTEM_ADDRESSABLE_KEY = "Assets/Jsons/Localize_System.json";
    private const string DIALOGUE_ADDRESSABLE_KEY = "Assets/Jsons/Localize_Dialogue.json";
    private const string TERMS_ADDRESSABLE_KEY = "Assets/Jsons/Localize_Terms.json";
    private const string BUILT_IN_ADDRESSABLE_KEY = "Assets/Jsons/Localize_Built_In.json";

    public static Action OnLanguageChanged;
    public static bool HasInit { get; private set; }
    public static bool HasBuiltInInit { get; private set; }
    public static string LanguageCode { get => selectLanguageCode; set => SetLanguageCode(value); }
    public static IReadOnlyDictionary<string, string> Datas => datas;

    private static string selectLanguageCode = "ko";
    private static List<Dictionary<string, string>> rawRows = new();
    private static readonly Dictionary<string, string> datas = new();
    private static readonly Regex regexSplitParseTarget = new Regex(@"([\w\-_\.]+),?(-?\d+)?:?(.*)");

    public static async UniTask InitBuiltInAsync()
    {
        if (HasBuiltInInit)
            return;

        HasBuiltInInit = true;

        selectLanguageCode = LanguageUtil.GetLanguage();

        Debug.Log($"[{TAG}] InitBuiltInAsync()");

        await LoadTableAsync(BUILT_IN_ADDRESSABLE_KEY);

        RebuildDataTable();
    }

    public static async UniTask InitAsync()
    {
        if (HasInit)
            return;

        HasInit = true;

        selectLanguageCode = LanguageUtil.GetLanguage();

        Debug.Log($"[{TAG}] InitAsync()");

        if (!HasBuiltInInit)
            await InitBuiltInAsync();

        await LoadTableAsync(SYSTEM_ADDRESSABLE_KEY);
        await LoadTableAsync(DIALOGUE_ADDRESSABLE_KEY);
        await LoadTableAsync(TERMS_ADDRESSABLE_KEY);

        RebuildDataTable();
    }

    private static async UniTask LoadTableAsync(string addressableKey)
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(addressableKey);
        var textAsset = await handle.Task;

        if (textAsset != null)
        {
            ParseJson(textAsset.text);
        }
        else
        {
            Debug.LogError($"[{TAG}] Failed to load {addressableKey}");
        }

        Addressables.Release(handle);
    }

    private static void ParseJson(string json)
    {
        var rows = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json) ?? new();
        rawRows.AddRange(rows);
        Debug.Log($"[{TAG}] ParseJson() rows={rows.Count} total={rawRows.Count}");
    }

    public static void RebuildDataTable()
    {
        var lang = selectLanguageCode;

        Debug.Log($"[{TAG}] RebuildDataTable({lang})");

        datas.Clear();

        foreach (var row in rawRows)
        {
            if (!row.TryGetValue("Key", out var key) || string.IsNullOrEmpty(key)) continue;

            if (row.TryGetValue(lang, out var val) && !string.IsNullOrEmpty(val))
                datas[key] = val;
        }
    }

    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "";
        return datas.TryGetValue(key, out var val) ? val : key;
    }

    private static void SetLanguageCode(string languageCode)
    {
        Debug.Log($"[{TAG}] SetLanguage({languageCode})");
        LanguageUtil.SetLanguage(languageCode);

        if (HasInit)
            RebuildDataTable();
    }
}
