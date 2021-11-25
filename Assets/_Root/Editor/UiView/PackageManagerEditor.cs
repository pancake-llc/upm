using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

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
    public class PackageManagerEditor : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Dictionary<string, Dictionary<string, List<string>>> _scopedData = new();
        private bool[] _foldoutScopes;
        private List<bool[]> _foldoutPackages; // scoped has many package
        private List<List<bool[]>> _foldoutVersions; //scope has manay package, packge has many version
        private List<PackageInfo> _packageInfos = new(); //local packages info (in package.json)
        private VersionPackage _versionPackageSelected;

        [MenuItem("Packages/Manage Pacakges &1", false, 22)]
        internal static void ManageRegistries() { GetWindow<PackageManagerEditor>(true, "Add packages", true); }

        private void OnEnable()
        {
            minSize = new Vector2(640, 320);
            _scopedData = GithubResponse.GetAllPackages();
            int numberRegistry = _scopedData.Keys.Count;
            _foldoutScopes = new bool[numberRegistry];
            // set default true for all scope to default is foldout
            for (var i = 0; i < _foldoutScopes.Length; i++)
            {
                _foldoutScopes[i] = true;
            }

            _foldoutPackages = new List<bool[]>(numberRegistry);
            _foldoutVersions = new List<List<bool[]>>(numberRegistry);

            for (var i = 0; i < numberRegistry; i++)
            {
                string keyRegistry = _scopedData.Keys.ToList()[i];
                int numberPackageInScope = _scopedData[keyRegistry].Keys.Count;
                _foldoutPackages.Add(new bool[numberPackageInScope]);
                _foldoutVersions.Add(new List<bool[]>(numberPackageInScope));

                for (var j = 0; j < numberPackageInScope; j++)
                {
                    string namePackage = _scopedData[keyRegistry].Keys.ToList()[j];
                    _foldoutVersions[i].Add(new bool[_scopedData[keyRegistry][namePackage].Count]);
                }
            }

            FetchAllLocalPackageInfo();
        }

        private void OnGUI()
        {
            void DrawLine()
            {
                EditorGUILayout.Space(4);
                var rect = EditorGUILayout.GetControlRect(false, 1);
                rect.height = 1;
                EditorGUI.DrawRect(rect, new Color(0.09f, 0.11f, 0.16f));
                EditorGUILayout.Space();
            }

            // reset state foldout
            void ResetState(IReadOnlyList<List<bool[]>> stateVersion, int i, int j, int k)
            {
                for (var l = 0; l < stateVersion.Count; l++)
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
                                for (var n = 0; n < stateVersion[i][j].Length; n++)
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

            void CheckClearInfoVersionSelected()
            {
                if (_versionPackageSelected == null) return;
                if (!_foldoutVersions[_versionPackageSelected.scopeIndex][_versionPackageSelected.packageIndex][_versionPackageSelected.versionIndex])
                    _versionPackageSelected = null;
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
                    foreach (string packageName in _scopedData[key].Keys)
                    {
                        _foldoutPackages[index][j] = EditorGUILayout.Foldout(_foldoutPackages[index][j], packageName, true);
                        if (_foldoutPackages[index][j])
                        {
                            EditorGUILayout.BeginVertical();
                            int k = 0;
                            foreach (string version in _scopedData[key][packageName])
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                string versionAndStatus = version;
                                if (IsExistPackage(packageName, version)) versionAndStatus += " (Installed)";

                                _foldoutVersions[index][j][k] = EditorGUILayout.Toggle(versionAndStatus, _foldoutVersions[index][j][k]);
                                if (_foldoutVersions[index][j][k])
                                {
                                    _versionPackageSelected = new VersionPackage
                                    {
                                        scope = key,
                                        packageName = packageName,
                                        version = version,
                                        scopeIndex = index,
                                        packageIndex = j,
                                        versionIndex = k
                                    };
                                    ResetState(_foldoutVersions, index, j, k);
                                }

                                ++k;
                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.EndVertical();
                        }

                        DrawLine();

                        ++j;
                    }

                    EditorGUILayout.EndVertical();
                }

                ++index;
            }

            EditorGUILayout.EndScrollView();
            CheckClearInfoVersionSelected();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (_versionPackageSelected != null)
            {
                bool isExistPack = IsExistPackage(_versionPackageSelected.packageName, _versionPackageSelected.version);
                switch (isExistPack)
                {
                    case true:
                    {
                        if (GUILayout.Button("Remove", GUILayout.Width(80)))
                        {
                            RemovePackage();
                            CloseWindow();
                        }

                        break;
                    }
                    case false:
                    {
                        if (GUILayout.Button("Install", GUILayout.Width(80)))
                        {
                            AddPackage();
                            CloseWindow();
                        }

                        break;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddPackage()
        {
            var result = "";

            // com.org.package_name@0.2.1
            var packageDependency = $"{_versionPackageSelected.packageName}@{_versionPackageSelected.version}";
            if (_versionPackageSelected == null)
            {
                EditorUtility.DisplayDialog("No package entered", "No package entered.", "OK");
                return;
            }

            var request = Client.Add(packageDependency);

            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (request.Status == StatusCode.Success)
            {
                result += "Imported: " + packageDependency + Environment.NewLine;
            }
            else
            {
                result += "Cannot import " + packageDependency + ": " + request.Error.message + Environment.NewLine;
            }

            EditorUtility.DisplayDialog("Added package", "Package added:" + Environment.NewLine + Environment.NewLine + result, "OK");
        }

        private void RemovePackage()
        {
            var result = "";

            // com.org.package_name
            if (_versionPackageSelected == null)
            {
                EditorUtility.DisplayDialog("No package removed", "No packages have been removed.", "OK");
                return;
            }

            var request = Client.Remove(_versionPackageSelected.packageName);

            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (request.Status == StatusCode.Success)
            {
                result += "Removed: " + _versionPackageSelected.packageName + Environment.NewLine;
            }
            else
            {
                result += "Cannot remove " + _versionPackageSelected.packageName + ": " + request.Error.message + Environment.NewLine;
            }

            EditorUtility.DisplayDialog("Remove package", "Package removed:" + Environment.NewLine + Environment.NewLine + result, "OK");
        }

        private bool IsExistPackage(string package, string version)
        {
            if (_packageInfos == null || _packageInfos.Count == 0) FetchAllLocalPackageInfo();
            return _packageInfos.FirstOrDefault(q => q.name == package && q.version == version) != null;
        }

        private void FetchAllLocalPackageInfo()
        {
            var request = Client.List();

            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            _packageInfos = request.Result.ToList();
        }

        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }
    }
}