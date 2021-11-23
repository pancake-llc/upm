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
}