Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Input

Namespace DX.Xpf.DnD
	' Provides app level feedback to the Drag and Drop manager
	Public Interface IDragAndDropHandler
		Function CanStartDrag(ByVal source As UIElement, ByVal e As MouseButtonEventArgs, ByRef dragData As Object) As Boolean
		Function CreateDragThumb(ByVal source As UIElement, ByVal target As UIElement, ByVal dragData As Object) As FrameworkElement
	End Interface
End Namespace