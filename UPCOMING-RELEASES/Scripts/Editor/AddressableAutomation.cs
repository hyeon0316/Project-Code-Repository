using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public static class AddressableAutomation
{
    public static void BuildFromCLI()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            string profileName = GetArg(args, "-addressableProfile") ?? "Develop";

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[Addressable] AddressableAssetSettings not found");
                EditorApplication.Exit(1);
                return;
            }

            string profileId = settings.profileSettings.GetProfileId(profileName);
            if (string.IsNullOrEmpty(profileId))
            {
                Debug.LogError($"[Addressable] Profile not found: {profileName}");
                EditorApplication.Exit(1);
                return;
            }

            settings.activeProfileId = profileId;
            Debug.Log($"[Addressable] Profile set: {profileName}");

            AddressableAssetSettings.CleanPlayerContent(null);

            AddressablesPlayerBuildResult result;
            AddressableAssetSettings.BuildPlayerContent(out result);

            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"[Addressable] Build failed: {result.Error}");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log($"[Addressable] Build succeeded. Duration: {result.Duration:F1}s");
                EditorApplication.Exit(0);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Addressable] Exception: {e}");
            EditorApplication.Exit(1);
        }
    }

    static string GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return null;
    }
}
