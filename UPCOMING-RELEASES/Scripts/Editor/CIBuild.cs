using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CIBuild
{
    public static void Build()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            string versionCode = GetArg(args, "-versionCode");
            string versionName = GetArg(args, "-versionName");
            string keystorePath = GetArg(args, "-keystorePath");
            string keystorePass = GetArg(args, "-keystorePass");
            string keyAlias = GetArg(args, "-keyAlias");
            string keyPass = GetArg(args, "-keyPass");

            string rawDir = GetArg(args, "-outputPath") ?? "Build/Android";
            string outputDir = Path.GetFullPath(rawDir);

            string bumpType = GetArg(args, "-bumpType") ?? "none";

            if (bumpType != "none")
            {
                ApplyVersionBump(bumpType);
                AssetDatabase.SaveAssets();
            }

            string buildName = $"v{PlayerSettings.bundleVersion}({PlayerSettings.Android.bundleVersionCode})";
            string outputPath = Path.Combine(outputDir, $"{buildName}.aab");

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                Debug.Log($"[CIBuild] Created output directory: {outputDir}");
            }

            if (!string.IsNullOrEmpty(versionCode))
                PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);

            if (!string.IsNullOrEmpty(versionName))
                PlayerSettings.bundleVersion = versionName;

            if (!string.IsNullOrEmpty(keystorePath))
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = keystorePath;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = keyAlias;
                PlayerSettings.Android.keyaliasPass = keyPass;
            }

            // Prevent duplicate Addressables build (already built in separate CI step)
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
                settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;

            EditorUserBuildSettings.buildAppBundle = true;

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[CIBuild] Build failed: {report.summary.result}");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log($"[CIBuild] Build succeeded: {outputPath} (v{PlayerSettings.bundleVersion}, vcode {PlayerSettings.Android.bundleVersionCode})");
                EditorApplication.Exit(0);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CIBuild] Exception: {e}");
            EditorApplication.Exit(1);
        }
    }

    static void ApplyVersionBump(string bumpType)
    {
        string current = PlayerSettings.bundleVersion;
        string[] parts = current.Split('.');
        if (parts.Length != 3)
        {
            Debug.LogWarning($"[CIBuild] bundleVersion '{current}' not semver — skipping bump");
            return;
        }

        int major = int.Parse(parts[0]);
        int minor = int.Parse(parts[1]);
        int patch = int.Parse(parts[2]);

        switch (bumpType)
        {
            case "major": major++; minor = 0; patch = 0; break;
            case "minor": minor++; patch = 0; break;
            case "patch": patch++; break;
            default: return;
        }

        PlayerSettings.bundleVersion = $"{major}.{minor}.{patch}";
        PlayerSettings.Android.bundleVersionCode++;
        Debug.Log($"[CIBuild] Version bumped: {current} → {PlayerSettings.bundleVersion} (vcode {PlayerSettings.Android.bundleVersionCode})");
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
