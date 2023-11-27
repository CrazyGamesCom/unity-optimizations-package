using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrazyGames.WindowComponents
{
    public class ExportOptimizations
    {
        public static void RenderGUI()
        {
            if (typeof(PlayerSettings.WebGL).GetProperty("compressionFormat") != null)
            {
                var compressionOk = PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Brotli;
                Action fixCompression = () => { PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli; };
                RenderFixableItem("Brotli compression", compressionOk, fixCompression);
            }

            if (typeof(PlayerSettings.WebGL).GetProperty("nameFilesAsHashes") != null)
            {
                var nameAsHashesOk = PlayerSettings.WebGL.nameFilesAsHashes;
                Action fixNameAsHashes = () => { PlayerSettings.WebGL.nameFilesAsHashes = true; };
                RenderFixableItem("Name file as hashes", nameAsHashesOk, fixNameAsHashes);
            }

            if (typeof(PlayerSettings.WebGL).GetProperty("exceptionSupport") != null)
            {
                var exceptionsOk = PlayerSettings.WebGL.exceptionSupport == WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
                Action fixExceptions = () => { PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly; };
                RenderFixableItem("Exception support", exceptionsOk, fixExceptions,
                    "The \"Fix\" button sets exception support to \"Explicitly thrown exceptions only\". You can choose \"None\" in Player Settings for better performance, but first of all read about it on our developer documentation.");
            }

            if (typeof(PlayerSettings).GetProperty("stripEngineCode") != null)
            {
                var stripEngineCodeOk = PlayerSettings.stripEngineCode;
                Action fixStripEngineCode = () => { PlayerSettings.stripEngineCode = true; };
                RenderFixableItem("Strip engine code", stripEngineCodeOk, fixStripEngineCode,
                    "To decrease the bundle size even more, you can select Medium or High stripping from Player Settings, but first of all read about them on our developer documentation.");
            }


            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                RenderInfoItem(
                    "If you are using URP but don't use post-processing we recommend disabling them. This will reduce approximately 1mb from your final build size. Check our tips on the link below for more info.");
            }

#if UNITY_2021 || UNITY_2022
            // Unity is currently missing an API for accessing the GraphicsSettings preloaded shaders, so these need to be read from a serialized object
            var serializedGraphicsSettings = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
            var preloadedShadersCount = serializedGraphicsSettings.FindProperty("m_PreloadedShaders").arraySize;
            if (preloadedShadersCount > 0)
            {
                RenderInfoItem(
                    "Your project is preloading " + preloadedShadersCount + " shader(s). On WebGL, preloading shaders may considerably slow down the loading of the game.");
            }
#endif


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Read more tips on our developer documentation"))
            {
                Application.OpenURL("https://docs.crazygames.com/sdk/unity/resources/export-tips/");
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }


        /// <summary>
        /// Render OK/FAIL, option name, and "Fix" button.
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="ok">If the export option has the correct value</param>
        /// <param name="fixAction">Is called when the fix button is clicked.</param>
        /// <param name="additionalInfo">If specified, some additional info is displayed below label name</param>
        private static void RenderFixableItem(string optionName, bool ok, Action fixAction, string additionalInfo = null)
        {
            var okStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.green
                }
            };

            var failStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.red
                }
            };

            var labelStyle = new GUIStyle
            {
                normal =
                {
                    textColor = EditorStyles.label.normal.textColor
                }
            };
            var additionalInfoStyle = new GUIStyle
            {
                fontSize = 11,
                wordWrap = true,
                normal =
                {
                    textColor = EditorStyles.label.normal.textColor
                }
            };

            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (ok)
                GUILayout.Label("OK", okStyle, GUILayout.Width(35));
            else
                GUILayout.Label("FAIL", failStyle, GUILayout.Width(35));
            GUILayout.Label(optionName, labelStyle);
            GUILayout.FlexibleSpace();

            if (!ok && GUILayout.Button("Fix"))
            {
                fixAction();
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(35);
                GUILayout.Label(additionalInfo, additionalInfoStyle);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        private static void RenderInfoItem(string info)
        {
            var infoStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = new Color(0.1618f, 0.5568f, 1)
                }
            };
            var labelStyle = new GUIStyle
            {
                wordWrap = true,
                normal =
                {
                    textColor = EditorStyles.label.normal.textColor,
                }
            };

            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("INFO", infoStyle, GUILayout.Width(35));

            GUILayout.Label(info, labelStyle);
            GUILayout.FlexibleSpace();


            EditorGUILayout.EndHorizontal();


            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }
    }
}