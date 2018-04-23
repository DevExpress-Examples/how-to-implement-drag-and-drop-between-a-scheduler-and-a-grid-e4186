Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports DevExpress.Xpf.Core
Imports System.Windows.Media.Imaging
Imports System.IO

Namespace DX.Xpf.DnD
	Public Class DragAndDropElement
		Implements IDragElement
		Private _source As FrameworkElement
		Public ReadOnly Property Source() As FrameworkElement
			Get
				Return _source
			End Get
		End Property
		Public Sub New(ByVal source As FrameworkElement, ByVal handler As IDragAndDropHandler, ByVal dataContainer As DragAndDropDataContainer)
			_dataContainer = dataContainer
			_source = source
			_handler = handler
			InitDragThumb()
		End Sub
		Private _handler As IDragAndDropHandler
		Public ReadOnly Property Handler() As IDragAndDropHandler
			Get
				Return _handler
			End Get
		End Property
		Private _popup As Popup
		Public ReadOnly Property Popup() As Popup
			Get
				Return _popup
			End Get
		End Property
		Private Sub InitDragThumb()
			_popup = New Popup()
			_popup.Child = CreateDragThumb()
			_popup.IsOpen = True
			If _defaultThumb AndAlso _source IsNot Nothing Then
				_source.Cursor = Cursors.None
			End If
		End Sub
		Private _defaultThumb As Boolean = True
		Private _dataContainer As DragAndDropDataContainer
		Public Overridable Function CreateDragThumb() As FrameworkElement
			_defaultThumb = True
			If _handler IsNot Nothing Then
				Dim thumb As FrameworkElement = _handler.CreateDragThumb(_source, Nothing,_dataContainer.DragData)
				If thumb IsNot Nothing Then
					_defaultThumb = False
					Return thumb
				End If
			End If

			Dim iconStream As Stream = GetResourceStream("Drag.png")
			If iconStream IsNot Nothing Then
				Dim bi As New BitmapImage()
				bi.SetSource(iconStream)
				Dim cursor As Image = New Image With {.Source = bi}
				Return cursor
			End If
			Return Nothing
		End Function
		Public Overridable Function CreateDropThumb(ByVal target As UIElement) As FrameworkElement
			If target Is Nothing Then
				Return Nothing
			End If
			If _handler IsNot Nothing Then
				Dim thumb As FrameworkElement = _handler.CreateDragThumb(_source, target, _dataContainer.DragData)
				If thumb IsNot Nothing Then
					Return thumb
				End If
			End If

			Dim iconStream As Stream = GetResourceStream("Drop.png")
			If iconStream IsNot Nothing Then
				Dim bi As New BitmapImage()
				bi.SetSource(iconStream)
				Dim cursor As Image = New Image With {.Source = bi}
				Return cursor
			End If
			Return Nothing
		End Function
		Private Function GetResourceStream(ByVal name As String) As Stream
'INSTANT VB WARNING: VB & C# use different formats for embedded resource names. You may need to adjust references to these names:
			Dim names() As String = GetType(DragAndDropManager).Assembly.GetManifestResourceNames()
			Dim fullResourceName As String = String.Empty
			For Each item As String In names
				If item.Contains(name) Then
					fullResourceName = item
				End If
			Next item
			Return GetType(DragAndDropManager).Assembly.GetManifestResourceStream(fullResourceName)
		End Function
		Public Overridable Sub ResetDragThumb()
			_popup.Child = CreateDragThumb()
		End Sub
		Public Overridable Sub UpdateDragOverThumb(ByVal target As UIElement)
			Dim child = CreateDropThumb(target)
			If child IsNot Nothing Then
				_popup.Child = child
			End If
		End Sub
		#Region "IDragElement Members"
		Private Sub Destroy() Implements IDragElement.Destroy
			If _defaultThumb AndAlso _source IsNot Nothing Then
				_source.Cursor = Cursors.Arrow
			End If
			If _popup IsNot Nothing Then
				_popup.Child = Nothing
				_popup.IsOpen = False
			End If
			_popup = Nothing
		End Sub
		Private Sub UpdateLocation(ByVal newPos As Point) Implements IDragElement.UpdateLocation
			Dim pt As Point = newPos
			If _popup IsNot Nothing Then

				If _source IsNot Nothing Then
					pt = TranslatePoint(pt)
				End If
				If _defaultThumb Then
					pt.X -= 0
					pt.Y -= 0
				Else
					pt.X -= 14
					pt.Y -= 14
				End If
				_popup.HorizontalOffset = pt.X
				_popup.VerticalOffset = pt.Y
			End If
		End Sub
		Private Function TranslatePoint(ByVal position As Point) As Point
			Dim elementPosition As Point = _source.GetPosition(TryCast(Application.Current.RootVisual, FrameworkElement))
			Return New Point(elementPosition.X + position.X, elementPosition.Y + position.Y)
		End Function
		#End Region
	End Class
End Namespace
