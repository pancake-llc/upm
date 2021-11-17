using UnityEditor;
using UnityEngine;

namespace com.snorlax.upm
{
    internal class TokenMethod : GUIContent
    {
        internal delegate bool GetToken(ScopedRegistry registry, string username, string password);

        internal string usernameName;
        internal string passwordName;
        internal GetToken action;

        public TokenMethod(string name, string usernameName, string passwordName, GetToken action)
            : base(name)
        {
            this.usernameName = usernameName;
            this.passwordName = passwordName;
            this.action = action;
        }
    }

    internal class GetTokenView : EditorWindow
    {
        private static TokenMethod[] methods =
        {
            new TokenMethod("npm login", "Registry username", "Registry password", GetNpmLoginToken),
            new TokenMethod("bintray", "Bintray username", "Bintray API key", GetBintrayToken),
            // TODO adjust TokenMethod to allow for opening GitHub token URL: https://github.com/settings/tokens/new
        };


        private string _username;
        private string _password;

        private bool _initialized;

        private TokenMethod _tokenMethod;

        private ScopedRegistry _registry;


        private void OnEnable() { error = null; }

        private void OnDisable() { _initialized = false; }

        private void SetRegistry(TokenMethod tokenMethod, ScopedRegistry registry)
        {
            _tokenMethod = tokenMethod;
            _registry = registry;
            _initialized = true;
        }

        private void OnGUI()
        {
            if (_initialized)
            {
                EditorGUILayout.LabelField(_tokenMethod, EditorStyles.whiteLargeLabel);
                _username = EditorGUILayout.TextField(_tokenMethod.usernameName, _username);
                _password = EditorGUILayout.PasswordField(_tokenMethod.passwordName, _password);

                if (GUILayout.Button("Login"))
                {
                    if (_tokenMethod.action(_registry, _username, _password))
                    {
                        CloseWindow();
                    }
                }

                if (GUILayout.Button("Close"))
                {
                    CloseWindow();
                }

                if (!string.IsNullOrEmpty(error))
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
        }

        private static void CreateWindow(TokenMethod method, ScopedRegistry registry)
        {
            var getTokenView = GetWindow<GetTokenView>(true, "Get token", true);
            getTokenView.SetRegistry(method, registry);
        }

        private static string error;

        private static bool GetNpmLoginToken(ScopedRegistry registry, string username, string password)
        {
            var response = NpmLogin.GetLoginToken(registry.url, username, password);

            if (string.IsNullOrEmpty(response.ok))
            {
                // EditorUtility.DisplayDialog("Cannot get token", response.error, "Ok");
                error = "Cannot get token: " + response.error;
                return false;
            }

            registry.token = response.token;
            return true;
        }

        private static bool GetBintrayToken(ScopedRegistry registry, string username, string password)
        {
            registry.token = NpmLogin.GetBintrayToken(username, password);
            return !string.IsNullOrEmpty(registry.token);
        }


        private void CloseWindow()
        {
            error = null;
            foreach (var view in Resources.FindObjectsOfTypeAll<CredentialEditorView>())
            {
                view.Repaint();
            }

            Close();
            GUIUtility.ExitGUI();
        }


        internal static int CreateGUI(int selectedIndex, ScopedRegistry registry)
        {
            EditorGUILayout.LabelField("Generate token", EditorStyles.whiteLargeLabel);
            EditorGUILayout.BeginHorizontal();
            selectedIndex = EditorGUILayout.Popup(new GUIContent("Method"), selectedIndex, methods);

            if (GUILayout.Button("Login & get auth token"))
            {
                CreateWindow(methods[selectedIndex], registry);
            }

            EditorGUILayout.EndHorizontal();

            return selectedIndex;
        }
    }
}