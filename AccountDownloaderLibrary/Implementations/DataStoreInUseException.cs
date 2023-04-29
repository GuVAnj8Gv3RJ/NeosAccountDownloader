using System.Runtime.Serialization;

namespace AccountDownloaderLibrary
{
    [Serializable]
    internal class DataStoreInUseException : Exception
    {
        public DataStoreInUseException(string message) : base(message)
        {
        }
    }
}
