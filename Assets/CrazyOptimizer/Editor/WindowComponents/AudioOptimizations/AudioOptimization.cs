using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CrazyGames.TreeLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CrazyGames.WindowComponents.AudioOptimizations
{
    public class AudioOptimization : EditorWindow
    {
        private static MultiColumnHeaderState _multiColumnHeaderState;
        private static AudioTree _audioCompressionTree;

        private static bool _isAnalyzing;
        private static bool _includeFilesFromPackages;

        public static void RenderGUI()
        {
            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(300));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Press \"Analyze audio\" button to load the table.");
            GUILayout.Label("Press it again when you need to refresh the data.");
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            _audioCompressionTree?.OnGUI(rect);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_isAnalyzing ? "Analyzing..." : "Analyze audio", GUILayout.Width(200)))
            {
                AnalyzeAudio();
            }

            var originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 160;
            _includeFilesFromPackages = EditorGUILayout.Toggle("Include files from Packages", _includeFilesFromPackages);
            EditorGUIUtility.labelWidth = originalValue;
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            // GUILayout.Label(
            //     "This utility gives you an overview of the textures used in your project. By optimizing various settings, you will be able to considerably decrease your final build size. You can click on a texture to select it in the Project view. To find out more about how the tool finds the textures, please check our GitHub repo.",
            //     EditorStyles.wordWrappedLabel);
            //
            //
            // BuildExplanation("Max size",
            //     "Decrease the max size as much as possible while the texture still looks good in game. You most likely don't need the default 2048 set by Unity.");
            // BuildExplanation("Compression", "Lower quality will decrease the final build size.");
            // BuildExplanation("Crunch compression",
            //     "All the textures with crunch compression enabled will be compressed together, decreasing the final build size.");
            // BuildExplanation("Crunch comp. quality", "A higher compression quality means larger textures and longer compression times.");
        }

        static void BuildExplanation(string label, string explanation)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Width(130));
            GUILayout.Label(
                explanation,
                EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }


        /**
         * Find recursively the audio clips on which this scene depends.
         */
        static List<string> GetSceneAudioDependencies(string scenePath)
        {
            var audioDependencies = new List<string>();
            var assetDependencies = AssetDatabase.GetDependencies(scenePath, true);
            foreach (var assetDependency in assetDependencies)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(assetDependency) == typeof(AudioClip))
                {
                    audioDependencies.Add(assetDependency);
                }
            }

            return audioDependencies;
        }

        static List<string> GetUsedAudioInBuildScenes()
        {
            var usedAudioPaths = new HashSet<string>();

            var scenesInBuild = OptimizerUtils.GetScenesInBuildPath();
            foreach (var scenePath in scenesInBuild)
            {
                var audioUsedInScene = GetSceneAudioDependencies(scenePath);
                foreach (var audioPath in audioUsedInScene)
                {
                    usedAudioPaths.Add(audioPath);
                }
            }

            return usedAudioPaths.ToList();
        }

        /**
         * Get the list of audio clips in the Resources folders, or on which assets from the Resources folder depend.
         */
        static List<string> GetUsedAudioInResources()
        {
            var usedAudioPaths = new HashSet<string>();
            var allAssetPaths = AssetDatabase.FindAssets("", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).ToList();

            // keep only the assets inside a Resources folder, that is not inside an Editor folder
            var rx = new Regex(@"\w*(?<!Editor\/)Resources\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            allAssetPaths = allAssetPaths.Where(assetPath => (rx.IsMatch(assetPath))).ToList();

            // find all the audio clips on which the assets from the Resources folder depend
            foreach (var assetPath in allAssetPaths)
            {
                var assetDependencies = AssetDatabase.GetDependencies(assetPath, true);
                foreach (var assetDependency in assetDependencies)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(assetDependency) == typeof(AudioClip))
                    {
                        usedAudioPaths.Add(assetDependency);
                    }
                }
            }

            return usedAudioPaths.ToList();
        }

        static void AnalyzeAudio()
        {
            _isAnalyzing = true;
            if (OptimizerWindow.EditorWindowInstance != null)
            {
                OptimizerWindow.EditorWindowInstance.Repaint();
            }

            var usedAudioPaths = new HashSet<string>();

            GetUsedAudioInBuildScenes().ForEach(path => usedAudioPaths.Add(path));
            GetUsedAudioInResources().ForEach(path => usedAudioPaths.Add(path));

            var treeElements = new List<AudioTreeItem>();
            var idIncrement = 0;
            var root = new AudioTreeItem("Root", -1, idIncrement, null, null);
            treeElements.Add(root);

            foreach (var audioPath in usedAudioPaths)
            {
                if (audioPath.StartsWith("Packages/") && !_includeFilesFromPackages)
                {
                    continue;
                }

                idIncrement++;
                try
                {
                    var audioImporter = (AudioImporter)AssetImporter.GetAtPath(audioPath);
                    treeElements.Add(new AudioTreeItem("AudioClip", 0, idIncrement, audioPath, audioImporter));
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Failed to analyze audio clip at path: " + audioPath);
                }
            }

            var treeModel = new TreeModel<AudioTreeItem>(treeElements);
            var treeViewState = new TreeViewState();
            if (_multiColumnHeaderState == null)
                _multiColumnHeaderState = new MultiColumnHeaderState(new[]
                {
                    // when adding a new column don't forget to check the sorting method, and the CellGUI method
                    new MultiColumnHeaderState.Column() { headerContent = new GUIContent() { text = "Audio clip" }, width = 150, minWidth = 150, canSort = true },
                    // new MultiColumnHeaderState.Column() { headerContent = new GUIContent() { text = "Type" }, width = 60, minWidth = 60, canSort = true },
                    // new MultiColumnHeaderState.Column() { headerContent = new GUIContent() { text = "Max size" }, width = 60, minWidth = 60, canSort = true },
                    // new MultiColumnHeaderState.Column()
                    //     { headerContent = new GUIContent() { text = "Compression" }, width = 80, minWidth = 80, canSort = true },
                    // new MultiColumnHeaderState.Column()
                    //     { headerContent = new GUIContent() { text = "Crunch compression" }, width = 120, minWidth = 120, canSort = true },
                    // new MultiColumnHeaderState.Column()
                    //     { headerContent = new GUIContent() { text = "Crunch comp. quality" }, width = 128, minWidth = 128, canSort = true },
                });
            _audioCompressionTree = new AudioTree(treeViewState, new MultiColumnHeader(_multiColumnHeaderState), treeModel);
            _isAnalyzing = false;
            if (OptimizerWindow.EditorWindowInstance != null)
            {
                OptimizerWindow.EditorWindowInstance.Repaint();
            }
        }
    }
}