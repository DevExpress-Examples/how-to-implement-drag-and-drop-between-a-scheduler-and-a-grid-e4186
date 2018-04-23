Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media
Imports DevExpress.Xpf.Core
Imports DevExpress.Xpf.Core.Native
Imports System.Windows.Media.Imaging

Namespace DX.Xpf.DnD
	Public NotInheritable Class DragAndDropManager
		Private Class DragAndDropHelper
			Implements ISupportDragDrop
			Private _source As FrameworkElement
			Private _sourceHelper As DragDropElementHelper
			Private _handler As IDragAndDropHandler

			Private _dataContainer As New DragAndDropDataContainer()
			Public ReadOnly Property DataContainer() As DragAndDropDataContainer
				Get
					Return _dataContainer
				End Get
			End Property

			Public Sub New(ByVal source As FrameworkElement)
				_source = source
				_sourceHelper = New DnDDragDropElementHelper(Me, False)
			End Sub

			Friend Sub SetHandler(ByVal handler As IDragAndDropHandler)
				_handler = handler
			End Sub

			Public Sub OnPreviewMouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
				_sourceHelper.OnPreviewMouseLeftButtonDown(sender, e)
			End Sub

			Private Function CanStartDrag(ByVal sender As Object, ByVal e As MouseButtonEventArgs) As Boolean Implements ISupportDragDrop.CanStartDrag
				If _handler Is Nothing Then
					Return True
				End If
				Dim dragData As Object = Nothing
				Dim retVal As Boolean = _handler.CanStartDrag(_source, e, dragData)
				_dataContainer.DragData = dragData
				Return retVal
			End Function

			Private Function CreateDragElement(ByVal offset As Point) As IDragElement Implements ISupportDragDrop.CreateDragElement
				Return New DragAndDropElement(_source, _handler, _dataContainer)
			End Function

			Public Sub ResetDragThumb()
				Dim elem As DragAndDropElement = TryCast(Me._sourceHelper.DragElement, DragAndDropElement)
				If elem IsNot Nothing Then
					elem.ResetDragThumb()
				End If
			End Sub

			Public Sub UpdateDragOverThumb(ByVal target As UIElement)
				Dim elem As DragAndDropElement = TryCast(Me._sourceHelper.DragElement, DragAndDropElement)
				If elem IsNot Nothing Then
					elem.UpdateDragOverThumb(target)
				End If
			End Sub

			Private Function CreateEmptyDropTarget() As IDropTarget Implements ISupportDragDrop.CreateEmptyDropTarget
				Return Nothing
			End Function

			Private Function GetTopLevelDropContainers() As IEnumerable(Of UIElement) Implements ISupportDragDrop.GetTopLevelDropContainers
				Return New List(Of UIElement) (New UIElement() {Application.Current.RootVisual})
			End Function

			Private Function IsCompatibleDropTargetFactory(ByVal factory As IDropTargetFactory) As Boolean
				Return True
			End Function

			Private ReadOnly Property SourceElement() As FrameworkElement Implements ISupportDragDrop.SourceElement
				Get
					Return _source
				End Get
			End Property

			Private Function IsCompatibleDropTargetFactory(ByVal factory As IDropTargetFactory, ByVal dropTargetElement As UIElement) As Boolean Implements ISupportDragDrop.IsCompatibleDropTargetFactory
				Return TypeOf factory Is DragAndDropTargetFactory
			End Function
		End Class

		' Private Attached Property to hold the DragAndDropHelper
		Private Shared ReadOnly DragAndDropHelperProperty As DependencyProperty = DependencyProperty.RegisterAttached("DragAndDropHelper", GetType(DragAndDropHelper), GetType(DragAndDropManager), New PropertyMetadata(Nothing))

		' If AllowDrag is set on an element, that element will be a drag source...
		Private Sub New()
		End Sub
		Public Shared Function GetAllowDrag(ByVal obj As DependencyObject) As Boolean
			Return CBool(obj.GetValue(AllowDragProperty))
		End Function

		Public Shared Sub SetAllowDrag(ByVal obj As DependencyObject, ByVal value As Boolean)
			obj.SetValue(AllowDragProperty, value)
		End Sub

		Public Shared ReadOnly AllowDragProperty As DependencyProperty = DependencyProperty.RegisterAttached("AllowDrag", GetType(Boolean), GetType(DragAndDropManager), New PropertyMetadata(False, AddressOf OnAllowDragChanged))

		Private Shared Sub OnAllowDragChanged(ByVal sender As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
			Dim source As FrameworkElement = TryCast(sender, FrameworkElement)
			If CBool(e.NewValue) Then
				source.SetValue(DragAndDropHelperProperty, New DragAndDropHelper(source))
				source.AddHandler(UIElement.MouseLeftButtonDownEvent, New MouseButtonEventHandler(AddressOf OnTargetMouseLeftButtonDown), True)
			Else
				source.RemoveHandler(UIElement.MouseLeftButtonDownEvent, New MouseButtonEventHandler(AddressOf OnTargetMouseLeftButtonDown))
				source.SetValue(DragAndDropHelperProperty, Nothing)
			End If
		End Sub

		Public Class DragAndDropTargetFactory
			Implements IDropTargetFactory
			Private _target As UIElement
			Public Sub New(ByVal target As UIElement)
				_target = target
			End Sub

			Private Function CreateDropTarget(ByVal dropTargetElement As UIElement) As IDropTarget Implements IDropTargetFactory.CreateDropTarget
				Return New DropAndTarget(_target)
			End Function
		End Class

		Public Class DropAndTarget
			Implements IDropTarget
			Private _target As UIElement
			Public Sub New(ByVal target As UIElement)
				_target = target
			End Sub

			Private Shared Function GetDragData(ByVal source As UIElement) As Object
				If source Is Nothing Then
					Return Nothing
				End If
				Dim dragDropHelper As DragAndDropHelper = TryCast(source.GetValue(DragAndDropHelperProperty), DragAndDropHelper)
				If dragDropHelper IsNot Nothing Then
					Return dragDropHelper.DataContainer.DragData
				End If

				Return Nothing
			End Function

			Private Shared Sub ResetDragThumb(ByVal source As UIElement)
				If source IsNot Nothing Then
					Dim dragDropHelper As DragAndDropHelper = TryCast(source.GetValue(DragAndDropHelperProperty), DragAndDropHelper)
					If dragDropHelper IsNot Nothing Then
						dragDropHelper.ResetDragThumb()
					End If
				End If
			End Sub

			Private Shared Sub UpdateDragOverThumb(ByVal source As UIElement, ByVal target As UIElement)
				If source IsNot Nothing Then
					Dim dragDropHelper As DragAndDropHelper = TryCast(source.GetValue(DragAndDropHelperProperty), DragAndDropHelper)
					If dragDropHelper IsNot Nothing Then
						dragDropHelper.UpdateDragOverThumb(target)
					End If
				End If
			End Sub

			#Region "IDropTarget Members"

			Private Sub IDropTarget_Drop(ByVal source As UIElement, ByVal pt As Point) Implements IDropTarget.Drop
				If (Not RaiseDragOver(source, pt)) Then
					Return
				End If
				If _target Is Nothing Then
					Return
				End If
				Dim instance As Object
				Dim drop As MethodInfo = GetDropEvent(_target, instance)
				If drop IsNot Nothing Then
					drop.Invoke(instance, New Object() { _target, New DragAndDropEventArgs(source, _target, GetDragData(source), pt) })
				End If
			End Sub

			Private _lastSource As UIElement
			Private Sub OnDragLeave() Implements IDropTarget.OnDragLeave
				ResetDragThumb(_lastSource)
				If _target Is Nothing Then
					Return
				End If
				Dim instance As Object
				Dim dragLeave As MethodInfo = GetDragLeaveEvent(_target, instance)
				If dragLeave IsNot Nothing Then
					dragLeave.Invoke(instance, New Object() { _target, New DragAndDropEventArgs(Nothing, _target, Nothing, New Point()) })
				End If
			End Sub

			Private Function RaiseDragOver(ByVal source As UIElement, ByVal pt As Point) As Boolean
				_lastSource = source
				If _target Is Nothing Then
					UpdateDragOverThumb(source, Nothing)
					Return False
				End If
				Dim instance As Object
				Dim dragOver As MethodInfo = GetDragOverEvent(_target, instance)
				If dragOver IsNot Nothing Then
					Dim args As New DragAndDropEventArgs(source, _target, GetDragData(source), pt)
					dragOver.Invoke(instance, New Object() { _target, args })
					UpdateDragOverThumb(source,If(args.Accept, _target, Nothing))
					Return args.Accept
				End If
				Return False
			End Function

			Private Sub OnDragOver(ByVal source As UIElement, ByVal pt As Point) Implements IDropTarget.OnDragOver
				RaiseDragOver(source, pt)
			End Sub

			#End Region
			Private privateIndex As Integer
			Private Property Index() As Integer
				Get
					Return privateIndex
				End Get
				Set(ByVal value As Integer)
					privateIndex = value
				End Set
			End Property
		End Class

		Private Shared Function GetEvent(ByVal name As String, ByVal target As DependencyObject, <System.Runtime.InteropServices.Out()> ByRef instance As Object) As MethodInfo
			If String.IsNullOrEmpty(name) OrElse target Is Nothing Then
				instance = Nothing
				Return Nothing
			End If
			Dim methodArgs() As Type = { GetType(Object), GetType(DragAndDropEventArgs) }
			Dim bindingFlags As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.FlattenHierarchy Or BindingFlags.IgnoreCase
			Dim methodInfo As MethodInfo = target.GetType().GetMethod(name, bindingFlags)
			If methodInfo IsNot Nothing Then
				instance = target
				Return methodInfo
			End If
			Return GetEvent(name, VisualTreeHelper.GetParent(target), instance)
		End Function

		Private Shared Function GetDragOverEvent(ByVal target As DependencyObject, <System.Runtime.InteropServices.Out()> ByRef instance As Object) As MethodInfo
			Return GetEvent(CStr(target.ReadLocalValue(DragOverProperty)), target, instance)
		End Function

		Private Shared Function GetDragLeaveEvent(ByVal target As DependencyObject, <System.Runtime.InteropServices.Out()> ByRef instance As Object) As MethodInfo
			Return GetEvent(CStr(target.ReadLocalValue(DragLeaveProperty)), target, instance)
		End Function

		Private Shared Function GetDropEvent(ByVal target As DependencyObject, <System.Runtime.InteropServices.Out()> ByRef instance As Object) As MethodInfo
			Return GetEvent(CStr(target.ReadLocalValue(DropProperty)), target, instance)
		End Function

		' DragOver
		Public Shared Function GetDragOver(ByVal obj As DependencyObject) As String
			Return CStr(obj.GetValue(DragOverProperty))
		End Function

		Public Shared Sub SetDragOver(ByVal obj As DependencyObject, ByVal value As String)
			obj.SetValue(DragOverProperty, value)
		End Sub

		Public Shared ReadOnly DragOverProperty As DependencyProperty = DependencyProperty.RegisterAttached("DragOver", GetType(String), GetType(DragAndDropManager), Nothing)


		' DragLeave
		Public Shared Function GetDragLeave(ByVal obj As DependencyObject) As String
			Return CStr(obj.GetValue(DragLeaveProperty))
		End Function

		Public Shared Sub SetDragLeave(ByVal obj As DependencyObject, ByVal value As String)
			obj.SetValue(DragLeaveProperty, value)
		End Sub

		Public Shared ReadOnly DragLeaveProperty As DependencyProperty = DependencyProperty.RegisterAttached("DragLeave", GetType(String), GetType(DragAndDropManager), Nothing)


		' Drop
		Public Shared Function GetDrop(ByVal obj As DependencyObject) As String
			Return CStr(obj.GetValue(DropProperty))
		End Function

		Public Shared Sub SetDrop(ByVal obj As DependencyObject, ByVal value As String)
			obj.SetValue(DropProperty, value)
		End Sub

		Public Shared ReadOnly DropProperty As DependencyProperty = DependencyProperty.RegisterAttached("Drop", GetType(String), GetType(DragAndDropManager), Nothing)


		' If AllowDrop is set on an element, that element will be a drag source...
		Public Shared Function GetAllowDrop(ByVal obj As DependencyObject) As Boolean
			Return CBool(obj.GetValue(AllowDropProperty))
		End Function

		Public Shared Sub SetAllowDrop(ByVal obj As DependencyObject, ByVal value As Boolean)
			obj.SetValue(AllowDropProperty, value)
		End Sub

		Public Shared ReadOnly AllowDropProperty As DependencyProperty = DependencyProperty.RegisterAttached("AllowDrop", GetType(Boolean), GetType(DragAndDropManager), New PropertyMetadata(False, AddressOf OnAllowDropChanged))

		Private Shared Sub OnAllowDropChanged(ByVal sender As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
			Dim target As UIElement = TryCast(sender, UIElement)
			If CBool(e.NewValue) Then
				DragManager.SetDropTargetFactory(target, New DragAndDropTargetFactory(target))
			Else
				DragManager.SetDropTargetFactory(target, Nothing)
			End If
		End Sub

		' Private Attached Property to hold the IDragAndDropHandler
		Private Shared ReadOnly DragAndDropHandlerProperty As DependencyProperty = DependencyProperty.RegisterAttached("DragAndDropHandler", GetType(IDragAndDropHandler), GetType(DragAndDropManager), New PropertyMetadata(Nothing))

		Public Shared Sub RegisterHandler(ByVal source As FrameworkElement, ByVal handler As IDragAndDropHandler)
			If source IsNot Nothing Then
				source.SetValue(DragAndDropHandlerProperty, handler)
			End If
		End Sub

		Private Shared Sub OnTargetMouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			Dim source As DependencyObject = TryCast(sender, DependencyObject)
			If source Is Nothing Then
				Return
			End If
			If CBool(source.GetValue(AllowDragProperty)) Then
				Dim dragDropHelper As DragAndDropHelper = TryCast(source.GetValue(DragAndDropHelperProperty), DragAndDropHelper)
				If dragDropHelper IsNot Nothing Then
					dragDropHelper.SetHandler(TryCast(source.GetValue(DragAndDropHandlerProperty), IDragAndDropHandler))
					dragDropHelper.OnPreviewMouseLeftButtonDown(source, e)
				End If
			End If
		End Sub

		Public Shared Function GetDropCursor() As BitmapImage
			Return New BitmapImage(New Uri("DragAndDrop\Cursors\Drop.png", UriKind.RelativeOrAbsolute))
		End Function
		Public Shared Function GetDragCursor() As BitmapImage
			Return New BitmapImage(New Uri("DragAndDrop\Cursors\Drag.png", UriKind.RelativeOrAbsolute))
		End Function
	End Class

	Public Class DnDDragDropElementHelper
		Inherits DragDropElementHelper
		Public Sub New(ByVal supDnD As ISupportDragDrop, ByVal isRelative As Boolean)
			MyBase.New(supDnD, isRelative)
		End Sub
		Protected Overrides Sub OnLostMouseCapture(ByVal sender As Object, ByVal e As MouseEventArgs)
			If IsReleaseBeforeStartDragging Then
				IsReleaseBeforeStartDragging = False
			End If
			If (Not IsDragging) Then
				IsMouseDown = False
			End If
		End Sub
	End Class
End Namespace
