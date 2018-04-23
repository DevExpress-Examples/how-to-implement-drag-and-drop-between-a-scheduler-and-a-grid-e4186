Imports Microsoft.VisualBasic
Imports System
Imports System.Windows

Namespace DX.Xpf.DnD
	Public NotInheritable Class DragAndDropEventArgs
		Inherits EventArgs
		Private _dragData As Object
		Private _source As UIElement
		Private _target As UIElement
		Private _location As Point
		Private _accept As Boolean
		Friend Sub New(ByVal source As UIElement, ByVal target As UIElement, ByVal dragData As Object, ByVal pt As Point)
			_location = pt
			_source = source
			_target = target
			_dragData = dragData
		End Sub
		Public Property Accept() As Boolean
			Get
				Return _accept
			End Get
			Set(ByVal value As Boolean)
				_accept = value
			End Set
		End Property
		Public ReadOnly Property Source() As UIElement
			Get
				Return _source
			End Get
		End Property
		Public ReadOnly Property Target() As UIElement
			Get
				Return _target
			End Get
		End Property
		Public ReadOnly Property DragData() As Object
			Get
				Return _dragData
			End Get
		End Property
		Public ReadOnly Property Location() As Point
			Get
				Return _location
			End Get
		End Property
	End Class
End Namespace