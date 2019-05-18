using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;
using System.Windows;


namespace WpfApp1
{
    public interface IModel
    {
        Task LoadAsync(ILoader t, CancellationToken cancellationToken);
        Task SaveAsync(ISaver t, CancellationToken cancellationToken);
        ObservableCollection<AnaliseType> AnalizeAsync(ICollectionView ICV, bool onlyFromNames);
        event EventHandler<MyEventArgs> Changed;
        event EventHandler<MyEventArgs> StartPause;
        event EventHandler<MyEventArgs> EndPause;

        IList<Record> GetData();
        ObservableCollection<AnaliseType> GetQData();

    }



    public class MyModel : IModel
    {
        private List<Record> Spisok { get; set; }

        ObservableCollection<AnaliseType> yap;

        public event EventHandler<MyEventArgs> Changed = delegate { };
        public event EventHandler<MyEventArgs> StartPause = delegate { };
        public event EventHandler<MyEventArgs> EndPause = delegate { };


        public MyModel()
        {
            //this.View = View;
            Spisok = new List<Record>();
            yap = new ObservableCollection<AnaliseType>();
            // убрано xml

            var searchText = File.ReadAllLines("searchtext.txt", Encoding.GetEncoding(1251));

            Array.ForEach(searchText, s => yap.Add(new AnaliseType(s.Trim('\''))));
        }

        public async Task SaveAsync(ISaver Saver, CancellationToken token)
        {
            Saver.Changed += ChangeState;
            await Saver.Save(Spisok, token);
        }

        public async Task LoadAsync(ILoader Loader, CancellationToken token)
        {
            Loader.Changed += ChangeState;
            Loader.StartPause += StartPauseEventFunc;
            Loader.EndPause += EndPauseEventFunc;



            try
            {
                await Loader.Load(Spisok, token, this);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Interrupted by user");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }



            //return Spisok;

            //var ti = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //this.spisok = (List<Record>)spisok;
            //PB.Maximum = 345;// ReadHHCountVac();
            //PB.Value = 0;
            //workerFunc = LoadFromXLS;
            //worker.RunWorkerAsync();
        }

        public IList<Record> GetData() => Spisok;
        public ObservableCollection<AnaliseType> GetQData() => yap;


        public ObservableCollection<AnaliseType> AnalizeAsync(ICollectionView ICV, bool onlyFromNames)
        {
            MyEventArgs args = new MyEventArgs();
            args.MaxValue = yap.Count;
            args.Value = 0;
            foreach (AnaliseType p in yap)
            {
                if (onlyFromNames)
                    p.count = ICV.Cast<Record>().Count(t => t.Name.ContainsCI(p.Name) || t.Name.ContainsCI(p.NameRus()));
                else
                    p.count = ICV.Cast<Record>().Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus()));

                p.prc = 100* (p.count) / ((double)ICV.Cast<Record>().Count());

                ++args.Value;
                if (Changed != null) Changed(this, args);

            }
            return yap;

            //UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);
            //yap.ForEach<AnaliseType>(p => p.count = spisok.Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus())));
            //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
            //Pbprc = value / yap.Count() * 100;
        }


        public void ChangeState(object sender, MyEventArgs e)
        {

            if (Changed != null) Changed(sender, e);
        }

        public void StartPauseEventFunc(object sender, MyEventArgs e)
        {

            if (StartPause != null) StartPause(sender, e);
        }

        public void EndPauseEventFunc(object sender, MyEventArgs e)
        {

            if (EndPause != null) EndPause(sender, e);
        }

    }
}


/*
 *       public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
 * 
 *   private double pbprc;
        public double Pbprc
        {
            get { return pbprc; }

            set
            {
                pbprc = value;
                OnPropertyChanged("Pbprc");
            }
        }
 * 
 * 
 * 
 */
