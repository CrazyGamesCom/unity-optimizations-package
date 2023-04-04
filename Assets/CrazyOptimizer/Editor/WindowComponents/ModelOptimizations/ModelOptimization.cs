using CrazyGames.TreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CrazyGames.WindowComponents.ModelOptimizations
{
    public class ModelOptimization : EditorWindow
    {
        private static MultiColumnHeaderState _multiColumnHeaderState;
        private static ModelTree _modelTree;

        private static bool _isAnalyzing;
        private static bool _includeFilesFromPackages;

        private static readonly List<string> _modelFormats = new List<string>() { ".fbx", ".dae", ".3ds", ".dxf", ".obj" };

        public static void RenderGUI()
        {
            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(300));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Press \"Analyze models\" button to load the table.");
            GUILayout.Label("Press it again when you need to refresh the data.");
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            _modelTree?.OnGUI(rect);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_isAnalyzing ? "Analyzing..." : "Analyze models", GUILayout.Width(200)))
            {
                AnalyzeModels();
            }

            var originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 160;
            _includeFilesFromPackages = EditorGUILayout.Toggle("Include files from Packages", _includeFilesFromPackages);
            EditorGUIUtility.labelWidth = originalValue;
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            GUILayout.Label(
                "This utility gives you an overview of the models used in your project. By optimizing various settings, you will be able to considerably decrease your final build size. You can click on a model to select it in the Project view. To find out more about how the tool finds the models, please check our GitHub repo.",
                EditorStyles.wordWrappedLabel);

            BuildExplanation("R/W enabled",
                "When a Mesh is read/write enabled, Unity uploads the Mesh data to GPU-addressable memory, but also keeps it in CPU-addressable memory. In most cases, you should disable this option to save runtime memory usage.");
            BuildExplanation("Polygons optimized",
                "Optimize the order of polygons in the mesh to make better use of the GPUs internal caches to improve rendering performance.");
            BuildExplanation("Vertices optimized",
                "Optimize the order of vertices in the mesh to make better use of the GPUs internal caches to improve rendering performance.");
            BuildExplanation("Mesh compression",
                "Compressing meshes will decrease the final build size, but more compression introduces more artifacts in vertex data.");
            BuildExplanation("Animation compression",
                "Compressing animations will decrease the final build size, but more compression introduces more artifacts in the animations.");
        }

        static void BuildExplanation(string label, string explanation)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Width(130));
            GUILayout.Label(explanation, EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        /**
         * Find recursively the models on which this scene depends.
         */
        static List<string> GetSceneModelDependencies(string scenePath)
        {
            var modelDependencies = new List<string>();
            var assetDependencies = AssetDatabase.GetDependencies(scenePath, true);

            foreach (var assetDependency in assetDependencies)
            {
                if (IsModelAtPath(assetDependency))
                {
                    modelDependencies.Add(assetDependency);
                }
            }

            return modelDependencies;
        }

        static List<string> GetUsedModelsInBuildScenes()
        {
            var usedModelPaths = new HashSet<string>();
            var scenesInBuild = OptimizerUtils.GetScenesInBuildPath();

            foreach (var scenePath in scenesInBuild)
            {
                var modelsUsedInScene = GetSceneModelDependencies(scenePath);

                foreach (var modelPath in modelsUsedInScene)
                {
                    usedModelPaths.Add(modelPath);
                }
            }

            return usedModelPaths.ToList();
        }

        /**
         * Get the list of models in the Resources folders, or on which assets from the Resources folder depend.
         */
        static List<string> GetUsedModelsInResources()
        {
            var usedModelPaths = new HashSet<string>();
            var allAssetPaths = AssetDatabase.FindAssets("", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).ToList();

            // keep only the assets inside a Resources folder, that is not inside an Editor folder
            var rx = new Regex(@"\w*(?<!Editor\/)Resources\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            allAssetPaths = allAssetPaths.Where(assetPath => (rx.IsMatch(assetPath))).ToList();

            // find all the models on which the assets from the Resources folder depend
            foreach (var assetPath in allAssetPaths)
            {
                var assetDependencies = AssetDatabase.GetDependencies(assetPath, true);

                foreach (var assetDependency in assetDependencies)
                {
                    if (IsModelAtPath(assetDependency))
                    {
                        string ext = System.IO.Path.GetExtension(assetDependency);
                        usedModelPaths.Add(assetDependency);
                    }
                }
            }

            return usedModelPaths.ToList();
        }

        static bool IsModelAtPath(string assetDependency)
        {
            return AssetDatabase.GetMainAssetTypeAtPath(assetDependency) == typeof(GameObject) &&
                   _modelFormats.Contains(System.IO.Path.GetExtension(assetDependency).ToLowerInvariant());
        }

        static void AnalyzeModels()
        {
            _isAnalyzing = true;

            if (OptimizerWindow.EditorWindowInstance != null)
            {
                OptimizerWindow.EditorWindowInstance.Repaint();
            }

            var usedModelPaths = new HashSet<string>();

            GetUsedModelsInBuildScenes().ForEach(path => usedModelPaths.Add(path));
            GetUsedModelsInResources().ForEach(path => usedModelPaths.Add(path));

            var treeElements = new List<ModelTreeItem>();
            var idIncrement = 0;
            var root = new ModelTreeItem("Root", -1, idIncrement, null, null);
            treeElements.Add(root);

            foreach (var modelPath in usedModelPaths)
            {
                if (modelPath.StartsWith("Packages/") && !_includeFilesFromPackages)
                {
                    continue;
                }

                idIncrement++;

                try
                {
                    var modelImporter = (ModelImporter)AssetImporter.GetAtPath(modelPath);
                    treeElements.Add(new ModelTreeItem("Model", 0, idIncrement, modelPath, modelImporter));
                }
                catch (Exception)
                {
                    Debug.LogWarning("Failed to analyze model at path: " + modelPath);
                }
            }

            var treeModel = new TreeModel<ModelTreeItem>(treeElements);
            var treeViewState = new TreeViewState();

            if (_multiColumnHeaderState == null)
                _multiColumnHeaderState = new MultiColumnHeaderState(new[]
                {
                    // when adding a new column don't forget to check the sorting method, and the CellGUI method
                    new MultiColumnHeaderState.Column() { headerContent = new GUIContent() { text = "Model" }, width = 150, minWidth = 150, canSort = true },
                    new MultiColumnHeaderState.Column()
                        { headerContent = new GUIContent() { text = "R/W enabled" }, width = 80, minWidth = 80, canSort = true },
                    new MultiColumnHeaderState.Column()
                        { headerContent = new GUIContent() { text = "Polygons optimized" }, width = 120, minWidth = 120, canSort = true },
                    new MultiColumnHeaderState.Column()
                        { headerContent = new GUIContent() { text = "Vertices optimized" }, width = 120, minWidth = 120, canSort = true },
                    new MultiColumnHeaderState.Column()
                        { headerContent = new GUIContent() { text = "Mesh compression" }, width = 120, minWidth = 120, canSort = true },
                    new MultiColumnHeaderState.Column()
                        { headerContent = new GUIContent() { text = "Animation compression" }, width = 140, minWidth = 140, canSort = true },
                });

            _modelTree = new ModelTree(treeViewState, new MultiColumnHeader(_multiColumnHeaderState), treeModel);
            _isAnalyzing = false;

            if (OptimizerWindow.EditorWindowInstance != null)
            {
                OptimizerWindow.EditorWindowInstance.Repaint();
            }
        }
    }
}