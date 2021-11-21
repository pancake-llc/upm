using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace com.snorlax.upm
{
    public class RegistryManager
    {
        private string _manifest = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

        public List<ScopedRegistry> Registries { get; private set; }

        public CredentialManager CredentialManager { get; private set; }

        public RegistryManager()
        {
            CredentialManager = new CredentialManager();
            Registries = new List<ScopedRegistry>();

            var manifestJson = JObject.Parse(File.ReadAllText(_manifest));

            var jregistries = (JArray)manifestJson["scopedRegistries"];
            if (jregistries != null)
            {
                foreach (var jRegistry in jregistries)
                {
                    Registries.Add(LoadRegistry((JObject)jRegistry));
                }
            }
            else
            {
                Debug.Log("No scoped registries set");
            }
        }

        private ScopedRegistry LoadRegistry(JObject jregistry)
        {
            var registry = new ScopedRegistry { name = (string)jregistry["name"], url = (string)jregistry["url"] };

            var scopes = new List<string>();
            foreach (var scope in (JArray)jregistry["scopes"])
            {
                scopes.Add((string)scope);
            }

            registry.scopes = new List<string>(scopes);

            if (CredentialManager.HasRegistry(registry.url))
            {
                var credential = CredentialManager.GetCredential(registry.url);
                registry.auth = credential.alwaysAuth;
                registry.token = credential.token;
            }

            return registry;
        }

        private void UpdateScope(ScopedRegistry registry, JToken registryElement)
        {
            var scopes = new JArray();
            foreach (var scope in registry.scopes)
            {
                scopes.Add(scope);
            }

            registryElement["scopes"] = scopes;
        }

        private JToken GetOrCreateScopedRegistry(ScopedRegistry registry, JObject manifestJson)
        {
            var jregistries = (JArray)manifestJson["scopedRegistries"];
            if (jregistries == null)
            {
                jregistries = new JArray();
                manifestJson["scopedRegistries"] = jregistries;
            }

            foreach (var jRegistryElement in jregistries)
            {
                if (jRegistryElement["name"] != null && jRegistryElement["url"] != null &&
                    string.Equals(jRegistryElement["name"].Value<string>(), registry.name, StringComparison.Ordinal) &&
                    string.Equals(jRegistryElement["url"].Value<string>(), registry.url, StringComparison.Ordinal))
                {
                    UpdateScope(registry, jRegistryElement);
                    return jRegistryElement;
                }
            }

            var jRegistry = new JObject { ["name"] = registry.name, ["url"] = registry.url };
            UpdateScope(registry, jRegistry);
            jregistries.Add(jRegistry);

            return jRegistry;
        }

        public void Remove(ScopedRegistry registry)
        {
            var manifestJson = JObject.Parse(File.ReadAllText(_manifest));
            var jregistries = (JArray)manifestJson["scopedRegistries"];

            foreach (var jRegistryElement in jregistries)
            {
                if (jRegistryElement["name"] != null && jRegistryElement["url"] != null &&
                    jRegistryElement["name"].Value<string>().Equals(registry.name, StringComparison.Ordinal) &&
                    jRegistryElement["url"].Value<string>().Equals(registry.url, StringComparison.Ordinal))
                {
                    jRegistryElement.Remove();
                    break;
                }
            }

            Write(manifestJson);
        }

        public void Save(ScopedRegistry registry)
        {
            var manifestJson = JObject.Parse(File.ReadAllText(_manifest));

            var manifestRegistry = GetOrCreateScopedRegistry(registry, manifestJson);

            if (!string.IsNullOrEmpty(registry.token))
            {
                CredentialManager.SetCredential(registry.url, registry.auth, registry.token);
            }
            else
            {
                CredentialManager.RemoveCredential(registry.url);
            }

            Write(manifestJson);

            CredentialManager.Write();
        }

        private void Write(JObject manifestJson)
        {
            File.WriteAllText(_manifest, manifestJson.ToString());
            AssetDatabase.Refresh();
        }
    }
}