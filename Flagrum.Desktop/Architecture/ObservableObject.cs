using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Flagrum.Desktop.Architecture;

public class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string info = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    protected bool SetValue<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(backingField, value))
        {
            return false;
        }

        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}