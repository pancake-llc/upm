using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Tomlyn;
using Tomlyn.Model;
using UnityEngine;

namespace com.snorlax.upm
{
    public static class GithubResponse
    {
        public static List<GithubOrganization> GetAllPackages()
        {
            var orgs = new List<GithubOrganization>();

            var credentialSet = new List<NpmCredential>();
            if (File.Exists(CredentialManager.UpmconfigFile))
            {
                var upmconfig = Toml.Parse(File.ReadAllText(CredentialManager.UpmconfigFile));
                if (upmconfig.HasErrors)
                {
                    Debug.LogError("Cannot load upmconfig, invalid format");
                    return null;
                }

                var table = upmconfig.ToModel();

                if (table != null && table.ContainsKey("npmAuth"))
                {
                    var auth = (TomlTable)table["npmAuth"];
                    if (auth != null)
                    {
                        foreach ((string key, object o) in auth)
                        {
                            var cred = new NpmCredential { url = key };
                            var value = (TomlTable)o;
                            cred.token = (string)value["token"];
                            cred.alwaysAuth = (bool)value["alwaysAuth"];

                            credentialSet.Add(cred);
                        }
                    }
                }
            }

            foreach (var credential in credentialSet)
            {
                var orgPacakges = new List<GithubPackage>();
                string scoped = credential.url.Split('@')[1];
                orgs.Add(new GithubOrganization() { scope = scoped, packages = orgPacakges });

                using var client = new WebClient();
                var urlRequestPackage = $"https://api.github.com/orgs/{scoped}/packages?package_type=npm";
                client.Headers.Add(HttpRequestHeader.Authorization, $"token {credential.token}");
                client.Headers.Add(HttpRequestHeader.UserAgent, "request");

                string result = client.DownloadString(urlRequestPackage);
                var packages = JsonConvert.DeserializeObject<List<Package>>(result);

                foreach (var package in packages)
                {
                    using var clientVersion = new WebClient();
                    clientVersion.Headers.Add(HttpRequestHeader.Authorization, $"token {credential.token}");
                    clientVersion.Headers.Add(HttpRequestHeader.UserAgent, "request");
                    var urlRequestVersion = $"https://api.github.com/orgs/{scoped}/packages/npm/{package.name}/versions";

                    string versionResult = clientVersion.DownloadString(urlRequestVersion);
                    var versions = JsonConvert.DeserializeObject<List<PackageVersion>>(versionResult);
                    orgPacakges.Add(new GithubPackage()
                    {
                        id = package.name,
                        name = package.repository.name,
                        description = package.repository.description,
                        versions = versions.Select(_ => _.name).ToList()
                    });
                }
            }

            return orgs;
        }
    }
}