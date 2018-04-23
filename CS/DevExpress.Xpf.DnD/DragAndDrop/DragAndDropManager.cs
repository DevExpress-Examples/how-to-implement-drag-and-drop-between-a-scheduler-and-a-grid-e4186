using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using System.Windows.Media.Imaging;

namespace DX.Xpf.DnD
{
	public static class DragAndDropManager
	{
		class DragAndDropHelper : ISupportDragDrop
		{
			FrameworkElement _source;
			DragDropElementHelper _sourceHelper;
			IDragAndDropHandler _handler;

			DragAndDropDataContainer _dataContainer = new DragAndDropDataContainer();
			public DragAndDropDataContainer DataContainer
			{
				get { return _dataContainer; }
			}

			public DragAndDropHelper(FrameworkElement source)
			{
				_source = source;
                _sourceHelper = new DnDDragDropElementHelper(this, false);
			}

			internal void SetHandler(IDragAndDropHandler handler)
			{
				_handler = handler;
			}

			public void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
			{
				_sourceHelper.OnPreviewMouseLeftButtonDown(sender, e);
			}

			bool ISupportDragDrop.CanStartDrag(object sender, MouseButtonEventArgs e)
			{
				if ( _handler == null )
				{
					return true;
				}
				object dragData = null;
				bool retVal = _handler.CanStartDrag(_source,
				e,
				ref dragData);
				_dataContainer.DragData = dragData;
				return retVal;
			}

			IDragElement ISupportDragDrop.CreateDragElement(Point offset)
			{
				return new DragAndDropElement(_source, _handler, _dataContainer);
			}

			public void ResetDragThumb()
			{
				DragAndDropElement elem = this._sourceHelper.DragElement as DragAndDropElement;
				if ( elem != null )
				{
					elem.ResetDragThumb();
				}
			}

			public void UpdateDragOverThumb(UIElement target)
			{
				DragAndDropElement elem = this._sourceHelper.DragElement as DragAndDropElement;
				if ( elem != null )
				{
					elem.UpdateDragOverThumb(target);
				}
			}

			IDropTarget ISupportDragDrop.CreateEmptyDropTarget()
			{
				return null;
			}

			IEnumerable<UIElement> ISupportDragDrop.GetTopLevelDropContainers()
			{
				return new List<UIElement>() { Application.Current.RootVisual };
			}

			bool IsCompatibleDropTargetFactory(IDropTargetFactory factory)
			{
				return true;
			}

			FrameworkElement ISupportDragDrop.SourceElement
			{
				get { return _source; }
			}

			bool ISupportDragDrop.IsCompatibleDropTargetFactory(IDropTargetFactory factory, UIElement dropTargetElement)
			 {
				return factory is DragAndDropTargetFactory;
			}
        }

		// Private Attached Property to hold the DragAndDropHelper
		private static readonly DependencyProperty DragAndDropHelperProperty = DependencyProperty.RegisterAttached("DragAndDropHelper", typeof(DragAndDropHelper), typeof(DragAndDropManager), new PropertyMetadata(null));

		// If AllowDrag is set on an element, that element will be a drag source...
		public static bool GetAllowDrag(DependencyObject obj)
		{
			return (bool)obj.GetValue(AllowDragProperty);
		}

		public static void SetAllowDrag(DependencyObject obj, bool value)
		{
			obj.SetValue(AllowDragProperty, value);
		}

		public static readonly DependencyProperty AllowDragProperty = DependencyProperty.RegisterAttached("AllowDrag", typeof(bool), typeof(DragAndDropManager), new PropertyMetadata(false, OnAllowDragChanged));

