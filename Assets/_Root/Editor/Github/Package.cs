using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace com.snorlax.upm
{
    [Serializable]
    public class Package
    {
        public int id;
        public string name;
        public string package_type;
        public int version_count;
        public Repository repository;
    }

    [Serializable]
    public class PackageVersion
    {
        public int id;
        public string name;
        public string package_html_url;
        public string description;
    }

    [Serializable]
    public class VersionPackage
    {
        public string scope;
        public string packageName;
        public string version;

        public int scopeIndex;
        public int packageIndex;
        public int versionIndex;
    }

    [Serializable]
    public class Repository
    {
        public string name;
        public string full_name;
        public string description;
        public bool @private;
    }

    [Serializable]
    public class  GithubPackage
    {
        public string id;
        public string name;
        public string description;
        public List<string> versions;
    }

    [Serializable]
    public class GithubOrganization
    {
        public string scope;
        public List<GithubPackage> packages;
    }
}