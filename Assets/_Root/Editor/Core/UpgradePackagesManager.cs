using System.Collections.Generic;
using System.Threading;
using Artees.UnitySemVer;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace com.snorlax.upm
{
    public class UpgradePackagesManager
    {
        public class PackageUpgradeState
        {
            public PackageUpgradeState(PackageInfo info)
            {
                this.info = info;
                _previewAvailable = false;
                _stableAvailable = false;
                _verifiedAvailable = false;
                _hasVerified = false;
                _stableVersion = SemVer.Parse(info.version);
                _previewVersion = SemVer.Parse(info.version);


                try
                {
                    _current = SemVer.Parse(info.version);
                }
                catch
                {
                    Debug.LogError("Cannot parse version for package " + info.displayName + ": " + info.version);
                }

                if (info.source == PackageSource.Git)
                {
                    _previewAvailable = true;
                    _preview = info.packageId;

                    _stableAvailable = true;
                    _stable = info.packageId;
                }
                else if (info.source == PackageSource.Registry)
                {
                    string[] compatible = info.versions.compatible;

                    foreach (string ver in compatible)
                    {
                        try
                        {
                            var version = SemVer.Parse(ver);

                            if (string.IsNullOrWhiteSpace(version.preRelease))
                            {
                                if (version > _stableVersion)
                                {
                                    _stableVersion = version;
                                    _stableAvailable = true;
                                    _stable = info.name + "@" + ver;
                                }
                            }
                            else
                            {
                                // This is a pre-release
                                if (version > _previewVersion)
                                {
                                    _previewVersion = version;
                                    _previewAvailable = true;
                                    _preview = info.name + "@" + ver;
                                }
                            }
                        }
                        catch
                        {
                            Debug.LogError("Invalid version for package " + info.displayName + ": " + ver);
                        }
                    }

                    _hasVerified = !string.IsNullOrWhiteSpace(info.versions.verified);
                    if (_hasVerified)
                    {
                        try
                        {
                            _verifiedVersion = SemVer.Parse(info.versions.verified);
                            if (_verifiedVersion > _current)
                            {
                                _verifiedAvailable = _verifiedVersion > _current;
                                _verified = info.name + "@" + info.versions.verified;
                            }
                        }
                        catch
                        {
                            Debug.LogError("Cannot parse version for package " + info.displayName + ": " + info.versions.verified);
                        }
                    }
                }
            }

            internal string GetCurrentVersion() { return info.packageId; }

            public PackageInfo info;
            private SemVer _current;
            private bool _previewAvailable;
            private SemVer _previewVersion;
            private string _preview;
            private bool _stableAvailable;
            private SemVer _stableVersion;
            private string _stable;
            private bool _hasVerified;
            private bool _verifiedAvailable;
            private SemVer _verifiedVersion;
            private string _verified;

            public bool HasNewVersion(bool showPreviewVersion, bool useVerified)
            {
                if (useVerified && _hasVerified)
                {
                    return _verifiedAvailable;
                }

                if (showPreviewVersion)
                {
                    return _previewAvailable || _stableAvailable;
                }

                return _stableAvailable;
            }

            public string GetNewestVersion(bool showPreviewVersion, bool useVerified)
            {
                if (useVerified && _hasVerified)
                {
                    if (_verifiedAvailable)
                    {
                        return _verified;
                    }
                }
                else if (showPreviewVersion)
                {
                    if (_previewAvailable)
                    {
                        if (!_stableAvailable || _previewVersion > _stableVersion)
                        {
                            return _preview;
                        }
                    }
                }

                if (_stableAvailable)
                {
                    if (_stableAvailable)
                    {
                        return _stable;
                    }
                }

                return null;
            }
        }

        public List<PackageUpgradeState> upgradeablePackages = new();

        private ListRequest _request;

        public bool packagesLoaded;

        public UpgradePackagesManager()
        {
#if UNITY_2019_1_OR_NEWER
            _request = Client.List(false, false);
#else
            request = Client.List();
#endif
        }

        public void Update()
        {
            if (!packagesLoaded && _request.IsCompleted)
            {
                if (_request.Status == StatusCode.Success)
                {
                    var collection = _request.Result;
                    foreach (var info in collection)
                    {
                        upgradeablePackages.Add(new PackageUpgradeState(info));
                    }
                }
                else
                {
                    Debug.LogError("Cannot query package manager for packages");
                }

                packagesLoaded = true;
            }
        }


        public bool UpgradePackage(string packageWithVersion, ref string error)
        {
            var request = Client.Add(packageWithVersion);

            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (request.Status == StatusCode.Success)
            {
                return true;
            }

            error = request.Error.message;
            return false;
        }
    }
}