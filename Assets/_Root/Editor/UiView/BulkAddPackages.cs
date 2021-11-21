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
    //         internal static void ManageRegistries() { Client.Add("com.halodi.halodi-unity-package-creator"); }
    //     }
    // }
    public class BulkAddPackages : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Dictionary<string, Dictionary<string, List<string>>> _scopedData = new();
        private bool[] _foldoutScopes;
        private List<bool[]> _foldoutPackages; // scoped has many package
        private List<List<bool[]>> _foldoutVersions; //scope has manay package, packge has many version

        [MenuItem("Packages/Add packages (bulk)", false, 22)]
        internal static void ManageRegistries() { GetWindow<BulkAddPackages>(true, "Add packages", true); }

        private void OnEnable()
        {
            minSize = new Vector2(640, 320);
            _scopedData = GithubResponse.GetAllPackages();
            int numberRegistry = _scopedData.Keys.Count;
            _foldoutScopes = new bool[numberRegistry];
            _foldoutPackages = new List<bool[]>(numberRegistry);
            _foldoutVersions = new List<List<bool[]>>(numberRegistry);

            for (var i = 0; i < numberRegistry; i++)
            {
                string keyRegistry = _scopedData.Keys.ToList()[i];
                int numberPackageInScope = _scopedData[keyRegistry].Keys.Count;
                Debug.Log(numberPackageInScope);
                _foldoutPackages.Add(new bool[numberPackageInScope]);
                _foldoutVersions.Add(new List<bool[]>(numberPackageInScope));

                for (var j = 0; j < numberPackageInScope; j++)
                {
                    string namePackage = _scopedData[keyRegistry].Keys.ToList()[j];
                    _foldoutVersions[i].Add(new bool[_scopedData[keyRegistry][namePackage].Count]);
                }
            }

            Debug.Log("OnEnable");
        }

        private void OnGUI()
        {
            void ResetState(List<List<bool[]>> stateVersion, int i, int j, int k)
            {
                for (int l = 0; l < stateVersion.Count; l++)
                {
                    if (l != i)
                    {
                        for (var m = 0; m < stateVersion[l].Count; m++)
                        {
                            for (var n = 0; n < stateVersion[l][m].Length; n++)
                            {
                                stateVersion[l][m][n] = false;
                            }
                        }
                    }
                    else
                    {
                        for (var m = 0; m < stateVersion[l].Count; m++)
                        {
                            if (m != j)
                            {
                                for (var n = 0; n < stateVersion[i][m].Length; n++)
                                {
                                    stateVersion[l][m][n] = false;
                                }
                            }
                            else
                            {
                                for (int n = 0; n < stateVersion[i][j].Length; n++)
                                {
                                    if (n != k)
                                    {
                                        stateVersion[l][m][n] = false;
                                    }
                                    else
                                    {
                                        stateVersion[i][j][k] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

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
                            int k = 0;
                            foreach (string version in _scopedData[key][pacakgeName])
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                _foldoutVersions[index][j][k] = EditorGUILayout.Toggle(version, _foldoutVersions[index][j][k]);
                                if (_foldoutVersions[index][j][k])
                                {
                                    ResetState(_foldoutVersions, index, j, k);
                                }

                                ++k;
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