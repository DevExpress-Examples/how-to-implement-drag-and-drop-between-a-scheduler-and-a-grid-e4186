using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using System.Windows.Media.Imaging;
using System.IO;

namespace DX.Xpf.DnD {
    public class DragAndDropElement : IDragElement {
        FrameworkElement _source;
        public FrameworkElement Source {
            get {
                return _source;
            }
        }
        public DragAndDropElement(FrameworkElement source, IDragAndDropHandler handler, DragAndDropDataContainer dataContainer) {
            _dataContainer = dataContainer;
            _source = source;
            _handler = handler;
            InitDragThumb();
        }
        IDragAndDropHandler _handler;
        public IDragAndDropHandler Handler {
            get {
                return _handler;
            }
        }
        Popup _popup;
        public Popup Popup {
            get {
                return _popup;
            }
        }
        void InitDragThumb() {
            _popup = new Popup();
            _popup.Child = CreateDragThumb();
            _popup.IsOpen = true;
            if(_defaultThumb && _source != null) {
                _source.Cursor = Cursors.None;
            }
        }
        bool _defaultThumb = true;
        private DragAndDropDataContainer _dataContainer;
        public virtual FrameworkElement CreateDragThumb() {
            _defaultThumb = true;
            if (_handler != null) {
                FrameworkElement thumb = _handler.CreateDragThumb(_source, null,_dataContainer.DragData);
                if (thumb != null) {
                    _defaultThumb = false;
                    return thumb;
                }
            }

            Stream iconStream = GetResourceStream("Drag.png");
            if(iconStream != null) {
                BitmapImage bi = new BitmapImage();
                bi.SetSource(iconStream);
                Image cursor = new Image {
                    Source = bi
                };
                return cursor;
            }
            return null;
        }
        public virtual FrameworkElement CreateDropThumb(UIElement target) {
            if (target == null) return null;
            if (_handler != null) {
                FrameworkElement thumb = _handler.CreateDragThumb(_source, target, _dataContainer.DragData);
                if (thumb != null) {
                    return thumb;
                }
            }
            
            Stream iconStream = GetResourceStream("Drop.png");
            if(iconStream != null) {
                BitmapImage bi = new BitmapImage();
                bi.SetSource(iconStream);
                Image cursor = new Image {
                    Source = bi
                };
                return cursor;
            }
            return null;
        }
        Stream GetResourceStream(string name) {
            string[] names = typeof(DragAndDropManager).Assembly.GetManifestResourceNames();
            string fullResourceName = string.Empty;
            foreach(string item in names) {
                if(item.Contains(name)) {
                    fullResourceName = item;
                }
            }
            return typeof(DragAndDropManager).Assembly.GetManifestResourceStream(fullResourceName);
        }
        public virtual void ResetDragThumb() {
            _popup.Child = CreateDragThumb();
        }
        public virtual void UpdateDragOverThumb(UIElement target) {
            var child = CreateDropThumb(target);
            if(child != null) {
                _popup.Child = child;
            }
        }
        #region IDragElement Members
        void IDragElement.Destroy() {
            if (_defaultThumb && _source != null) {
                _source.Cursor = Cursors.Arrow;
            }
            if (_popup != null) {
                _popup.Child = null;
                _popup.IsOpen = false;
            }
            _popup = null;
        }
        void IDragElement.UpdateLocation(Point newPos) {
            Point pt = newPos;
            if(_popup != null) {

                if(_source != null) {
                    pt = TranslatePoint(pt);
                }
                if(_defaultThumb) {
                    pt.X -= 0;
                    pt.Y -= 0;
                } else {
                    pt.X -= 14;
                    pt.Y -= 14;
                }
                _popup.HorizontalOffset = pt.X;
                _popup.VerticalOffset = pt.Y;
            }
        }
        Point TranslatePoint(Point position) {
            Point elementPosition = _source.GetPosition(Application.Current.RootVisual as FrameworkElement);
            return new Point(elementPosition.X + position.X, elementPosition.Y + position.Y);
        }
        #endregion
    }
}
