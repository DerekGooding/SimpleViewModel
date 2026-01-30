using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace SimpleViewModel.BaseClasses;

/// <summary>
/// Provides a base implementation of <see cref="INotifyPropertyChanged"/> for view models in a WPF MVVM application.
/// Includes property change notification, design mode detection, and a common <c>IsInUse</c> property.
/// </summary>
public class BaseViewModel : INotifyPropertyChanged
{
    private readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="reference">A reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property. This is optional and is automatically provided by the compiler.</param>
    protected void SetProperty<T>(ref T reference, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(reference, value))
            return;

        reference = value;
        RaisePropertyChanged(propertyName);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property. This is optional and is automatically provided by the compiler.</param>
    protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RaisePropertyChanged(string propertyName)
    {
        var handler = PropertyChanged;
        if (handler is null)
            return;

        if (_syncContext != null)
        {
            _syncContext.Post(_ => handler(this, new(propertyName)), null);
        }
        else
        {
            handler(this, new(propertyName));
        }
    }

    /// <summary>
    /// Called when a generated command throws.
    /// Default behavior preserves stack and crashes (correct for debugging).
    /// Override in derived VMs for logging or UI reporting.
    /// </summary>
    public virtual void OnCommandException(Exception ex) => ExceptionDispatchInfo.Capture(ex).Throw();
}
