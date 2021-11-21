using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace com.snorlax.upm
{
    // using UnityEditor;
    // using UnityEditor.PackageManager;
    //
    // namespace com.snorlax.upm
    // {
    //     internal class InstallPackageCreatorView : EditorWindow
    //     {
    //         [MenuItem("Packages/Install Halodi Package Creator", false, 41)]
    //         internal static void ManageRegistries() { Client.Add("com.halodi.halodi-unity-package-creator"); }
    //     }
    // }
    public class BulkAddPackages : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Dictionary<string, Dictionary<string, List<string>>> _scopedData = new();
        private bool[] _foldoutScopes;
        private List<bool[]> _foldoutPackages;

        [MenuItem("Packages/Add packages (bulk)", false, 22)]
        internal static void ManageRegistries() { GetWindow<BulkAddPackages>(true, "Add packages", true); }

        private void OnEnable()
        {
            minSize = new Vector2(640, 320);
            _scopedData = GithubResponse.GetAllPackages();
            _foldoutScopes = new bool[_scopedData.Keys.Count];
            _foldoutPackages = new List<bool[]>(_foldoutScopes.Length);

            for (var i = 0; i < _foldoutScopes.Length; i++)
            {
                _foldoutPackages.Add(new bool[_scopedData[_scopedData.Keys.ToList()[i]].Keys.Count]);
            }

            Debug.Log("OnEnable");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            var index = 0;
            foreach (string key in _scopedData.Keys)
            {
                string scopeTitle = key;
                if (key.Equals("snorluxe")) scopeTitle = "Snorlax";
                _foldoutScopes[index] = EditorGUILayout.Foldout(_foldoutScopes[index], scopeTitle, true);
                EditorGUILayout.Separator();

                if (_foldoutScopes[index])
                {
                    EditorGUILayout.BeginVertical();

                    var j = 0;
                    foreach (string pacakgeName in _scopedData[key].Keys)
                    {
                        _foldoutPackages[index][j] = EditorGUILayout.Foldout(_foldoutPackages[index][j], pacakgeName, true);
                        if (_foldoutPackages[index][j])
                        {
                            EditorGUILayout.BeginVertical();
                            foreach (string version in _scopedData[key][pacakgeName])
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.LabelField(version);
                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.EndVertical();
                        }

                        ++j;
                    }

                    EditorGUILayout.EndVertical();
                }

                ++index;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Install", GUILayout.Width(80)))
            {
                AddPackages();
                CloseWindow();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddPackages()
        {
            // var result = "";
            //
            // var hasPackages = false;
            //
            // using (var reader = new StringReader(_packageList))
            // {
            //     string line;
            //     while ((line = reader.ReadLine()) != null)
            //     {
            //         if (!string.IsNullOrEmpty(line))
            //         {
            //             var request = Client.Add(line);
            //
            //             while (!request.IsCompleted)
            //             {
            //                 Thread.Sleep(100);
            //             }
            //
            //             if (request.Status == StatusCode.Success)
            //             {
            //                 result += "Imported: " + line + Environment.NewLine;
            //             }
            //             else
            //             {
            //                 result += "Cannot import " + line + ": " + request.Error.message + Environment.NewLine;
            //             }
            //
            //             hasPackages = true;
            //         }
            //     }
            // }
            //
            // if (hasPackages)
            // {
            //     EditorUtility.DisplayDialog("Added packages", "Packages added:" + Environment.NewLine + Environment.NewLine + result, "OK");
            // }
            // else
            // {
            //     EditorUtility.DisplayDialog("No packages entered", "No packages entered.", "OK");
            // }
        }

        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }
    }
}