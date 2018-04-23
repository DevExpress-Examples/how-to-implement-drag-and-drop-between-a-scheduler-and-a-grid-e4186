using System;
using System.Windows;

namespace DX.Xpf.DnD {
    public sealed class DragAndDropEventArgs : EventArgs {
        private object _dragData;
        private UIElement _source;
        private UIElement _target;
        private Point _location;
        private bool _accept;
        internal DragAndDropEventArgs(UIElement source, UIElement target, object dragData, Point pt) {
            _location = pt;
            _source = source;
            _target = target;
            _dragData = dragData;
        }
        public bool Accept {
            get {
                return _accept;
            }
            set {
                _accept = value;
            }
        }
        public UIElement Source {
            get {
                return _source;
            }
        }
        public UIElement Target {
            get {
                return _target;
            }
        }
        public object DragData {
            get {
                return _dragData;
            }
        }
        public Point Location {
            get {
                return _location;
            }
        }
    }
}