using System;
using System.Windows.Input;
using System.Windows;
using VFSBrowser.Annotations;

namespace VFSBrowser.ViewModel
{
    /// <summary>
    /// FROM: http://www.wpfsharp.com/2012/03/22/mvvm-and-drag-and-drop-command-binding-with-an-attached-behavior/
    /// 
    /// This is an Attached Behavior and is intended for use with
    /// XAML objects to enable binding a drag and drop event to
    /// an ICommand.
    /// </summary>
    public static class DropBehavior
    {
        /// <summary>
        /// Dependency property so we can bind to a Command in the ViewModel
        /// </summary>
        private static readonly DependencyProperty DropCommandProperty = 
                                DependencyProperty.RegisterAttached ("DropCommand", 
                                                                     typeof(ICommand), 
                                                                     typeof(DropBehavior), 
                                                                     new PropertyMetadata(DropCommandPropertyChangedCallBack));

        /// <summary>
        /// Extension method for all UIElements, so we can set the DropCommand in the XAML.
        /// </summary>
        public static void SetDropCommand(this UIElement uiElement, ICommand command)
        {
            if (uiElement == null) throw new ArgumentNullException("uiElement");

            uiElement.SetValue(DropCommandProperty, command);
        }

        /// <summary>
        /// Returns the DropCommand of an UIElement
        /// </summary>
        private static ICommand GetDropCommand(UIElement uiElement)
        {
            return (ICommand)uiElement.GetValue(DropCommandProperty);
        }

        /// <summary>
        /// Adds a handler to the Drop-Event of the UIElement. The handler executes the DropCommand of the Element.
        /// </summary>
        private static void DropCommandPropertyChangedCallBack (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var uiElement = dependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.Drop += (sender, dragArgs) =>
            {
                GetDropCommand(uiElement).Execute(dragArgs);
                dragArgs.Handled = true;
            };
        }
    }
}