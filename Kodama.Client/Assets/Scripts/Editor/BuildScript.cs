using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace YuankunHuang.Kodama.Editor
{
    public static class BuildScript
    {
        private const string BUILD_PATH = "Build/Kodama.Client.exe";

        [MenuItem("Build/Build Windows x64")]
        public static void BuildWindows()
        {
            // Parse command line arguments
            var version = GetArgument("-buildVersion") ?? "0.1.0";
            var scriptingBackend = GetArgument("-scriptingBackend") ?? "IL2CPP";
            var strippingLevel = GetArgument("-strippingLevel") ?? "Medium";
            var developmentBuild = GetArgument("-developmentBuild") ?? "false";
            var compression = GetArgument("-compression") ?? "LZ4";
            
            // Apply settings
            PlayerSettings.bundleVersion = version;
            
            // Scripting Backend
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, 
                scriptingBackend.ToUpperInvariant() == "IL2CPP" ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            
            // Stripping Level
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone, strippingLevel.ToLowerInvariant() switch
            {
                "minimal" => ManagedStrippingLevel.Minimal,
                "low" => ManagedStrippingLevel.Low,
                "medium" => ManagedStrippingLevel.Medium,
                "high" => ManagedStrippingLevel.High,
                _ => ManagedStrippingLevel.Medium
            });
            
            Debug.Log($"[Build] Version: {version}, Backend: {scriptingBackend}, Stripping: {strippingLevel}, Dev: {developmentBuild}, Compression: {compression}");
            
            string[] scenes = GetEnabledScenes();

            if (scenes.Length == 0)
            {
                Debug.LogError("No scenes found in Build Settings!");
                EditorApplication.Exit(1);
                return;
            }

            // Build options
            BuildOptions buildOptions = compression.ToUpperInvariant() == "LZ4HC" 
                ? BuildOptions.CompressWithLz4HC 
                : BuildOptions.CompressWithLz4;
            
            if (developmentBuild.ToLowerInvariant() == "true")
            {
                buildOptions |= BuildOptions.Development;
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BUILD_PATH,
                target = BuildTarget.StandaloneWindows64,
                options = buildOptions
            };

            Debug.Log($"Building to: {BUILD_PATH}");
            Debug.Log($"Scenes: {string.Join(", ", scenes)}");

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize / 1024 / 1024} MB");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build failed: {summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                            Debug.LogError(message.content);
                    }
                }

                EditorApplication.Exit(1);
            }
        }

        private static string GetArgument(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; ++i)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            var enabledScenes = new List<string>();

            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes.Add(scene.path);
                }
            }

            return enabledScenes.ToArray();
        }
    }
}