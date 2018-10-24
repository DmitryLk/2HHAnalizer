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



namespace WpfApp1
{
    public interface IModel
    {
        Task<ObservableCollection<Record>> LoadAsync(ILoader t, CancellationToken cancellationToken);
        Task SaveAsync(ISaver t, CancellationToken cancellationToken);
        ObservableCollection<q> AnalizeAsync();
        event EventHandler<MyEventArgs> Changed;
        ObservableCollection<Record> GetData();
        void OnPropertyChanged(string propName);
    }


    public class MyModel : IModel, INotifyPropertyChanged
    {
        private ObservableCollection<Record> Spisok { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }





        ObservableCollection<q> yap;

        public event EventHandler<MyEventArgs> Changed = delegate { };

        //private readonly IView View;

        private double pbprc;
        public double Pbprc
        {
            get { return pbprc; }

            set
            {
                pbprc = value;
                OnPropertyChanged("Pbprc");
            }
        }
       

        public MyModel()
        {
            //this.View = View;
            Spisok = new ObservableCollection<Record>();
            yap = new ObservableCollection<q>();
            Array.ForEach(new string[] { "1C", ".NET", "ADO.NET", "ajax", "Angular", "AngularJS", "aop", "ASP.NET", "bash", "C#", "C++", "CD", "CI", "Confluence", "CSS", "D3.js", "delphi", "design patterns", "Entity Framework", "ExtJS", "firebird", "git", "gitlab", "HTML", "html5", "Java ", "JavaScript", "jira", "jQuery", "jquery", "js", "kanban", "kiss", "Knockout", "Linq", "MongoDB", "mssql", "mvc", "mvi", "mvp", "mvvm", "mysql", "Node.js", "oracle", "orm", "Perl", "PHP", "PL/SQL", "PostgreSQL", "powershell", "Python", "React", "rest", "rubocop", "Ruby", "scrum", "slack", "soap", "solid", "Swift", "tdd", "tfs", "T-SQL", "TypeScript", "vcs", "Vue.js", "wcf", "webapi", "WebGL", "winforms", "xml", "xpath", "xquery", "xsd", "xsl", "zendesk" }, s => yap.Add(new q(s)));
            //Array.ForEach(new string[] { "Java ", "JavaScript", "C#", "C++", "Ruby", "1C", "PHP ", "ASP.NET", "PostgeSQL", "Python" }, s => yap.Add(new q(s)));

            
        }

        public async Task SaveAsync(ISaver Saver, CancellationToken token)
        {
            Saver.Changed += ChangeState;
            await Saver.Save(Spisok, token);
        }

        public async Task<ObservableCollection<Record>> LoadAsync(ILoader Loader, CancellationToken token)
        {
            Loader.Changed += ChangeState;
            await Loader.Load(Spisok, token, this);
            return Spisok;

            //var ti = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //this.spisok = (ObservableCollection<Record>)spisok;
            //PB.Maximum = 345;// ReadHHCountVac();
            //PB.Value = 0;
            //workerFunc = LoadFromXLS;
            //worker.RunWorkerAsync();
        }

        public ObservableCollection<Record> GetData()
        {
            return Spisok;
        }

        public ObservableCollection<q> AnalizeAsync()
        {
            MyEventArgs args = new MyEventArgs();
            args.MaxValue = yap.Count;
            args.Value = 0;
            foreach (q p in yap)
            {
                p.count = Spisok.Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus()));

                ++args.Value;
                if (Changed != null) Changed(this, args);

            }
            return yap;

            //UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);
            //yap.ForEach<q>(p => p.count = spisok.Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus())));
            //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
            //Pbprc = value / yap.Count() * 100;
        }


        public void ChangeState(object sender, MyEventArgs e)
        {

            if (Changed != null) Changed(sender, e);
        }

    }
}



