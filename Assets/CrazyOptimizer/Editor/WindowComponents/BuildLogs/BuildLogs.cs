using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CrazyGames;
using CrazyGames.TreeLib;
using CrazyGames.WindowComponents.TextureOptimizations;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CrazyOptimizer.Editor.WindowComponents.BuildLogs
{
    public class BuildLogs
    {
        private static MultiColumnHeaderState _multiColumnHeaderState;
        private static BuildLogTree _buildLogTree;
        private static bool _isAnalyzing;
        private static string _errorMessage;
        private static bool _includeFilesFromPackages;

        public static void RenderGUI()
        {
            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(300));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Press \"Analyze build logs\" button, but be sure the project was built at least once on this machine.");
            GUILayout.Label("Press it again when you need to refresh the data.");
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            _buildLogTree?.OnGUI(rect);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(_isAnalyzing ? "Analyzing..." : "Analyze build logs", GUILayout.Width(200)))
            {
                AnalyzeBuildLogs();
            }

            var originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 160;
            _includeFilesFromPackages = EditorGUILayout.Toggle("Include files from Packages", _includeFilesFromPackages);
            EditorGUIUtility.labelWidth = originalValue;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Editor.log", GUILayout.Width(200)))
            {
                Process.Start("notepad.exe", GetEditorLogPath());
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_errorMessage))
            {
                GUILayout.Label(_errorMessage, new GUIStyle
                {
                    wordWrap = true,
                    normal =
                    {
                        textColor = Color.red
                    }
                });
            }

            EditorGUILayout.Space(5);

            GUILayout.Label(
                "This utility analyzes the Build Report from the Editor.log file. It will display all the files included in your final build, and the memory they occupy. You can use this utility to detect more opportunities to decrease the final build size. There may be textures that still occupy a lot of memory, uncompressed sounds, or stuff forgotten in the Resources folders that gets included in the build.",
                EditorStyles.wordWrappedLabel);
        }

        private static string GetEditorLogPath()
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = $@"{localAppDataPath}\Unity\Editor\Editor.log";
            return path;
        }

        /**
         * Return the contents of the Editor.log file.
         */
        private static string GetEditorLog()
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var originalEditorLogPath = GetEditorLogPath();
            var tempEditorLogPath = $@"{localAppDataPath}\Unity\Editor\EditorCrazyGamesTemp.log";
            // original file is blocked, perhaps by Unity editor. Need to copy it and read from the copied file.
            File.Copy(originalEditorLogPath, tempEditorLogPath, true);
            var editorLogStr = File.ReadAllText(tempEditorLogPath);
            File.Delete(tempEditorLogPath);
            return editorLogStr;
        }

        static void AnalyzeBuildLogs()
        {
            _isAnalyzing = true;
            _errorMessage = "";
            if (OptimizerWindow.EditorWindowInstance != null)
            {
                OptimizerWindow.EditorWindowInstance.Repaint();
            }
            string editorLogStr;
            try
            {
                editorLogStr = GetEditorLog();
            }
            catch (Exception e)
            {
                _errorMessage = "Failed to read Editor.log file, check console for more details.";
                Debug.LogError(e);
                return;
            }

            var buildReportStr = editorLogStr
                .Split(new[] {$"----------------------{Environment.NewLine}Build Report{Environment.NewLine}"}, StringSplitOptions.None).Last();
            if (!buildReportStr.StartsWith("Uncompressed usage by category"))
            {
                _errorMessage =
                    "Failed to find Build Report in the Editor.log file. Please be sure the project was recently built on this machine. If the error persists, feel free to contact us.";
                return;
            }

            // clear the lines until we reach the lines with files and the memory they occupy
            var buildReportLines = buildReportStr.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
            while (!buildReportLines[0].StartsWith("Used Assets and files from the Resources folder"))
            {
                buildReportLines.RemoveAt(0);
            }

            buildReportLines.RemoveAt(0);


            // start building the tree with the lines with files from the report
            var treeElements = new List<BuildLogTreeItem>();
            var idIncrement = 0;
            var root = new BuildLogTreeItem("Root", -1, idIncrement, 0, "", 0, "");
            treeElements.Add(root);

            while (!buildReportLines[0].StartsWith("------------"))
            {
                var line = buildReportLines[0];
                buildReportLines.RemoveAt(0);

                idIncrement++;
                var splitLine = line.Replace("\t", " ").Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                var size = float.Parse(splitLine[0], CultureInfo.InvariantCulture.NumberFormat);
                var sizeUnit = splitLine[1];
                var sizePercentage = float.Parse(splitLine[2].Replace("%", ""), CultureInfo.InvariantCulture.NumberFormat);
                var path = splitLine[3];

                if (path.StartsWith("Packages/") && !_includeFilesFromPackages)
                {
                    continue;
                }

                treeElements.Add(new BuildLogTreeItem("BuildLogLine", 0, idIncrement, size, sizeUnit, sizePercentage, path));
            }


            var treeModel = new TreeModel<BuildLogTreeItem>(treeElements);
            var treeViewState = new TreeViewState();
            _multiColumnHeaderState = _multiColumnHeaderState ?? new MultiColumnHeaderState(new[]
            {
                // when adding a new column don't forget to check the sorting method, and the CellGUI method
                new MultiColumnHeaderState.Column() {headerContent = new GUIContent() {text = "Size"}, width = 80, minWidth = 60, canSort = true},
                new MultiColumnHeaderState.Column() {headerContent = new GUIContent() {text = "Size %"}, width = 60, minWidth = 40, canSort = true},
                new MultiColumnHeaderState.Column() {headerContent = new GUIContent() {text = "Path"}, width = 300, minWidth = 200, canSort = true},
            });
            _buildLogTree = new BuildLogTree(treeViewState, new MultiColumnHeader(_multiColumnHeaderState), treeModel);
            _isAnalyzing = false;
            if (OptimizerWindow.EditorWindowInstance != null)
            {
                OptimizerWindow.EditorWindowInstance.Repaint();
            }
        }
    }
}