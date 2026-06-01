using System;

namespace Conkist.GDK.Loading
{
    /// <summary>
    /// Interface for tracking download/load progress reporting.
    /// </summary>
    public interface ITrackableProgress<TProgress>
    {
        void TrackProgress(IProgress<TProgress> progress);
    }
}
