using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static com.snorlax.upm.UpgradePackagesManager;

namespace com.snorlax.upm
{
    internal class UpgradePackagesView : EditorWindow
    {
        [MenuItem("Packages/Upgrade Packages", false, 23)]
        internal static void ManageRegistries() { GetWindow<UpgradePackagesView>(true, "Upgrade packages", true); }

        private UpgradePackagesManager _manager;

        private bool _upgradeAll;

        private bool _showPreviewPackages;

        private bool _useVerified = true;
        private Dictionary<PackageUpgradeState, bool> _upgradeList = new Dictionary<PackageUpgradeState, bool>();

        private void OnEnable()
        {
            _manager = new UpgradePackagesManager();

            minSize = new Vector2(640, 320);
            _upgradeAll = false;
        }

        private void OnDisable() { _manager = null; }

        private Vector2 _scrollPos;

        private void Package(PackageUpgradeState info)
        {
            if (info.HasNewVersion(_showPreviewPackages, _useVerified))
            {
                var boxStyle = new GUIStyle { padding = new RectOffset(10, 10, 0, 0) };

                EditorGUILayout.BeginHorizontal(boxStyle);


                EditorGUI.BeginChangeCheck();

                var upgrade = false;
                if (_upgradeList.ContainsKey(info))
                {
                    upgrade = _upgradeList[info];
                }

                upgrade = EditorGUILayout.BeginToggleGroup(info.GetCurrentVersion(), upgrade);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!upgrade)
                    {
                        _upgradeAll = false;
                    }
                }

                _upgradeList[info] = upgrade;

                EditorGUILayout.EndToggleGroup();


                EditorGUILayout.LabelField(info.GetNewestVersion(_showPreviewPackages, _useVerified));

                EditorGUILayout.EndHorizontal();
            }
        }

        private void OnGUI()
        {
            if (_manager != null)
            {
                _manager.Update();

                EditorGUILayout.LabelField("Upgrade packages", EditorStyles.whiteLargeLabel);

                _showPreviewPackages = EditorGUILayout.ToggleLeft("Show Preview Packages", _showPreviewPackages);

                _useVerified = EditorGUILayout.ToggleLeft("Prefer verified packages", _useVerified);

                if (_manager.packagesLoaded)
                {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);


                    foreach (var info in _manager.upgradeablePackages)
                    {
                        Package(info);
                    }

                    EditorGUILayout.EndScrollView();


                    EditorGUI.BeginChangeCheck();
                    _upgradeAll = EditorGUILayout.ToggleLeft("Upgrade all packages", _upgradeAll);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var info in _manager.upgradeablePackages)
                        {
                            _upgradeList[info] = _upgradeAll;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Upgrade"))
                    {
                        Upgrade();
                        CloseWindow();
                    }

                    if (GUILayout.Button("Close"))
                    {
                        CloseWindow();
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("Loading packages...", EditorStyles.whiteLargeLabel);
                }
            }
        }

        private void Upgrade()
        {
            if (_manager != null)
            {
                EditorUtility.DisplayProgressBar("Upgrading packages", "Starting", 0);

                var output = "";
                var failures = false;
                try
                {
                    foreach (var info in _manager.upgradeablePackages)
                    {
                        if (_upgradeList.ContainsKey(info))
                        {
                            if (_upgradeList[info])
                            {
                                EditorUtility.DisplayProgressBar("Upgrading packages", "Upgrading " + info.info.displayName, 0.5f);

                                var error = "";
                                if (_manager.UpgradePackage(info.GetNewestVersion(_showPreviewPackages, _useVerified), ref error))
                                {
                                    output += "[Success] Upgraded " + info.info.displayName + Environment.NewLine;
                                }
                                else
                                {
                                    output += "[Error] Failed upgrade of" + info.info.displayName + " with error: " + error + Environment.NewLine;
                                    failures = true;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }


                string message;
                if (failures)
                {
                    message = "Upgraded with errors." + Environment.NewLine + output;
                }
                else
                {
                    message = "Upgraded all packages. " + Environment.NewLine + output;
                }

                EditorUtility.DisplayDialog("Upgrade finished", message, "OK");
            }
        }


        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }
    }
}