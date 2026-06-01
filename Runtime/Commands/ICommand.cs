using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Commands
{
    /// <summary>
    /// Interface for standard execute-only synchronous commands.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// Interface for standard execute-only asynchronous commands using UniTask.
    /// </summary>
    public interface IAsyncCommand
    {
        /// <summary>
        /// Asynchronously executes the command.
        /// </summary>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        UniTask ExecuteAsync();
    }
}
