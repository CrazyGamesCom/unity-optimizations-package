using System.Collections.Generic;
using CrazyGames.TreeLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CrazyGames.WindowComponents.TextureOptimizations
{
    public class TextureOptimization : EditorWindow
    {
        private static MultiColumnHeaderState _multiColumnHeaderState;
        private static MultiColumnTree _textureCompressionTree;


        public static void RenderGUI()
        {
            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(300));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Press \"Analyze textures\" button to load the table.");
            GUILayout.Label("Press it again when you need to refresh the data.");
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            _textureCompressionTree?.OnGUI(rect);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Analyze textures"))
            {
                AnalyzeTextures();
            }

            EditorGUILayout.Space(5);

            GUILayout.Label(
                "This utility gives you an overview of all the textures in your project. By optimizing various settings you will be able to considerably decrease your final build size. You can click on a texture to select it in the Project view.",
                EditorStyles.wordWrappedLabel);


            BuildExplanation("Max size",
                "Decrease the max size as much as possible while the texture still looks good in game. You most likely don't need the default 2048 set by Unity.");
            BuildExplanation("Compression", "Lower quality will decrease the final build size.");
            BuildExplanation("Crunch compression",
                "All the textures with crunch compression enabled will be compressed together, decreasing the final build size.");
            BuildExplanation("Crunch comp. quality", "A higher compression quality means larger textures and longer compression times.");
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


        static void AnalyzeTextures()
        {
            var treeElements = new List<TextureTreeItem>();
            var textureGuids = AssetDatabase.FindAssets("t:texture2D", new[] {"Assets"});
            var idIncrement = 0;
            var root = new TextureTreeItem("Root", -1, idIncrement, null, null);
            treeElements.Add(root);

            foreach (var guid in textureGuids)
            {
                idIncrement++;
                var texturePath = AssetDatabase.GUIDToAssetPath(guid);
                var textureImporter = (TextureImporter) AssetImporter.GetAtPath(texturePath);
                treeElements.Add(new TextureTreeItem("Texture2D", 0, idIncrement, texturePath, textureImporter));
            }

            var treeModel = new TreeModel<TextureTreeItem>(treeElements);
            var treeViewState = new TreeViewState();
            if (_multiColumnHeaderState == null)
                _multiColumnHeaderState = new MultiColumnHeaderState(new[]
                {
                    // when adding a new column don't forget to check the sorting method, and the CellGUI method
                    new MultiColumnHeaderState.Column() {headerContent = new GUIContent() {text = "Texture"}, width = 150, minWidth = 150, canSort = true},
                    new MultiColumnHeaderState.Column() {headerContent = new GUIContent() {text = "Type"}, width = 60, minWidth = 60, canSort = true},
                    new MultiColumnHeaderState.Column() {headerContent = new GUIContent() {text = "Max size"}, width = 60, minWidth = 60, canSort = true},
                    new MultiColumnHeaderState.Column() {headerContent = new GUIContent() {text = "Compression"}, width = 80, minWidth = 80, canSort = true},
                    new MultiColumnHeaderState.Column()
                        {headerContent = new GUIContent() {text = "Crunch compression"}, width = 120, minWidth = 120, canSort = true},
                    new MultiColumnHeaderState.Column()
                        {headerContent = new GUIContent() {text = "Crunch comp. quality"}, width = 128, minWidth = 128, canSort = true},
                });
            _textureCompressionTree = new MultiColumnTree(treeViewState, new MultiColumnHeader(_multiColumnHeaderState), treeModel);
        }
    }
}