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




namespace WpfApp1
{


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

    interface IMyWindow
    {
        string WebString { get; set; }
        double PBmax { get;  set; }
        WebBrowser wb { get; }
    }

    public partial class MainWindow : Window, IMyWindow
    {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        MyController Ctrl;
        ILoader xlsloader;


        public string WebString
        {
            get { return TextBox1.Text; }
            set { TextBox1.Text = value; }
        }

        public double PBmax
        {
            get { return PB.Maximum; }
            set { PB.Maximum = value; PB.Value = 0; }
        }

        public WebBrowser wb
        {
            get { return WebBrowser1; }
            //set { TextBox1.Text = value; }
        }

        //====================================================
        public MainWindow() 
        {
            InitializeComponent();
            Ctrl = new MyController(this);
            xlsloader = new FromXLSLoader(this);
        }

        BackgroundWorker worker;
        public void Main()
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            //WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;

            PBtext.DataContext = this;

            yap = new ObservableCollection<q>();

            Array.ForEach(new string[] { "1C", ".NET", "ADO.NET", "ajax", "Angular", "AngularJS", "aop", "ASP.NET", "bash", "C#", "C++", "CD", "CI", "Confluence", "CSS", "D3.js", "delphi", "design patterns", "Entity Framework", "ExtJS", "firebird", "git", "gitlab", "HTML", "html5", "Java ", "JavaScript", "jira", "jQuery", "jquery", "js", "kanban", "kiss", "Knockout", "Linq", "MongoDB", "mssql", "mvc", "mvi", "mvp", "mvvm", "mysql", "Node.js", "oracle", "orm", "Perl", "PHP", "PL/SQL", "PostgreSQL", "powershell", "Python", "React", "rest", "rubocop", "Ruby", "scrum", "slack", "soap", "solid", "Swift", "tdd", "tfs", "T-SQL", "TypeScript", "vcs", "Vue.js", "wcf", "webapi", "WebGL", "winforms", "xml", "xpath", "xquery", "xsd", "xsl", "zendesk" }, s => yap.Add(new q(s)));
            //Array.ForEach(new string[] { "Java ", "JavaScript", "C#", "C++", "Ruby", "1C", "PHP ", "ASP.NET", "PostgeSQL", "Python" }, s => yap.Add(new q(s)));


            Binding binding = new Binding();
            listbox1.SetBinding(ListBox.ItemsSourceProperty, binding);
            listbox1.DataContext = yap;

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TextBox2.Text = answer;
            MyGrid.ItemsSource = spisok;
        }


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            workerFunc();
            //ReadHH();
            //SaveToXLS();
        }



        private void Button3_Click(object sender, RoutedEventArgs e)  //CheckHH
        {
            Ctrl.Navigate();
        }

        private void Button5_Click(object sender, RoutedEventArgs e)  //LoadFromXLS
        {
            Ctrl.Load(xlsloader);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)  // Analize
        {
            PB.Maximum = yap.Count();
            PB.Value = 0;
            workerFunc = Analize;
            worker.RunWorkerAsync();
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
        }
        //====================================================




        private void LBColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                listbox1.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);

            listbox1.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));

            //ICollectionView dataView = CollectionViewSource.GetDefaultView(listbox1.ItemsSource);

            //dataView.SortDescriptions.Clear();
            //SortDescription sd = new SortDescription(sortBy, newDir);
            //dataView.SortDescriptions.Add(sd);
            //dataView.Refresh();

        }

    }

    public class SortAdorner : Adorner
    {
        private static Geometry ascGeometry = Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry = Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir)
            : base(element)
        {
            this.Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                (
                    AdornedElement.RenderSize.Width - 15,
                    (AdornedElement.RenderSize.Height - 5) / 2
                );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            drawingContext.Pop();
        }
    }




}


