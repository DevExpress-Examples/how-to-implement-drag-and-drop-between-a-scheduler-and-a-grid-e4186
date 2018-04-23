using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Scheduler;
using DevExpress.Xpf.Scheduler.Drawing;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.Drawing;
using DevExpress.XtraScheduler.Native;
using SchedulerGridDragDrop;
using DX.Xpf.DnD;

namespace SchedulerGeneralSLT {
    public partial class MainPage : UserControl, IDragAndDropHandler {
        public MainPage() {
            InitializeComponent();

            gridControl1.ItemsSource = DemoUtils.GenerateScheduleTasks();

            DragAndDropManager.RegisterHandler(schedulerControl1, this);
            DragAndDropManager.RegisterHandler(gridControl1, this);
        }

        public void grid_DragOver(object sender, DragAndDropEventArgs e) {
            if (!ReferenceEquals(e.Target, e.Source))
                e.Accept = true;
        }

        public void scheduler_DragOver(object sender, DragAndDropEventArgs e) {
            if (!ReferenceEquals(e.Target, e.Source))
                e.Accept = true;
        }

        public void grid_Drop(object sender, DragAndDropEventArgs e) {
            schedulerControl1.SelectedAppointments[0].Delete();
            ((IList)gridControl1.ItemsSource).Add((ScheduleTask)e.DragData);
        }

        public void scheduler_Drop(object sender, DragAndDropEventArgs e) {
            ISchedulerHitInfo hitInfo = SchedulerHitInfo.CreateSchedulerHitInfo(schedulerControl1, e.Location);
            VisualSelectableIntervalViewInfo vsivi = hitInfo.ViewInfo as VisualSelectableIntervalViewInfo;

            if (vsivi != null) {
                IList dragData = (IList)e.DragData;

                for(int i = 0; i < dragData.Count; i++) {
                    ((IList)gridControl1.ItemsSource).Remove(dragData[i]);
                    Appointment apt = ScheduleTaskToAppointment(dragData[i] as ScheduleTask);
                    apt.Start = vsivi.Interval.Start;
                    schedulerControl1.Storage.AppointmentStorage.Add(apt);
                }
            }
        }

        public void grid_DragLeave(object sender, DragAndDropEventArgs e) {

        }

        public void scheduler_DragLeave(object sender, DragAndDropEventArgs e) {
            
        }

        public bool CanStartDrag(UIElement source, MouseButtonEventArgs e, ref object dragData) {
            SchedulerControl scheduler = source as SchedulerControl;
            if (scheduler != null)
                return SchedulerCanStartDrag(scheduler, e, ref dragData);
            GridControl grid = source as GridControl;
            if (grid != null)
                return GridControlCanStartDrag(grid, e, ref dragData);
            return false;
        }

        private bool SchedulerCanStartDrag(SchedulerControl source, MouseButtonEventArgs e, ref object dragData) {
            ISchedulerHitInfo hitInfo = SchedulerHitInfo.CreateSchedulerHitInfo(source, e.GetPosition(source));
            VisualAppointmentViewInfo vavi = hitInfo.ViewInfo as VisualAppointmentViewInfo;

            if (vavi != null) {
                dragData = AppointmentToScheduleTask(((IAppointmentView)vavi).Appointment);

                return dragData != null;
            }

            return false;
        }

        private bool GridControlCanStartDrag(GridControl source, MouseButtonEventArgs e, ref object dragData) {
            TableView tableView = source.View as TableView;
            TableViewHitInfo hitInfo = tableView.CalcHitInfo(e.OriginalSource as DependencyObject);
            if (tableView != null && hitInfo.InRow) {
                dragData = gridControl1.SelectedItems;
                return dragData != null;
            }
            return false;
        }

        public FrameworkElement CreateDragThumb(UIElement source, UIElement target, object dragData) {
            return null;
        }

        private ScheduleTask AppointmentToScheduleTask(Appointment apt) {
            ScheduleTask result = new ScheduleTask() {
                Subject = apt.Subject,
                Severity = apt.LabelId,
                Priority = apt.StatusId,
                Duration = (int)apt.Duration.TotalMinutes,
                Description = apt.Description
            };

            return result;
        }

        private Appointment ScheduleTaskToAppointment(ScheduleTask scheduleTask) {
            Appointment apt = new Appointment(AppointmentType.Normal) {
                Subject = scheduleTask.Subject,
                LabelId = scheduleTask.Severity,
                StatusId = scheduleTask.Priority,
                Duration = TimeSpan.FromMinutes(scheduleTask.Duration),
                Description = scheduleTask.Description
            };

            return apt;
        }
    }
}