		static void OnAllowDragChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement source = sender as FrameworkElement;
			if ( (bool)e.NewValue )
			{
				source.SetValue(DragAndDropHelperProperty, new DragAndDropHelper(source));
				source.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnTargetMouseLeftButtonDown), true);
			}
			else
			{
				source.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnTargetMouseLeftButtonDown));
				source.SetValue(DragAndDropHelperProperty, null);
			}
		}

		public class DragAndDropTargetFactory : IDropTargetFactory
		{
			private UIElement _target;
			public DragAndDropTargetFactory(UIElement target)
			{
				_target = target;
			}

			IDropTarget IDropTargetFactory.CreateDropTarget(UIElement dropTargetElement)
			{
				return new DropAndTarget(_target);
			}
		}

		public class DropAndTarget : IDropTarget
		{
			UIElement _target;
			public DropAndTarget(UIElement target)
			{
				_target = target;
			}

			static object GetDragData(UIElement source)
			{
				if ( source == null )
					return null;
				DragAndDropHelper dragDropHelper = source.GetValue(DragAndDropHelperProperty) as DragAndDropHelper;
				if ( dragDropHelper != null )
					return dragDropHelper.DataContainer.DragData;
				
				return null;
			}

			static void ResetDragThumb(UIElement source)
			{
				if ( source != null )
				{
					DragAndDropHelper dragDropHelper = source.GetValue(DragAndDropHelperProperty) as DragAndDropHelper;
					if ( dragDropHelper != null )
					{
						dragDropHelper.ResetDragThumb();
					}
				}
			}

			static void UpdateDragOverThumb(UIElement source, UIElement target)
			{
				if ( source != null )
				{
					DragAndDropHelper dragDropHelper = source.GetValue(DragAndDropHelperProperty) as DragAndDropHelper;
					if ( dragDropHelper != null )
					{
						dragDropHelper.UpdateDragOverThumb(target);
					}
				}
			}

			#region IDropTarget Members

			void IDropTarget.Drop(UIElement source, Point pt)
			{
				if ( !RaiseDragOver(source, pt) )
					return;
				if ( _target == null )
					return;
				object instance;
				MethodInfo drop = GetDropEvent(_target, out instance);
				if ( drop != null )
				{
					drop.Invoke(instance, new object[] { _target, new DragAndDropEventArgs(source, _target, GetDragData(source), pt) });
				}
			}

			UIElement _lastSource;
			void IDropTarget.OnDragLeave()
			{
				ResetDragThumb(_lastSource);
				if ( _target == null )
					return;
				object instance;
				MethodInfo dragLeave = GetDragLeaveEvent(_target, out instance);
				if ( dragLeave != null )
				{
					dragLeave.Invoke(instance, new object[] { _target, new DragAndDropEventArgs(null, _target, null, new Point()) });
				}
			}

			bool RaiseDragOver(UIElement source, Point pt)
			{
				_lastSource = source;
				if ( _target == null )
				{
					UpdateDragOverThumb(source, null);
					return false;
				}
				object instance;
				MethodInfo dragOver = GetDragOverEvent(_target, out instance);
				if ( dragOver != null )
				{
					DragAndDropEventArgs args = new DragAndDropEventArgs(source, _target, GetDragData(source), pt);
					dragOver.Invoke(instance, new object[] { _target, args });
					UpdateDragOverThumb(source, args.Accept ? _target : null);
					return args.Accept;
				}
				return false;
			}

			void IDropTarget.OnDragOver(UIElement source, Point pt)
			{
				RaiseDragOver(source, pt);
			}

			#endregion
			int Index { get; set; }
		}

		static MethodInfo GetEvent(string name, DependencyObject target,
		out object instance)
		{
			if ( string.IsNullOrEmpty(name) || target == null )
			{
				instance = null;
				return null;
			}
			Type[] methodArgs =
			new Type[] { typeof(object),
				typeof(DragAndDropEventArgs) };
			BindingFlags bindingFlags =
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.FlattenHierarchy |
			BindingFlags.IgnoreCase;
			MethodInfo methodInfo = target.GetType().GetMethod(name, bindingFlags);
			if ( methodInfo != null )
			{
				instance = target;
				return methodInfo;
			}
			return GetEvent(name, VisualTreeHelper.GetParent(target), out instance);
		}

		static MethodInfo GetDragOverEvent(DependencyObject target,
		out object instance)
		{
			return GetEvent((string)target.ReadLocalValue(DragOverProperty), target,
			out instance);
		}

		static MethodInfo GetDragLeaveEvent(DependencyObject target,
		out object instance)
		{
			return GetEvent((string)target.ReadLocalValue(DragLeaveProperty), target,
			out instance);
		}

		static MethodInfo GetDropEvent(DependencyObject target,
		out object instance)
		{
			return GetEvent((string)target.ReadLocalValue(DropProperty), target,
			out instance);
		}

		// DragOver
		public static string GetDragOver(DependencyObject obj)
		{
			return (string)obj.GetValue(DragOverProperty);
		}

		public static void SetDragOver(DependencyObject obj, string value)
		{
			obj.SetValue(DragOverProperty, value);
		}

		public static readonly DependencyProperty DragOverProperty =
		DependencyProperty.RegisterAttached("DragOver", typeof(string), typeof(DragAndDropManager), null);


		// DragLeave
		public static string GetDragLeave(DependencyObject obj)
		{
			return (string)obj.GetValue(DragLeaveProperty);
		}

		public static void SetDragLeave(DependencyObject obj, string value)
		{
			obj.SetValue(DragLeaveProperty, value);
		}

		public static readonly DependencyProperty DragLeaveProperty =
		DependencyProperty.RegisterAttached("DragLeave", typeof(string), typeof(DragAndDropManager), null);


		// Drop
		public static string GetDrop(DependencyObject obj)
		{
			return (string)obj.GetValue(DropProperty);
		}

		public static void SetDrop(DependencyObject obj, string value)
		{
			obj.SetValue(DropProperty, value);
		}

		public static readonly DependencyProperty DropProperty =
		DependencyProperty.RegisterAttached("Drop", typeof(string), typeof(DragAndDropManager), null);


		// If AllowDrop is set on an element, that element will be a drag source...
		public static bool GetAllowDrop(DependencyObject obj)
		{
			return (bool)obj.GetValue(AllowDropProperty);
		}

		public static void SetAllowDrop(DependencyObject obj, bool value)
		{
			obj.SetValue(AllowDropProperty, value);
		}

		public static readonly DependencyProperty AllowDropProperty =
		DependencyProperty.RegisterAttached("AllowDrop", typeof(bool), typeof(DragAndDropManager),
		new PropertyMetadata(false, OnAllowDropChanged));

		static void OnAllowDropChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			UIElement target = sender as UIElement;
			if ( (bool)e.NewValue )
			{
				DragManager.SetDropTargetFactory(target, new DragAndDropTargetFactory(target));
			}
			else
			{
				DragManager.SetDropTargetFactory(target, null);
			}
		}

		// Private Attached Property to hold the IDragAndDropHandler
		private static readonly DependencyProperty DragAndDropHandlerProperty =
		DependencyProperty.RegisterAttached("DragAndDropHandler", typeof(IDragAndDropHandler), typeof(DragAndDropManager),
		new PropertyMetadata(null));

		public static void RegisterHandler(FrameworkElement source, IDragAndDropHandler handler)
		{
			if ( source != null )
			{
				source.SetValue(DragAndDropHandlerProperty, handler);
			}
		}

		static void OnTargetMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject source = sender as DependencyObject;
			if ( source == null )
				return;
			if ( (bool)source.GetValue(AllowDragProperty) )
			{
				DragAndDropHelper dragDropHelper = source.GetValue(DragAndDropHelperProperty) as DragAndDropHelper;
				if ( dragDropHelper != null )
				{
					dragDropHelper.SetHandler(source.GetValue(DragAndDropHandlerProperty) as IDragAndDropHandler);
					dragDropHelper.OnPreviewMouseLeftButtonDown(source, e);
				}
			}
		}

        public static BitmapImage GetDropCursor() {
            return new BitmapImage(new Uri("DragAndDrop\\Cursors\\Drop.png", UriKind.RelativeOrAbsolute));
        }
        public static BitmapImage GetDragCursor() {
            return new BitmapImage(new Uri("DragAndDrop\\Cursors\\Drag.png", UriKind.RelativeOrAbsolute));
        }
	}

    public class DnDDragDropElementHelper : DragDropElementHelper {
        public DnDDragDropElementHelper(ISupportDragDrop supDnD, bool isRelative)
            : base(supDnD, isRelative) { }
        protected override void OnLostMouseCapture(object sender, MouseEventArgs e) {
            if(IsReleaseBeforeStartDragging) {
                IsReleaseBeforeStartDragging = false;
            }
            if(!IsDragging) {
                IsMouseDown = false;
            }
        }
    }
}
