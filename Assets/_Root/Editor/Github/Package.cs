using System;

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
    }

    [Serializable]
    public class PackageVersion
    {
        public int id;
        public string name;
        public string package_html_url;
        public string description;
    }

    public class VersionPackage
    {
        public string scope;
        public string packageName;
        public string version;

        public int scopeIndex;
        public int packageIndex;
        public int versionIndex;
    }
}