using System;
using UnityEditor;
using UnityEngine;

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
                RenderItem("Brotli compression", compressionOk, fixCompression);
            }

            if (typeof(PlayerSettings.WebGL).GetProperty("nameFilesAsHashes") != null)
            {
                var nameAsHashesOk = PlayerSettings.WebGL.nameFilesAsHashes;
                Action fixNameAsHashes = () => { PlayerSettings.WebGL.nameFilesAsHashes = true; };
                RenderItem("Name file as hashes", nameAsHashesOk, fixNameAsHashes);
            }

            if (typeof(PlayerSettings.WebGL).GetProperty("exceptionSupport") != null)
            {
                var exceptionsOk = PlayerSettings.WebGL.exceptionSupport == WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
                Action fixExceptions = () => { PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly; };
                RenderItem("Exception support", exceptionsOk, fixExceptions,
                    "The \"Fix\" button sets exception support to \"Explicitly thrown exceptions only\". You can choose \"None\" in Player Settings for better performance, but first of all read about it on our developer documentation.");
            }

            if (typeof(PlayerSettings).GetProperty("stripEngineCode") != null)
            {
                var stripEngineCodeOk = PlayerSettings.stripEngineCode;
                Action fixStripEngineCode = () => { PlayerSettings.stripEngineCode = true; };
                RenderItem("Strip engine code", stripEngineCodeOk, fixStripEngineCode,
                    "To decrease the bundle size even more, you can select Medium or High stripping from Player Settings, but first of all read about them on our developer documentation.");
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Read more tips on our developer documentation"))
            {
                Application.OpenURL("https://developer.crazygames.com/unity-export-tips");
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
        private static void RenderItem(string optionName, bool ok, Action fixAction, string additionalInfo = null)
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
                GUILayout.Label("OK", okStyle, GUILayout.Width(30));
            else
                GUILayout.Label("FAIL", failStyle, GUILayout.Width(30));
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
                GUILayout.Space(30);
                GUILayout.Label(additionalInfo, additionalInfoStyle);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }
    }
}