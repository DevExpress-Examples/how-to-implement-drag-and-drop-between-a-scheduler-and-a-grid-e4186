Imports Microsoft.VisualBasic
Imports System

Namespace DX.Xpf.DnD
	Public Class DragAndDropDataContainer
		Private _dragData As Object
		Public Property DragData() As Object
			Get
				Return _dragData
			End Get
			Set(ByVal value As Object)
				_dragData = value
			End Set
		End Property
	End Class
End Namespace
