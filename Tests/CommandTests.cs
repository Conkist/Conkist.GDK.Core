using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using Conkist.GDK.Commands;

namespace Conkist.GDK.Tests
{
    public class CommandTests
    {
        // Simple synchronous command for testing
        public class SimpleCommand : ICommand
        {
            public bool Executed { get; private set; } = false;

            public void Execute()
            {
                Executed = true;
            }
        }

        // Reversible synchronous command for testing Undo/Redo
        public class UndoableCommand : IUndoableCommand
        {
            public int State { get; private set; }
            private readonly int _targetState;
            private readonly int _previousState;

            public UndoableCommand(int initialState, int targetState)
            {
                State = initialState;
                _previousState = initialState;
                _targetState = targetState;
            }

            public void Execute()
            {
                State = _targetState;
            }

            public void Undo()
            {
                State = _previousState;
            }
        }

        // Simple asynchronous command for testing
        public class SimpleAsyncCommand : IAsyncCommand
        {
            public bool Executed { get; private set; } = false;

            public async UniTask ExecuteAsync()
            {
                await UniTask.Yield();
                Executed = true;
            }
        }

        // Reversible asynchronous command for testing async Undo/Redo
        public class UndoableAsyncCommand : IUndoableAsyncCommand
        {
            public int State { get; private set; }
            private readonly int _targetState;
            private readonly int _previousState;

            public UndoableAsyncCommand(int initialState, int targetState)
            {
                State = initialState;
                _previousState = initialState;
                _targetState = targetState;
            }

            public async UniTask ExecuteAsync()
            {
                await UniTask.Yield();
                State = _targetState;
            }

            public async UniTask UndoAsync()
            {
                await UniTask.Yield();
                State = _previousState;
            }
        }

        [UnityTest]
        public IEnumerator CommandExecutor_ExecutesSimpleCommand()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var executor = new CommandExecutor();
                var cmd = new SimpleCommand();

                // Act
                executor.Execute(cmd);

                // Assert
                Assert.IsTrue(cmd.Executed, "The command should have executed successfully");
                Assert.AreEqual(0, executor.UndoCount, "Non-undoable commands should not be stored in the undo history stack");
                Assert.AreEqual(0, executor.RedoCount);

                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator CommandExecutor_ExecutesAndUndoesAndRedoesCommand()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var executor = new CommandExecutor();
                var cmd = new UndoableCommand(100, 200);

                // Act - 1. Execute
                executor.Execute(cmd);

                // Assert Execution
                Assert.AreEqual(200, cmd.State, "State should be changed to target state");
                Assert.AreEqual(1, executor.UndoCount, "Undo history should store the executed command");
                Assert.AreEqual(0, executor.RedoCount);

                // Act - 2. Undo
                executor.Undo();

                // Assert Undo
                Assert.AreEqual(100, cmd.State, "State should revert back to original state on Undo");
                Assert.AreEqual(0, executor.UndoCount);
                Assert.AreEqual(1, executor.RedoCount, "Redo history should store the undone command");

                // Act - 3. Redo
                executor.Redo();

                // Assert Redo
                Assert.AreEqual(200, cmd.State, "State should be restored back to target state on Redo");
                Assert.AreEqual(1, executor.UndoCount);
                Assert.AreEqual(0, executor.RedoCount);

                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator CommandExecutor_ExecutesSimpleAsyncCommand()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var executor = new CommandExecutor();
                var cmd = new SimpleAsyncCommand();

                // Act
                await executor.ExecuteAsync(cmd);

                // Assert
                Assert.IsTrue(cmd.Executed, "Async command should execute successfully");
                Assert.AreEqual(0, executor.UndoAsyncCount, "Non-undoable async commands should not be stored in history");
                Assert.AreEqual(0, executor.RedoAsyncCount);
            });
        }

        [UnityTest]
        public IEnumerator CommandExecutor_ExecutesAndUndoesAndRedoesAsyncCommand()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var executor = new CommandExecutor();
                var cmd = new UndoableAsyncCommand(10, 20);

                // Act - 1. Execute
                await executor.ExecuteAsync(cmd);

                // Assert Execution
                Assert.AreEqual(20, cmd.State);
                Assert.AreEqual(1, executor.UndoAsyncCount);
                Assert.AreEqual(0, executor.RedoAsyncCount);

                // Act - 2. Undo
                await executor.UndoAsync();

                // Assert Undo
                Assert.AreEqual(10, cmd.State);
                Assert.AreEqual(0, executor.UndoAsyncCount);
                Assert.AreEqual(1, executor.RedoAsyncCount);

                // Act - 3. Redo
                await executor.RedoAsync();

                // Assert Redo
                Assert.AreEqual(20, cmd.State);
                Assert.AreEqual(1, executor.UndoAsyncCount);
                Assert.AreEqual(0, executor.RedoAsyncCount);
            });
        }

        [UnityTest]
        public IEnumerator CommandExecutor_ClearsHistory()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var executor = new CommandExecutor();
                var syncCmd = new UndoableCommand(1, 2);
                var asyncCmd = new UndoableAsyncCommand(3, 4);

                executor.Execute(syncCmd);
                await executor.ExecuteAsync(asyncCmd);

                Assert.AreEqual(1, executor.UndoCount);
                Assert.AreEqual(1, executor.UndoAsyncCount);

                // Act
                executor.ClearHistory();

                // Assert
                Assert.AreEqual(0, executor.UndoCount);
                Assert.AreEqual(0, executor.RedoCount);
                Assert.AreEqual(0, executor.UndoAsyncCount);
                Assert.AreEqual(0, executor.RedoAsyncCount);
            });
        }
    }
}
