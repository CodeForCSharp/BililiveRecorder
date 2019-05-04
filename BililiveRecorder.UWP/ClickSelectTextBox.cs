/**
 * Author: Donnelle
 * From: https://stackoverflow.com/a/661224/8877830 
 * */
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace BililiveRecorder.UWP
{
    public class ClickSelectTextBox : TextBox
    {
        public ClickSelectTextBox()
        {
            PointerPressed += SelectivelyIgnoreMouseButton;
            GotFocus += SelectAllText;
            DoubleTapped += SelectAllText;
        }

        private static void SelectivelyIgnoreMouseButton(object sender,
                                                         PointerRoutedEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (textBox.FocusState != FocusState.Keyboard)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus(FocusState.Pointer);
                    e.Handled = true;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox)
                textBox.SelectAll();
        }
    }
}
