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
        ObservableCollection<q> AnalizeAsync(ICollectionView ICV);
        event EventHandler<MyEventArgs> Changed;
        event EventHandler<MyEventArgs> StartPause;
        event EventHandler<MyEventArgs> EndPause;

        IList<Record> GetData();
        ObservableCollection<q> GetQData();

    }



    public class MyModel : IModel
    {
        private List<Record> Spisok { get; set; }

        ObservableCollection<q> yap;

        public event EventHandler<MyEventArgs> Changed = delegate { };
        public event EventHandler<MyEventArgs> StartPause = delegate { };
        public event EventHandler<MyEventArgs> EndPause = delegate { };


        public MyModel()
        {
            //this.View = View;
            Spisok = new List<Record>();
            yap = new ObservableCollection<q>();
            // убрано xml
            Array.ForEach(new string[] { ".NET", "1C", "1С", "activemq", "ADO.NET", "aerospike", "ajax", "amazon", "Angular", "AngularJS", "aop", "apache", "artisto", "ASP.NET", "autofac", "automapper", "azure", "babel", "BackBone", "backend", "bash", "bigdata", "biztalk", "blockchain", "bootstrap", "bug", "C#", "C++", "cassandra", "CD", "centos", "ceph", "chef", "CI", "citus", "clickhouse", "cloud", "clr", "cmmi", "componentpro", "confluence", "continuous integration", "cotlin", "CSS", "custis", "D3.js", "dapper", "ddd", "ddos", "debian", "delphi", "design patterns", "devexpress", "devops", "docker", "dom", "dropbox", "ecommerce", "ef6", "elastic", "elasticsearch", "enterprise", "Entity Framework", "es5", "es6", "ExtJS", "fdb", "firebird", "fluent", "flutter", "framework", "freebsd", "frontend", "front-end", "fullstack", "gatling", "git", "github", "gitlab", "go", "golang", "google", "grafana", "grunt", "gulp", "hbase", "helm-chart", "highload", "hightload", "HTML", "html5", "iaas", "iis", "influx", "integration", "ioc", "iot", "Java ", "JavaScript", "jenkins", "jira", "jQuery", "js", "json", "kafka", "kanban", "karma", "kendo", "kernel", "kibana", "kiss", "knockout", "kubernetes", "less", "Linq", "linux", "lmdb", "loggly", "logly", "logstash", "lua", "marathon", "mariadb", "marionettejs", "microservice", "mocha", "MongoDB", "monolith", "moxy", "mssql", "mvc", "mvi", "mvp", "mvvm", "mysql", "ncrunch", "newrelic", "ngenix", "nginx", "nhibernate", "ninject", "Node.js", "nodejs", "noisesocket", "nosql", "npm", "nunit", "openbsd", "open-source", "oracle", "orm", "paas", "Perl", "PHP", "pipeline", "PL/SQL", "postgres", "PostgreSQL", "powershell", "prometeus", "puppet", "Python", "rabbit", "rabbitmq", "React", "redis", "redux", "rest", "restrful", "rps", "rubocop", "Ruby", "saas", "sass", "scala", "scrum", "sdlc", "sharepoint", "slack", "soap", "solaris", "solid", "spark", "sphinx", "sqlite", "stylus", "svn", "swift", "tarantool", "tdd", "teamcity", "tfs", "timerjob", "tomcat", "trello", "T-SQL", "typescript", "ubuntu", "unity", "unity3d", "unix", "vagrant", "vcs", "vfx", "Vue.js", "wcf", "webapi", "WebGL", "webpack", "windows", "winforms", "wpf", "xml", "xpath", "xquery", "xsd", "xsl", "xunit", "yarn", "youtrack", "zabbix", "zendesk", "С#", "С++", "тест" }, 
                s => yap.Add(new q(s)));
            //Array.ForEach(new string[] { "Java ", "JavaScript", "C#", "C++", "Ruby", "1C", "PHP ", "ASP.NET", "PostgeSQL", "Python" }, s => yap.Add(new q(s)));
            //"ADO.NET", ".NET", "1C", "activemq", "aerospike", "ajax", "amazon", "Angular", "AngularJS", "aop", "ASP.NET", "autofac", "automapper", "azure", "babel", "BackBone", "backend", "bash", "bug", "C#", "C++", "cassandra", "CD", "centos", "CI", "clickhouse", "cloud", "clr", "cmmi", "componentpro", "Confluence", "CSS", "D3.js", "dapper", "ddd", "debian", "delphi", "design patterns", "devexpress", "ef6", "elastic", "elasticsearch", "Entity Framework", "es5", "es6", "ExtJS", "fdb", "firebird", "fluent", "framework", "freebcd", "frontend", "front-end", "fullstack", "gatling", "git", "github", "gitlab", "go", "google", "grafana", "grunt", "gulp", "hbase", "hightload", "HTML", "html5", "iis", "influx", "ioc", "Java", "JavaScript", "jenkins", "jira", "jQuery", "js", "kanban", "karma", "kendo", "kibana", "kiss", "knockout", "less", "Linq", "linux", "lmdb", "loggly", "logly", "logstash", "marionettejs", "mocha", "MongoDB", "mssql", "mvc", "mvi", "mvp", "mvvm", "mysql", "ncrunch", "newrelic", "nginx", "nhibernate", "ninject", "Node.js", "npm", "nunit", "oracle", "orm", "Perl", "PHP", "PL/SQL", "PostgreSQL", "powershell", "Python", "rabbitmq", "React", "redis", "redux", "rest", "rubocop", "Ruby", "sass", "scala", "scrum", "slack", "soap", "solid", "sqlite", "stylus", "svn", "Swift", "tdd", "tfs", "timerjob", "trello", "T-SQL", "typescript", "ubuntu", "unity", "unity3d", "unix", "vcs", "vfx", "Vue.js", "wcf", "webapi", "WebGL", "webpack", "windows", "winforms", "wpf", "xml", "xpath", "xquery", "xsd", "xsl", "xunit", "yarn", "youtrack", "zabbix", "zendesk", "тест"

            //".NET", "1C", "activemq", "ADO.NET", "aerospike", "ajax", "amazon", "Angular", "AngularJS", "aop", "apache", "artisto", "ASP.NET", "autofac", "automapper", "azure", "babel", "BackBone", "backend", "bash", "bigdata", "biztalk", "blockchain", "bootstrap", "bug", "C#", "C++", "cassandra", "CD", "centos", "ceph", "chef", "CI", "citus", "clickhouse", "cloud", "clr", "cmmi", "componentpro", "confluence", "continuous integration", "cotlin", "CSS", "custis", "D3.js", "dapper", "ddd", "ddos", "debian", "delphi", "design patterns", "devexpress", "devops", "docker", "dom", "dropbox", "ecommerce", "ef6", "elastic", "elasticsearch", "enterprise", "Entity Framework", "es5", "es6", "ExtJS", "fdb", "firebird", "fluent", "flutter", "framework", "freebsd", "frontend", "front-end", "fullstack", "gatling", "git", "github", "gitlab", "go", "golang", "google", "grafana", "grunt", "gulp", "hbase", "helm-chart", "highload", "hightload", "HTML", "html5", "iaas", "iis", "influx", "integration", "ioc", "iot", "Java", "JavaScript", "jenkins", "jira", "jQuery", "js", "json", "kafka", "kanban", "karma", "kendo", "kernel", "kibana", "kiss", "knockout", "kubernetes", "less", "Linq", "linux", "lmdb", "loggly", "logly", "logstash", "lua", "marathon", "mariadb", "marionettejs", "microservice", "mocha", "MongoDB", "monolith", "moxy", "mssql", "mvc", "mvi", "mvp", "mvvm", "mysql", "ncrunch", "newrelic", "ngenix", "nginx", "nhibernate", "ninject", "Node.js", "nodejs", "noisesocket", "nosql", "npm", "nunit", "openbsd", "open-source", "oracle", "orm", "paas", "Perl", "PHP", "pipeline", "PL/SQL", "postgres", "PostgreSQL", "powershell", "prometeus", "puppet", "Python", "rabbit", "rabbitmq", "React", "redis", "redux", "rest", "restrful", "rps", "rubocop", "Ruby", "saas", "sass", "scala", "scrum", "sdlc", "sharepoint", "slack", "soap", "solaris", "solid", "spark", "sphinx", "sqlite", "stylus", "svn", "swift", "tarantool", "tdd", "teamcity", "tfs", "timerjob", "tomcat", "trello", "T-SQL", "typescript", "ubuntu", "unity", "unity3d", "unix", "vagrant", "vcs", "vfx", "Vue.js", "wcf", "webapi", "WebGL", "webpack", "windows", "winforms", "wpf", "xml", "xpath", "xquery", "xsd", "xsl", "xunit", "yarn", "youtrack", "zabbix", "zendesk", "тест"

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
        public ObservableCollection<q> GetQData() => yap;


        public ObservableCollection<q> AnalizeAsync(ICollectionView ICV)
        {
            MyEventArgs args = new MyEventArgs();
            args.MaxValue = yap.Count;
            args.Value = 0;
            foreach (q p in yap)
            {
                
                p.count = ICV.Cast<Record>().Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus()));
                p.prc = 100* (p.count) / ((double)ICV.Cast<Record>().Count());

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
