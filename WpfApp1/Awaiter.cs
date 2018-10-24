using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Runtime.CompilerServices;
using System.Threading;


namespace WpfApp1
{

    public class MyWebBrowserAwaitable
    {
        private volatile bool completed;
        private volatile object result;
        private Action continuation;

        public bool IsCompleted => completed;
        public object Result => RunToCompletionAndGetResult();
        //public HtmlDocument Result => result;

        public MyWebBrowserAwaitable(WebBrowser wb) //происходит до Navigate
        {
            completed = false;
            wb.LoadCompleted += new LoadCompletedEventHandler(this.WebBrowser_LoadCompleted);
            //    if (wb уже загрузился -невозможно)
            //{
            //        completed = true;
            //        result = wb.Document;
            //    }
        }

        public void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (!completed)
            {
                completed = true;
                WebBrowser wb = sender as WebBrowser;
                result = wb.Document;
                continuation?.Invoke();
            }
        }

        public WebBrowserAwaiter GetAwaiter() => new WebBrowserAwaiter(this);

        internal void ScheduleContinuation(Action action) => continuation += action;

        internal object RunToCompletionAndGetResult()
        {
            var wait = new SpinWait();
            while (!completed)
                wait.SpinOnce();
            return result;
        }


    }


    public class WebBrowserAwaiter : INotifyCompletion
    {
        private readonly MyWebBrowserAwaitable awaitable;

        public WebBrowserAwaiter(MyWebBrowserAwaitable awaitable) => this.awaitable = awaitable;



        public bool IsCompleted => awaitable.IsCompleted;

        public object GetResult() => awaitable.RunToCompletionAndGetResult();

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation();
                return;
            }

            var capturedContext = SynchronizationContext.Current;

            awaitable.ScheduleContinuation(() =>
            {
                if (capturedContext != null)
                    capturedContext.Post(_ => continuation(), null);
                else
                    continuation();
            });
        }

    }

}
