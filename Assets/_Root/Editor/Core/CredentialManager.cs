using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using UnityEngine;

namespace com.snorlax.upm
{
    public class NpmCredential
    {
        public string url;
        public string token;
        public bool alwaysAuth;
    }

    public class CredentialManager
    {
        private string _upmconfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".upmconfig.toml");
        private List<NpmCredential> _credentials = new List<NpmCredential>();

        public List<NpmCredential> CredentialSet { get { return _credentials; } }

        public String[] Registries
        {
            get
            {
                var urls = new String[_credentials.Count];
                var index = 0;
                foreach (var cred in CredentialSet)
                {
                    urls[index] = cred.url;
                    ++index;
                }

                return urls;
            }
        }

        public CredentialManager()
        {
            if (File.Exists(_upmconfigFile))
            {
                var upmconfig = Toml.Parse(File.ReadAllText(_upmconfigFile));
                if (upmconfig.HasErrors)
                {
                    Debug.LogError("Cannot load upmconfig, invalid format");
                    return;
                }

                var table = upmconfig.ToModel();

                if (table != null && table.ContainsKey("npmAuth"))
                {
                    var auth = (TomlTable)table["npmAuth"];
                    if (auth != null)
                    {
                        foreach (var registry in auth)
                        {
                            var cred = new NpmCredential { url = registry.Key };
                            var value = (TomlTable)registry.Value;
                            cred.token = (string)value["token"];
                            cred.alwaysAuth = (bool)value["alwaysAuth"];

                            _credentials.Add(cred);
                        }
                    }
                }
            }
        }

        public void Write()
        {
            var doc = new DocumentSyntax();

            foreach (var credential in _credentials)
            {
                if (string.IsNullOrEmpty(credential.token))
                {
                    credential.token = "";
                }

                doc.Tables.Add(new TableSyntax(new KeySyntax("npmAuth", credential.url))
                {
                    Items = { { "token", credential.token }, { "alwaysAuth", credential.alwaysAuth } }
                });
            }


            File.WriteAllText(_upmconfigFile, doc.ToString());
        }

        public bool HasRegistry(string url) { return _credentials.Any(x => x.url.Equals(url, StringComparison.Ordinal)); }

        public NpmCredential GetCredential(string url) { return _credentials.FirstOrDefault(x => x.url?.Equals(url, StringComparison.Ordinal) ?? false); }

        public void SetCredential(string url, bool alwaysAuth, string token)
        {
            if (HasRegistry(url))
            {
                var cred = GetCredential(url);
                cred.url = url;
                cred.alwaysAuth = alwaysAuth;
                cred.token = token;
            }
            else
            {
                var newCred = new NpmCredential { url = url, alwaysAuth = alwaysAuth, token = token };

                _credentials.Add(newCred);
            }
        }

        public void RemoveCredential(string url)
        {
            if (HasRegistry(url))
            {
                _credentials.RemoveAll(x => x.url.Equals(url, StringComparison.Ordinal));
            }
        }
    }
}