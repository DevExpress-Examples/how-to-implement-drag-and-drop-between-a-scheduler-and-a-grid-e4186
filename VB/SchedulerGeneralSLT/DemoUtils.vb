Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Namespace SchedulerGridDragDrop
	Public Class DemoUtils
		Public Shared RandomInstance As New Random()

		Private Shared taskDescriptions() As String = { "Implementing Developer Express MasterView control into Accounting System.", "Web Edition: Data Entry Page. The issue with date validation.", "Payables Due Calculator. It is ready for testing.", "Web Edition: Search Page. It is ready for testing.", "Main Menu: Duplicate Items. Somebody has to review all menu items in the system.", "Receivables Calculator. Where can I found the complete specs", "Ledger: Inconsistency. Please fix it.", "Receivables Printing. It is ready for testing.", "Screen Redraw. Somebody has to look at it.", "Email System. What library we are going to use?", "Adding New Vendors Fails. This module doesn't work completely!", "History. Will we track the sales history in our system?", "Main Menu: Add a File menu. File menu is missed!!!", "Currency Mask. The current currency mask in completely inconvinience.", "Drag & Drop. In the schedule module drag & drop is not available.", "Data Import. What competitors databases will we support?", "Reports. The list of incomplete reports.", "Data Archiving. This features is still missed in our application", "Email Attachments. How to add the multiple attachment? I did not find a way to do it.", "Check Register. We are using different paths for different modules.", "Data Export. Our customers asked for export into Excel"}

		Public Shared Function GenerateScheduleTasks() As ObservableCollection(Of ScheduleTask)
			Dim table As New ObservableCollection(Of ScheduleTask)()

			For i As Integer = 0 To 20
				Dim description As String = taskDescriptions(i)
				Dim index As Integer = description.IndexOf("."c)
				Dim subject As String
				If index <= 0 Then
					subject = "task" & Convert.ToInt32(i + 1)
				Else
					subject = description.Substring(0, index)
				End If
				table.Add(New ScheduleTask() With {.Id = i + 1, .Subject = subject, .Severity = RandomInstance.Next(3), .Priority = RandomInstance.Next(3), .Duration = Math.Max(1, RandomInstance.Next(8)) * 60, .Description = description})
			Next i
			Return table
		End Function
	End Class

	Public Class ScheduleTask
		Private id_Renamed As Integer

		Public Property Id() As Integer
			Get
				Return id_Renamed
			End Get
			Set(ByVal value As Integer)
				id_Renamed = value
			End Set
		End Property
		Private subject_Renamed As String

		Public Property Subject() As String
			Get
				Return subject_Renamed
			End Get
			Set(ByVal value As String)
				subject_Renamed = value
			End Set
		End Property
		Private description_Renamed As String

		Public Property Description() As String
			Get
				Return description_Renamed
			End Get
			Set(ByVal value As String)
				description_Renamed = value
			End Set
		End Property
		Private severity_Renamed As Integer

		Public Property Severity() As Integer
			Get
				Return severity_Renamed
			End Get
			Set(ByVal value As Integer)
				severity_Renamed = value
			End Set
		End Property
		Private priority_Renamed As Integer

		Public Property Priority() As Integer
			Get
				Return priority_Renamed
			End Get
			Set(ByVal value As Integer)
				priority_Renamed = value
			End Set
		End Property
		Private duration_Renamed As Integer

		Public Property Duration() As Integer
			Get
				Return duration_Renamed
			End Get
			Set(ByVal value As Integer)
				duration_Renamed = value
			End Set
		End Property
	End Class
End Namespace