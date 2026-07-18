using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Commands
{
    /// <summary>
    /// Manages command execution and handles a history stack to support Undo/Redo operations.
    /// Supports both synchronous and asynchronous commands.
    /// </summary>
    public class CommandExecutor
    {
        private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();

        private readonly Stack<IUndoableAsyncCommand> _undoAsyncStack = new Stack<IUndoableAsyncCommand>();
        private readonly Stack<IUndoableAsyncCommand> _redoAsyncStack = new Stack<IUndoableAsyncCommand>();

        /// <summary>
        /// Gets the number of undoable synchronous commands currently in the history.
        /// </summary>
        public int UndoCount => _undoStack.Count;

        /// <summary>
        /// Gets the number of redoable synchronous commands currently in the history.
        /// </summary>
        public int RedoCount => _redoStack.Count;

        /// <summary>
        /// Gets the number of undoable asynchronous commands currently in the history.
        /// </summary>
        public int UndoAsyncCount => _undoAsyncStack.Count;

        /// <summary>
        /// Gets the number of redoable asynchronous commands currently in the history.
        /// </summary>
        public int RedoAsyncCount => _redoAsyncStack.Count;

        #region Synchronous Commands

        /// <summary>
        /// Executes a synchronous command. If undoable, adds it to the undo history and clears the redo history.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Execute(ICommand command)
        {
            if (command == null) return;

            command.Execute();

            if (command is IUndoableCommand undoable)
            {
                _undoStack.Push(undoable);
                _redoStack.Clear();
            }
        }

        /// <summary>
        /// Undoes the last executed undoable synchronous command.
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count == 0) return;

            IUndoableCommand command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }

        /// <summary>
        /// Redoes the last undone synchronous command.
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            IUndoableCommand command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }

        #endregion

        #region Asynchronous Commands

        /// <summary>
        /// Asynchronously executes an asynchronous command. If undoable, adds it to the undo history and clears the redo history.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public async UniTask ExecuteAsync(IAsyncCommand command)
        {
            if (command == null) return;

            await command.ExecuteAsync();

            if (command is IUndoableAsyncCommand undoable)
            {
                _undoAsyncStack.Push(undoable);
                _redoAsyncStack.Clear();
            }
        }

        /// <summary>
        /// Asynchronously undoes the last executed undoable asynchronous command.
        /// </summary>
        public async UniTask UndoAsync()
        {
            if (_undoAsyncStack.Count == 0) return;

            IUndoableAsyncCommand command = _undoAsyncStack.Pop();
            await command.UndoAsync();
            _redoAsyncStack.Push(command);
        }

        /// <summary>
        /// Asynchronously redoes the last undone asynchronous command.
        /// </summary>
        public async UniTask RedoAsync()
        {
            if (_redoAsyncStack.Count == 0) return;

            IUndoableAsyncCommand command = _redoAsyncStack.Pop();
            await command.ExecuteAsync();
            _undoAsyncStack.Push(command);
        }

        #endregion

        /// <summary>
        /// Clears all execution histories.
        /// </summary>
        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _undoAsyncStack.Clear();
            _redoAsyncStack.Clear();
        }
    }
}
