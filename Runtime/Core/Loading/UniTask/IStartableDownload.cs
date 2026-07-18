using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Loading
{
    public interface IStartableDownload<TDownloadResult>
    {
        UniTask<TDownloadResult> StartDownloadAsync();
    }
}
