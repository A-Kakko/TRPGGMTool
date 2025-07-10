using System.Windows.Input;

namespace TRPGGMTool.Commands
{
    /// <summary>
    /// 汎用的なRelayCommandの実装
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _validationFunc;

        public RelayCommand(Action execute, Func<bool>? validationFunc = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _validationFunc = validationFunc;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _validationFunc?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// パラメータ付きRelayCommand
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<bool>? _validationFunc;

        public RelayCommand(Action<T?> execute, Func<bool>? validationFunc = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _validationFunc = validationFunc;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _validationFunc?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }
    }
}