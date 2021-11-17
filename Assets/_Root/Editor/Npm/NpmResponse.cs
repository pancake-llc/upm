using System;

namespace com.snorlax.upm
{
    [Serializable]
    public class NpmResponse
    {
        public string error;
        public string ok;
        public string token;
        public bool success;
        public string reason;
    }
}