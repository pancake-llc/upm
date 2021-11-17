using System;
using UnityEditor;
using UnityEngine;

namespace com.snorlax.upm
{
    internal class CredentialEditorView : EditorWindow
    {
        private bool _initialized;

        private CredentialManager _credentialManager;

        private bool _createNew;

        private ScopedRegistry _registry;

        private int _tokenMethod;


        private void OnEnable()
        {
            _tokenMethod = 0;
            minSize = new Vector2(480, 320);
        }

        private void OnDisable() { _initialized = false; }

        public void CreateNew(CredentialManager credentialManager)
        {
            _credentialManager = credentialManager;
            _createNew = true;
            _registry = new ScopedRegistry();
            _initialized = true;
        }

        public void Edit(NpmCredential credential, CredentialManager credentialManager)
        {
            _credentialManager = credentialManager;
            _registry = new ScopedRegistry { url = credential.url, auth = credential.alwaysAuth, token = credential.token };

            _createNew = false;
            _initialized = true;
        }

        private void OnGUI()
        {
            if (_initialized)
            {
                if (_createNew)
                {
                    EditorGUILayout.LabelField("Add credential", EditorStyles.whiteLargeLabel);

                    _registry.url = EditorGUILayout.TextField("Registry URL", _registry.url);
                }
                else
                {
                    EditorGUILayout.LabelField("Edit credential", EditorStyles.whiteLargeLabel);
                    EditorGUILayout.LabelField("Registry URL: " + _registry.url);
                }

                if (string.IsNullOrEmpty(_registry.url))
                {
                    EditorGUILayout.HelpBox("Enter the registry URL you want to add authentication for.", MessageType.Warning);
                }

                _registry.auth = EditorGUILayout.Toggle("Always auth", _registry.auth);
                _registry.token = EditorGUILayout.TextField("Token", _registry.token);

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_registry.url));
                _tokenMethod = GetTokenView.CreateGUI(_tokenMethod, _registry);

                if (!string.IsNullOrEmpty(_registry.url) && string.IsNullOrEmpty(_registry.token))
                {
                    EditorGUILayout.HelpBox("Select an authentication method and click on \"Get token\"", MessageType.Warning);
                }

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_registry.token));

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                EditorGUILayout.EndVertical();

                EditorGUILayout.HelpBox("Restart Unity to reload credentials after saving.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                if (_createNew)
                {
                    if (GUILayout.Button("Add"))
                    {
                        Save();
                    }
                }
                else
                {
                    if (GUILayout.Button("Save"))
                    {
                        Save();
                    }
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Cancel"))
                {
                    Close();
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void Save()
        {
            if (_registry.IsValidCredential() && !string.IsNullOrEmpty(_registry.token))
            {
                _credentialManager.SetCredential(_registry.url, _registry.auth, _registry.token);
                _credentialManager.Write();

                // TODO figure out in which cases/Editor versions a restart is actually required,
                // and where a Client.Resolve() call or PackMan reload is sufficient
                if (EditorUtility.DisplayDialog("Unity Editor restart might be required",
                    "The Unity editor might need to be restarted for this change to take effect.",
                    "Restart Now",
                    "Cancel"))
                {
                    EditorApplication.OpenProject(Environment.CurrentDirectory);
                }

                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for credential.", "Ok");
            }
        }
    }
}