using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Commands
{
    /// <summary>
    /// Interface for reversible synchronous commands.
    /// </summary>
    public interface IUndoableCommand : ICommand
    {
        /// <summary>
        /// Reverses the execution of the command.
        /// </summary>
        void Undo();
    }

    /// <summary>
    /// Interface for reversible asynchronous commands using UniTask.
    /// </summary>
    public interface IUndoableAsyncCommand : IAsyncCommand
    {
        /// <summary>
        /// Asynchronously reverses the execution of the command.
        /// </summary>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        UniTask UndoAsync();
    }
}
