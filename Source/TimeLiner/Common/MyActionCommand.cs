// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimeLiner.Common
{
    internal class MyActionCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, Task> _executeAsync;
        private readonly Func<object, bool> _canExecute;

        private bool _isExecuting;

        public MyActionCommand(Action<object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public MyActionCommand(Action<object> execute, Func<object, bool> canExecute)
            : this(execute)
        {
            _canExecute = canExecute;
        }

        public MyActionCommand(Func<object, Task> executeAsync)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        }

        public MyActionCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute)
            : this(executeAsync)
        {
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public async void Execute(object parameter)
        {
            try
            {
                if (!CanExecute(parameter))
                {
                    return;
                }

                try
                {
                    _isExecuting = true;
                    CommandManager.InvalidateRequerySuggested();

                    if (_executeAsync != null)
                    {
                        await _executeAsync(parameter);
                    }
                    else
                    {
                        _execute(parameter);
                    }
                }
                finally
                {
                    _isExecuting = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch
            {
                // ignore
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_isExecuting)
            {
                return false;
            }

            return _canExecute?.Invoke(parameter) ?? true;
        }
    }
}