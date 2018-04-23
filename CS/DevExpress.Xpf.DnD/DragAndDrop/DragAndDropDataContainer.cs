using System;

namespace DX.Xpf.DnD {
    public class DragAndDropDataContainer {
        private object _dragData;
        public object DragData {
            get {
                return _dragData;
            }
            set {
                _dragData = value;
            }
        }
    }
}