/*
 * 
 *   //class MyWebClient : WebClient
    //{
    //    protected override WebRequest GetWebRequest(Uri address)
    //    {
    //        HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
    //        request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
    //        return request;
    //    }
    //}
            //web = new HtmlWeb();
            //document = web.Load(webs);
            //text = document.SelectSingleNode("//div[@data-qa='vacancies-total-found']")?.InnerText ?? "";
            //    HtmlAgilityPack.HtmlDocument hDocument = new HtmlAgilityPack.HtmlDocument();
            //    hDocument.LoadHtml(WebBrowser.Document.GetElementsByTagName("HTML")[0].OuterHtml);
            ////    //this.Price = Convert.ToDouble(hDocument.DocumentNode.SelectNodes("//td[@class='ask']").FirstOrDefault().InnerText.Trim());
            //    //_WebBrowser.FindForm().Close();
            //    //_lock.Set();
            //string HTML2 = WebBrowser.InvokeScript(@"document.getElementsByTagName ('html')[0].innerHTML").ToString();
            //mshtml.IHTMLDocument2 doc = (mshtml.IHTMLDocument2)WebBrowser.Document.DomDocument;
            //    foreach (IHTMLElement element in doc.all)
            //    {
            //        System.Diagnostics.Debug.WriteLine(element.outerHTML);
            //    }
            //        Dim eCollections As HtmlElementCollection
            //Dim strDoc As String
            //eCollections = WB.Document.GetElementsByTagName("HTML")
            //strDoc = eCollections(0).OuterHtml
            //        docHtml = browser.DocumentText;
            //        var doc = ((Form1)Application.OpenForms[0]).webBrowser1.Document;
            //        doc.GetElementById("myDataTable");
            //        var renderedHtml = doc.GetElementsByTagName("HTML")[0].OuterHtml;
            //        webBrowser1.Document.GetElementsByTagName("HTML")[0].OuterHtml;
            //wb.DocumentCompleted += delegate (object sender, WebBrowserDocumentCompletedEventArgs e)
            //{
            //    mshtml.IHTMLDocument2 doc = (mshtml.IHTMLDocument2)wb.Document.DomDocument;
            //    foreach (IHTMLElement element in doc.all)
            //    {
            //        System.Diagnostics.Debug.WriteLine(element.outerHTML);
            //    }
            //};



                //Array.ForEach(yap,  s => listbox1.Items.Add(s));
            //foreach (string s in yap)
            //{
            //    listbox1.Items.Add(s);
            //}
            //string[] list = new string[] { "1", "2", "3" };
            //listbox1.ItemsSource = yap;
            //(listbox1.ItemsSource as ObservableCollection<q>).RemoveAt(0);
            //binding.ElementName = "myTextBox"; // элемент-источник
            //binding.Path = new PropertyPath("Text"); // свойство элемента-источника
            //myTextBlock.SetBinding(TextBlock.TextProperty, binding); // установка привязки для элемента-приемника
            //ObservableCollection<string> oList;
            //oList = new System.Collections.ObjectModel.ObservableCollection<string>(list);
            //listBox1.DataContext = oList;
            //Binding binding = new Binding();
            //listBox1.SetBinding(ListBox.ItemsSourceProperty, binding);
            //(listBox1.ItemsSource as ObservableCollection<string>).RemoveAt(0);
            //yap[2].count = spisok.Count(t => t.Name.Contains("Java"));
            //ObservableCollection<q> yap = new ObservableCollection<q>();
            //int size = numbers.Count(i => i % 2 == 0 && i > 10);
            //yap.ForEach<q>(p => p.count2 = p.Name);
            //++.ToList().ForEach(p => p.count = spisok.Count(t => t.Name.Contains(p.Name)));
            //yap.ToList().Sort();
            //List<int> yap2 = new List<int>();

                //using (var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            //{
            //    using (var client = new HttpClient(handler))
            //    {

                //worker.RunWorkerCompleted += new DoWorkEventHandler(worker_RunWorkerCompleted);
            //worker.WorkerReportsProgress = true;
            //worker.ProgressChanged += worker_ProgressChanged;

                //yap.ForEach<q>(p => p.count = spisok.Count(t => t.AllInfo().ContainsCI(p.Name)));
            //foreach (q p in yap)
            //{
            //    p.count = spisok.Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus()));
            //}
            //rec.JavaScript = rec.AllInfo().ContainsCI("JavaScript");
            //int sf = 0;
            //string h;
            //foreach (Record rec in spisok)
            //{
            //    h = rec.AllInfo();
            //    if (h.ContainsCI("C#") || h.ContainsCI("С#")) sf++;
            //    else
            //        if (rec.Sharp)
            //        sf--;
            //}
            //rec.JavaScript = rec.AllInfo().ContainsCI("JavaScript");

 *     private void Button1_Click(object sender, RoutedEventArgs e) => TextBox1.Text = "123";

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            string urlStr = TextBox1.Text;

            MyWebClient client = new MyWebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            Stream data = client.OpenRead(urlStr);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();

            TextBox2.Text = s;
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {

            string urlStr = TextBox1.Text;
            try
            {
                var client = new WebClient();
                client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                var responseStream = new GZipStream(client.OpenRead(urlStr), CompressionMode.Decompress);
                var reader = new StreamReader(responseStream);
                var textResponse = reader.ReadToEnd();
                TextBox2.Text = textResponse;
                // do stuff
            }
            catch { }

        }
 *         //= (x, y) => Math.Sqrt(x * x + y * y);
        //void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    pbCalculationProgress.Value = e.ProgressPercentage;
        //    if (e.UserState != null)
        //        lbResults.Items.Add(e.UserState);
        //}

                //WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;

 * 
 */
