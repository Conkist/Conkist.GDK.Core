using System;

namespace Conkist.GDK.Loading
{
    /// <summary>
    /// Interface for querying the current assets download status.
    /// </summary>
    public interface IAssetsDownloadStatus<out TStatus> : IDisposable
    {
        TStatus DownloadStatus { get; }
    }
}
