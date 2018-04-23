using System;
using System.Windows;
using System.Windows.Input;

namespace DX.Xpf.DnD {
    // Provides app level feedback to the Drag and Drop manager
    public interface IDragAndDropHandler {
        bool CanStartDrag(UIElement source, MouseButtonEventArgs e, ref object dragData);
        FrameworkElement CreateDragThumb(UIElement source, UIElement target, object dragData);
    }
}