using System;
using CrazyGames.WindowComponents;
using CrazyGames.WindowComponents.TextureOptimizations;
using UnityEditor;
using UnityEngine;

namespace CrazyGames
{
    public class OptimizerWindow : EditorWindow
    {
        private int _toolbarInt = 0;
        private readonly string[] _toolbarStrings = {"Export", "Textures", "About"};

        [MenuItem("CrazyGames/WebGL Optimizer")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(OptimizerWindow), false, "WebGL Optimizer");
            window.minSize = new Vector2(800, 600);
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            _toolbarInt = GUILayout.Toolbar(_toolbarInt, _toolbarStrings);
            switch (_toolbarInt)
            {
                case 0:
                    ExportOptimizations.RenderGUI();
                    break;
                case 1:
                    TextureOptimization.RenderGUI();
                    break;
                case 2:
                    About.RenderGUI();
                    break;
            }

            GUILayout.Space(50);
            RenderCredits();
            EditorGUILayout.EndVertical();
        }

        void RenderCredits()
        {
            // don't render the about section when the package is integrated in CrazySDK
            if (Type.GetType("CrazyGames.SiteLock") != null)
                return;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var banner = Resources.Load<Texture>("CrazyGamesLogo");
            GUILayout.Box(banner, GUILayout.Height(45));

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Maintained by volunteers and CrazyGames.", EditorStyles.miniLabel);
            GUILayout.Label("We can make your game popular on the Web!", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("GitHub"))
                Application.OpenURL("https://github.com/CrazyGamesCom/unity-optimizations-package");
            GUILayout.Label("|");
            if (GUILayout.Button("Submit game on CrazyGames"))
                Application.OpenURL("https://developer.crazygames.com/");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}