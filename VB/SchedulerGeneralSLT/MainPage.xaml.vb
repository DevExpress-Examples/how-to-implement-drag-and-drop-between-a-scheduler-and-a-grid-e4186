Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports DevExpress.Xpf.Grid
Imports DevExpress.Xpf.Scheduler
Imports DevExpress.Xpf.Scheduler.Drawing
Imports DevExpress.XtraScheduler
Imports DevExpress.XtraScheduler.Drawing
Imports DevExpress.XtraScheduler.Native
Imports SchedulerGridDragDrop
Imports DX.Xpf.DnD

Namespace SchedulerGeneralSLT
	Partial Public Class MainPage
		Inherits UserControl
		Implements IDragAndDropHandler
		Public Sub New()
			InitializeComponent()

			gridControl1.ItemsSource = DemoUtils.GenerateScheduleTasks()

			DragAndDropManager.RegisterHandler(schedulerControl1, Me)
			DragAndDropManager.RegisterHandler(gridControl1, Me)
		End Sub

		Public Sub grid_DragOver(ByVal sender As Object, ByVal e As DragAndDropEventArgs)
			If (Not ReferenceEquals(e.Target, e.Source)) Then
				e.Accept = True
			End If
		End Sub

		Public Sub scheduler_DragOver(ByVal sender As Object, ByVal e As DragAndDropEventArgs)
			If (Not ReferenceEquals(e.Target, e.Source)) Then
				e.Accept = True
			End If
		End Sub

		Public Sub grid_Drop(ByVal sender As Object, ByVal e As DragAndDropEventArgs)
			schedulerControl1.SelectedAppointments(0).Delete()
			CType(gridControl1.ItemsSource, IList).Add(CType(e.DragData, ScheduleTask))
		End Sub

		Public Sub scheduler_Drop(ByVal sender As Object, ByVal e As DragAndDropEventArgs)
			Dim hitInfo As ISchedulerHitInfo = SchedulerHitInfo.CreateSchedulerHitInfo(schedulerControl1, e.Location)
			Dim vsivi As VisualSelectableIntervalViewInfo = TryCast(hitInfo.ViewInfo, VisualSelectableIntervalViewInfo)

			If vsivi IsNot Nothing Then
				Dim dragData As IList = CType(e.DragData, IList)

				For Each item As ScheduleTask In dragData
					CType(gridControl1.ItemsSource, IList).Remove(item)
					Dim apt As Appointment = ScheduleTaskToAppointment(item)
					apt.Start = vsivi.Interval.Start
					schedulerControl1.Storage.AppointmentStorage.Add(apt)
				Next item
			End If
		End Sub

		Public Sub grid_DragLeave(ByVal sender As Object, ByVal e As DragAndDropEventArgs)

		End Sub

		Public Sub scheduler_DragLeave(ByVal sender As Object, ByVal e As DragAndDropEventArgs)

		End Sub

		Public Function CanStartDrag(ByVal source As UIElement, ByVal e As MouseButtonEventArgs, ByRef dragData As Object) As Boolean Implements IDragAndDropHandler.CanStartDrag
			Dim scheduler As SchedulerControl = TryCast(source, SchedulerControl)
			If scheduler IsNot Nothing Then
				Return SchedulerCanStartDrag(scheduler, e, dragData)
			End If
			Dim grid As GridControl = TryCast(source, GridControl)
			If grid IsNot Nothing Then
				Return GridControlCanStartDrag(grid, e, dragData)
			End If
			Return False
		End Function

		Private Function SchedulerCanStartDrag(ByVal source As SchedulerControl, ByVal e As MouseButtonEventArgs, ByRef dragData As Object) As Boolean
			Dim hitInfo As ISchedulerHitInfo = SchedulerHitInfo.CreateSchedulerHitInfo(source, e.GetPosition(source))
			Dim vavi As VisualAppointmentViewInfo = TryCast(hitInfo.ViewInfo, VisualAppointmentViewInfo)

			If vavi IsNot Nothing Then
				dragData = AppointmentToScheduleTask((CType(vavi, IAppointmentView)).Appointment)

				Return dragData IsNot Nothing
			End If

			Return False
		End Function

		Private Function GridControlCanStartDrag(ByVal source As GridControl, ByVal e As MouseButtonEventArgs, ByRef dragData As Object) As Boolean
			Dim tableView As TableView = TryCast(source.View, TableView)
			Dim hitInfo As TableViewHitInfo = tableView.CalcHitInfo(TryCast(e.OriginalSource, DependencyObject))
			If tableView IsNot Nothing AndAlso hitInfo.InRow Then
				dragData = tableView.SelectedRows.ToList()
				Return dragData IsNot Nothing
			End If
			Return False
		End Function

		Public Function CreateDragThumb(ByVal source As UIElement, ByVal target As UIElement, ByVal dragData As Object) As FrameworkElement Implements IDragAndDropHandler.CreateDragThumb
			Return Nothing
		End Function

		Private Function AppointmentToScheduleTask(ByVal apt As Appointment) As ScheduleTask
			Dim result As New ScheduleTask() With {.Subject = apt.Subject, .Severity = apt.LabelId, .Priority = apt.StatusId, .Duration = CInt(Fix(apt.Duration.TotalMinutes)), .Description = apt.Description}

			Return result
		End Function

		Private Function ScheduleTaskToAppointment(ByVal scheduleTask As ScheduleTask) As Appointment
			Dim apt As New Appointment(AppointmentType.Normal) With {.Subject = scheduleTask.Subject, .LabelId = scheduleTask.Severity, .StatusId = scheduleTask.Priority, .Duration = TimeSpan.FromMinutes(scheduleTask.Duration), .Description = scheduleTask.Description}

			Return apt
		End Function
	End Class
End Namespace