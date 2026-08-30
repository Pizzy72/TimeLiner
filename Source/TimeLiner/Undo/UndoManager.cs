// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeLiner.Models;

namespace TimeLiner.Undo
{
    /// <summary>
    /// Manages undo and redo snapshots of the TimeLinesModel.
    /// Stores deep-cloned model instances on stacks to allow undo/redo operations
    /// and tracks a version index to determine saved state.
    /// </summary>
    internal sealed class UndoManager
    {
        /// <summary>
        /// Stack holding previous model states for undo operations.
        /// </summary>
        private readonly Stack<TimeLinesModel> _undoStack = new();

        /// <summary>
        /// Stack holding states that can be reapplied via redo.
        /// </summary>
        private readonly Stack<TimeLinesModel> _redoStack = new();

        /// <summary>
        /// Current version index (incremented on capture, decremented on undo).
        /// </summary>
        private int _currentVersion;

        /// <summary>
        /// Version index that represents the last saved state.
        /// </summary>
        private int _savedVersion;

        /// <summary>
        /// True when the current version equals the saved version.
        /// </summary>
        public bool IsAtSavedVersion => _currentVersion == _savedVersion;

        /// <summary>
        /// Maximum number of undo steps to keep on the stack.
        /// </summary>
        public int MaxUndoSteps { get; set; } = 10;

        /// <summary>
        /// Whether an undo operation is currently available.
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Whether a redo operation is currently available.
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Take a deep clone of the provided model and push it onto the undo stack.
        /// Clears the redo stack and trims the undo stack to the configured maximum.
        /// </summary>
        public async Task CaptureAsync(TimeLinesModel current)
        {
            if (current == null)
            {
                return;
            }

            _undoStack.Push(await current.CloneAsync());
            _redoStack.Clear();

            _currentVersion++;

            TrimUndoStack();
        }

        /// <summary>
        /// Push a clone of the current state to the redo stack and return the last stored state
        /// from the undo stack. If undo is not available or current is null, returns the provided instance.
        /// </summary>
        public async Task<TimeLinesModel> UndoAsync(TimeLinesModel current)
        {
            if (!CanUndo || current == null)
            {
                return current;
            }

            _redoStack.Push(await current.CloneAsync());

            _currentVersion--;

            return _undoStack.Pop();
        }

        /// <summary>
        /// Push a clone of the current state to the undo stack and return the next stored state
        /// from the redo stack. If redo is not available or current is null, returns the provided instance.
        /// </summary>
        public async Task<TimeLinesModel> RedoAsync(TimeLinesModel current)
        {
            if (!CanRedo || current == null)
            {
                return current;
            }

            _undoStack.Push(await current.CloneAsync());

            _currentVersion++;

            return _redoStack.Pop();
        }

        /// <summary>
        /// Remove all undo/redo history and reset version tracking.
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();

            _currentVersion = 0;
            _savedVersion = 0;
        }

        /// <summary>
        /// Record the current version as the saved version.
        /// </summary>
        public void MarkSaved()
        {
            _savedVersion = _currentVersion;
        }

        /// <summary>
        /// Ensure the undo stack does not exceed the configured maximum by keeping only the newest entries.
        /// </summary>
        private void TrimUndoStack()
        {
            if (_undoStack.Count <= MaxUndoSteps)
            {
                return;
            }

            TimeLinesModel[] newest = _undoStack
                .Take(MaxUndoSteps)
                .Reverse()
                .ToArray();

            _undoStack.Clear();

            foreach (TimeLinesModel model in newest)
            {
                _undoStack.Push(model);
            }
        }
    }
}