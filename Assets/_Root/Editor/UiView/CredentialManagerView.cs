using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.snorlax.upm
{
    public class CredentialManagerView : EditorWindow
    {
        private ReorderableList _drawer;

        private void OnEnable()
        {
            _drawer = GetCredentialList(new CredentialManager());
            minSize = new Vector2(640, 320);
        }

        private void OnGUI() { _drawer.DoLayoutList(); }

        internal static ReorderableList GetCredentialList(CredentialManager credentialManager)
        {
            ReorderableList credentialList = null;
            credentialList = new ReorderableList(credentialManager.CredentialSet,
                typeof(NpmCredential),
                false,
                true,
                true,
                true)
            {
                drawHeaderCallback = rect => { GUI.Label(rect, "User Credentials on this computer"); },
                drawElementCallback = (rect, index, _, _) =>
                {
                    var credential = credentialList.list[index] as NpmCredential;
                    if (credential == null) return;

                    rect.width -= 60;
                    EditorGUI.LabelField(rect, credential.url);

                    rect.x += rect.width;
                    rect.width = 60;
                    if (GUI.Button(rect, "Edit"))
                    {
                        var credentialEditor = GetWindow<CredentialEditorView>(true, "Edit credential", true);
                        credentialEditor.Edit(credential, credentialManager);
                    }
                },
                onAddCallback = _ =>
                {
                    var credentialEditor = GetWindow<CredentialEditorView>(true, "Add credential", true);
                    credentialEditor.CreateNew(credentialManager);
                },
                onRemoveCallback = _ =>
                {
                    var credential = credentialList.list[credentialList.index] as NpmCredential;

                    credentialManager.RemoveCredential(credential.url);
                    credentialManager.Write();
                }
            };
            return credentialList;
        }
    }
}