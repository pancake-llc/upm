using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using Tomlyn;
using Tomlyn.Model;
using UnityEditor;
using UnityEngine;

namespace com.snorlax.upm
{
    public class GithubResponse
    {
        [MenuItem("Packages/Get All", false, 20)]
        public static void GetAllPackages()
        {
            var credentialSet = new List<NpmCredential>();
            if (File.Exists(CredentialManager.UpmconfigFile))
            {
                var upmconfig = Toml.Parse(File.ReadAllText(CredentialManager.UpmconfigFile));
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

                            credentialSet.Add(cred);
                        }
                    }
                }
            }

            var hrefs = new List<string>();
            var dictPackage = new Dictionary<string, List<string>>();

            foreach (var credential in credentialSet)
            {
                hrefs.Clear();
                dictPackage.Clear();
                var html = $"https://github.com/orgs/{credential.url.Split('@')[1]}/packages";

                var web = new HtmlWeb();
                var htmlDoc = web.Load(html);
                var node = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

                foreach (var n in node)
                {
                    string hrefValue = n.GetAttributeValue("href", string.Empty);
                    if (hrefValue.Contains("/snorluxe/") && hrefValue.Contains("/packages/")) hrefs.Add(hrefValue);
                }

                foreach (string href in hrefs)
                {
                    string namePackage = href.Split('/')[2];
                    if (!dictPackage.ContainsKey(namePackage)) dictPackage.Add(namePackage, new List<string>());

                    var versionQuery = $"https://github.com{href}versions";
                    var versionDoc = web.Load(versionQuery);
                    var versionNode = versionDoc.DocumentNode.SelectNodes("//a[@href]");

                    foreach (var version in versionNode)
                    {
                        string hrefValue = version.GetAttributeValue("href", string.Empty);
                        if (hrefValue.Contains("/snorluxe/") && hrefValue.Contains("/packages/") && hrefValue.Contains("?version="))
                        {
                            string[] elementVersion = hrefValue.Split("?version=");
                            dictPackage[namePackage].Add(elementVersion[1]);
                        }
                    }
                }

                // foreach (var package in dictPackage)
                // {
                //     Debug.Log("name: " + package.Key);
                //     foreach (string s in package.Value)
                //     {
                //         Debug.Log(s);
                //     }
                //
                //     Debug.Log("------------------");
                // }
            }
        }

        public void HandleFetchPackagesResult(string response) { }
    }
}