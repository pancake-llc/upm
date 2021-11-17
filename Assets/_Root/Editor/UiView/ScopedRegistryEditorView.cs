using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.snorlax.upm
{
    internal class ScopedRegistryEditorView : EditorWindow
    {
        private bool _initialized;

        private RegistryManager _controller;

        private bool _createNew;

        private ScopedRegistry _registry;

        private int _tokenMethod;

        private void OnEnable()
        {
            _tokenMethod = 0;

            minSize = new Vector2(480, 320);
        }

        private void OnDisable() { _initialized = false; }

        public void CreateNew(RegistryManager controller)
        {
            _controller = controller;
            _createNew = true;
            _registry = new ScopedRegistry();
            _initialized = true;
        }

        public void Edit(ScopedRegistry registry, RegistryManager controller)
        {
            _controller = controller;
            _registry = registry;
            _createNew = false;
            _initialized = true;
        }


        private ReorderableList _scopeList;

        private void OnGUI()
        {
            if (_initialized)
            {
                EditorGUILayout.Space();
                if (_createNew)
                {
                    EditorGUILayout.LabelField("Add scoped registry ", EditorStyles.whiteLargeLabel);
                    _registry.name = EditorGUILayout.TextField("Name", _registry.name);

                    EditorGUI.BeginChangeCheck();
                    _registry.url = EditorGUILayout.TextField("URL", _registry.url);
                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdateCredential();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Edit scoped registry", EditorStyles.whiteLargeLabel);
                    EditorGUILayout.LabelField("Name", _registry.name);
                    EditorGUILayout.LabelField("URL", _registry.url);
                }

                if (_scopeList == null)
                {
                    _scopeList = new ReorderableList(_registry.scopes,
                        typeof(string),
                        true,
                        false,
                        true,
                        true)
                    {
                        drawElementCallback = (rect, index, _, _) => { _registry.scopes[index] = EditorGUI.TextField(rect, _registry.scopes[index]); },
                        onAddCallback = _ => { _registry.scopes.Add(""); }
                    };
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Package Scopes");
                EditorGUILayout.BeginVertical();
                _scopeList.DoLayoutList();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Authentication / Credentials", EditorStyles.whiteLargeLabel);

                _registry.auth = EditorGUILayout.Toggle("Always auth", _registry.auth);
                _registry.token = EditorGUILayout.TextField("Token", _registry.token);

                EditorGUILayout.Space();

                _tokenMethod = GetTokenView.CreateGUI(_tokenMethod, _registry);

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                EditorGUILayout.EndVertical();

                EditorGUILayout.HelpBox("Restart Unity to reload credentials after saving.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                if (_createNew)
                {
                    if (GUILayout.Button("Add"))
                    {
                        Add();
                    }
                }
                else
                {
                    if (GUILayout.Button("Save"))
                    {
                        Save();
                    }
                }

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
            if (_registry.IsValid())
            {
                _controller.Save(_registry);
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for registry.", "Ok");
            }
        }

        private void Add()
        {
            if (_registry.IsValid())
            {
                _controller.Save(_registry);
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for registry.", "Ok");
            }
        }

        private void UpdateCredential()
        {
            if (_controller.CredentialManager.HasRegistry(_registry.url))
            {
                var cred = _controller.CredentialManager.GetCredential(_registry.url);
                _registry.auth = cred.alwaysAuth;
                _registry.token = cred.token;
            }
        }
    }
}