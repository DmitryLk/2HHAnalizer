using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.ComponentModel;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Win32;
using System.Threading;


namespace WpfApp1
{

    delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

    public interface IPresentier
    {
        Task LoadFromXLS(SynchronizationContext SC);
        Task SaveToXLS();
        Task LoadFromWeb(string webs, WebBrowser wb, SynchronizationContext SC);
        Task AnalizeAsync();
        void Cancel();

        //event EventHandler<MyEventArgs> SpisokReady;

    }

    public class MyPresentier : IPresentier
    {
        private readonly IView View;
        private readonly IModel Model;
        private CancellationTokenSource _tokensource;
        CancellationToken token;
        //public event EventHandler<MyEventArgs> SpisokReady = delegate { };

        public MyPresentier(IView View, IModel Model)
        {
            this.View = View;
            this.Model = Model;
            Model.Changed += ProgressBar_Change;

            

        }

        public async Task SaveToXLS()
        {
            await Task.Delay(10);
            try
            {
                _tokensource = new CancellationTokenSource();
                token = _tokensource.Token;
                await Task.Run(() => Model.SaveAsync(new XLSSaver(), token), token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        public async Task LoadFromWeb(string webs, WebBrowser wb, SynchronizationContext SC)
        {
            await LoadFromAsync(new WebLoader(webs, wb, SC));
        }

        public async Task LoadFromXLS(SynchronizationContext SC)
        {
            await LoadFromAsync(new XLSLoader(SC));
        }

     

        public async Task LoadFromAsync(ILoader loader)
        {
            
            ObservableCollection<Record> Spisok = null;
            //Task<ObservableCollection<Record>> task = Task<ObservableCollection<Record>>.Run(() => Model.LoadAsync(loader));
            try
            {
                _tokensource = new CancellationTokenSource();
                token = _tokensource.Token;
                Spisok = await Task.Run(() => Model.LoadAsync(loader, token), token);
            }
            catch (OperationCanceledException)
            {
                Spisok = Model.GetData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            View.SpisokView(Spisok);
            
            
        }

        public void Cancel()
        {
            _tokensource.Cancel();
        }

        public async Task AnalizeAsync()
        {
            ObservableCollection<q> yap;
            yap = await Task.Run(() => Model.AnalizeAsync());
            View.YapView(yap);
        }

        public void ProgressBar_Change(object sender, MyEventArgs e)
        {
            
            View.PB_Update(e);
            //System.Threading.Thread.Sleep(10);
           
        }

           
        //private ILoader loader;

     
        //public string SourceIp { get; set; }
        //public Listen(string sourceIp)
        //{
        //    SourceIp = sourceIp;
        //}


    }
}
