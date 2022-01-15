using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace WpfApp1
{
    public static class ExtensionMethods

    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)

        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public static class WebBrowserExtensions
    {
        public static Task<object> NavigateAsync(this WebBrowser wb, string link, SynchronizationContext SC)
        {
            if (wb == null) throw new ArgumentNullException("wb");
            var tcs = new TaskCompletionSource<object>();
            LoadCompletedEventHandler handler1 = delegate { };
            NavigatedEventHandler handler2 = delegate { };

            handler1 = (s, e) =>
            {
                dynamic activeX = wb.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, wb, new object[] { });
                activeX.Silent = true;

                //if (wb.ReadyState != WebBrowserReadyState.Complete) return;
                //if (PageLoaded.Task.IsCompleted) return;
                tcs.SetResult(wb.Document);
                wb.LoadCompleted -= handler1;
                //wb.Navigated -= handler2;
            };

            handler2 = (s, e) =>
            {
                dynamic doc = ((WebBrowser)s).Document;
                var url = doc.url as string;
                if (url != null && url.StartsWith("res://ieframe.dll"))
                {
                    tcs.SetException(new InvalidOperationException("Page load error"));
                }
                //wb.LoadCompleted -= handler1;
                wb.Navigated -= handler2;
            };

            wb.LoadCompleted += handler1;
            wb.Navigated += handler2;
            SC.Post(new SendOrPostCallback(o => { wb.Navigate(link); }), link);
            return tcs.Task;

            //await Application.Current.Dispatcher.BeginInvoke(new Action(() => { wb.Navigate(link); }));
            //wb.Navigate(new Uri(link));
            /*
            int TimeElapsed = 0;
                        while (tcs.Task.Status != TaskStatus.RanToCompletion)
                        {
                            await Task.Delay(50);
                            TimeElapsed++;
                            if (TimeElapsed >= 1000)
                            {
                               tcs.TrySetResult(true);
                            }
                        }
                */
            //надо ли?
            //while (tcs.Task.Status != TaskStatus.RanToCompletion)
            //{
            //    await Task.Delay(50);
            //}
            //await tcs.Task;
        }
    }

    public static class ExtensionsIEnumerable
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }

    public static class ExtensionsString
    {
        public static bool ContainsCI(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }

    public class MyEventArgs : EventArgs
    {
        public int Value { get; set; }
        public int Value2 { get; set; }

        public int MaxValue { get; set; }
        public FlowDocument flowDocument { get; set; }
        public Record Rec { get; set; }

        //public List<Record> Spisok { get; set; }

        //public String Message { get; set; }
    }

    public class Record
    {
        public string MyId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Zp { get; set; }
        public int? Zp2 { get; set; }
        public string Comp { get; set; }
        public string Town { get; set; }
        public string Resp1 { get; set; }
        public string Req1 { get; set; }
        public DateTime Dat { get; set; }
        public string Opt { get; set; }
        public string Desc { get; set; }
        public string link { get; set; }
        public bool Sharp { get; set; }
        public bool JavaScript { get; set; }
        public bool SQL { get; set; }
        public bool _1C { get; set; }
        public bool Distant { get; set; }
        public bool Closed { get; set; }
        public bool NewUpdates { get; set; }
        public DateTime BeginingDate { get; set; }
        public DateTime LastCheckDate { get; set; }
        public double DaysLong { get; set; }

        public bool? Interes { get; set; }
        public string Rating { get; set; }

        public int LanguageIndex { get; set; }

        public string AllInfo() => Name + Zp + Comp + Town + Resp1 + Req1 + Dat + Opt + Desc;
    }

    public class AnaliseType
    {
        public string Name { get; set; }
        public int count { get; set; }
        public double prc { get; set; }

        public AnaliseType(string s)
        { Name = s; count = 0; }

        public string NameRus() => Name.Replace("C", "С");
    }
}