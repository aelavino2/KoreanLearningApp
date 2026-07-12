using System;
using System.Collections.Generic;
using System.Text;

namespace KoreanLearningApp.Events
{
    public class AlertRequestEventArgs : EventArgs
    {
        public string Title { get; }
        public string Message { get; }
        public string Cancel { get; }

        public AlertRequestEventArgs(string title, string message, string cancel)
        {
            Title = title;
            Message = message;
            Cancel = cancel;
        }
    }
}
