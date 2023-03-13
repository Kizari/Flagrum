using System;
using System.Windows.Input;

namespace Flagrum.Desktop.Architecture;

public class RelayCommand : ICommand
{
    private readonly Action _action;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action action) : this(action, () => true) {}
    public RelayCommand(Action action, Func<bool> canExecute) 
    { 
        _action = action; 
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute();
    }

    public void Execute(object? parameter)
    {
        _action();
    }

    public event EventHandler? CanExecuteChanged;
}