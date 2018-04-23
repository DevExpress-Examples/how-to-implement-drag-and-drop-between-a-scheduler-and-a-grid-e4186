using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SchedulerGridDragDrop {
    public class DemoUtils {
        public static Random RandomInstance = new Random();

        static string[] taskDescriptions = new string[] {
                                                   "Implementing Developer Express MasterView control into Accounting System.",
                                                   "Web Edition: Data Entry Page. The issue with date validation.",
                                                   "Payables Due Calculator. It is ready for testing.",
                                                   "Web Edition: Search Page. It is ready for testing.",
                                                   "Main Menu: Duplicate Items. Somebody has to review all menu items in the system.",
                                                   "Receivables Calculator. Where can I found the complete specs",
                                                   "Ledger: Inconsistency. Please fix it.",
                                                   "Receivables Printing. It is ready for testing.",
                                                   "Screen Redraw. Somebody has to look at it.",
                                                   "Email System. What library we are going to use?",
                                                   "Adding New Vendors Fails. This module doesn't work completely!",
                                                   "History. Will we track the sales history in our system?",
                                                   "Main Menu: Add a File menu. File menu is missed!!!",
                                                   "Currency Mask. The current currency mask in completely inconvinience.",
                                                   "Drag & Drop. In the schedule module drag & drop is not available.",
                                                   "Data Import. What competitors databases will we support?",
                                                   "Reports. The list of incomplete reports.",
                                                   "Data Archiving. This features is still missed in our application",
                                                   "Email Attachments. How to add the multiple attachment? I did not find a way to do it.",
                                                   "Check Register. We are using different paths for different modules.",
                                                   "Data Export. Our customers asked for export into Excel"};

        public static ObservableCollection<ScheduleTask> GenerateScheduleTasks() {
            ObservableCollection<ScheduleTask> table = new ObservableCollection<ScheduleTask>();

            for (int i = 0; i < 21; i++) {
                string description = taskDescriptions[i];
                int index = description.IndexOf('.');
                string subject;
                if (index <= 0)
                    subject = "task" + Convert.ToInt32(i + 1);
                else
                    subject = description.Substring(0, index);
                table.Add(new ScheduleTask() {
                    Id = i + 1,
                    Subject = subject,
                    Severity = RandomInstance.Next(3),
                    Priority = RandomInstance.Next(3),
                    Duration = Math.Max(1, RandomInstance.Next(8)) * 60,
                    Description = description
                });
            }
            return table;
        }
    }

    public class ScheduleTask {
        private int id;

        public int Id {
            get { return id; }
            set { id = value; }
        }
        private string subject;

        public string Subject {
            get { return subject; }
            set { subject = value; }
        }
        private string description;

        public string Description {
            get { return description; }
            set { description = value; }
        }
        private int severity;

        public int Severity {
            get { return severity; }
            set { severity = value; }
        }
        private int priority;

        public int Priority {
            get { return priority; }
            set { priority = value; }
        }
        private int duration;

        public int Duration {
            get { return duration; }
            set { duration = value; }
        }
    }
}