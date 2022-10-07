using System;
using System.Windows;
using System.Windows.Input;

namespace Flagrum.Desktop.Architecture;

public static class InputBinding
{
    /// <summary>
    /// <see cref="DependencyProperty" /> for methods <see cref="GetGesture(System.Windows.Input.InputBinding)" />
    /// and <see cref="SetGesture(System.Windows.Input.InputBinding, InputGesture)" />.
    /// </summary>
    public static readonly DependencyProperty GestureProperty =
        DependencyProperty.RegisterAttached(
            nameof(GetGesture).Substring(3),
            typeof(InputGesture),
            typeof(InputBinding),
            new PropertyMetadata(null, OnGestureChanged));

    /// <summary>Returns the value of the attached property Gesture for the <paramref name="inputBinding" />.</summary>
    /// <param name="inputBinding"><see cref="System.Windows.Input.InputBinding" />  whose property value will be returned.</param>
    /// <returns>Property value <see cref="InputGesture" />.</returns>
    public static InputGesture GetGesture(System.Windows.Input.InputBinding inputBinding)
    {
        return (InputGesture)inputBinding.GetValue(GestureProperty);
    }

    /// <summary>Sets the value of the Gesture attached property to <paramref name="inputBinding" />.</summary>
    /// <param name="inputBinding"><see cref="System.Windows.Input.InputBinding" /> whose property is setting to a value..</param>
    /// <param name="value"><see cref="InputGesture" /> value for property.</param>
    public static void SetGesture(System.Windows.Input.InputBinding inputBinding, InputGesture value)
    {
        inputBinding.SetValue(GestureProperty, value);
    }

    private static void OnGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not System.Windows.Input.InputBinding inputBinding)
        {
            throw new NotImplementedException(
                $"Implemented only for the \"{typeof(System.Windows.Input.InputBinding).FullName}\" class");
        }

        inputBinding.Gesture = (InputGesture)e.NewValue;
    }
}