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
    delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

    public class Record
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Zp { get; set; }
        public string Comp { get; set; }
        public string Town { get; set; }
        public string Resp1 { get; set; }
        public string Req1 { get; set; }
        public string Dat { get; set; }
        public string Opt { get; set; }
        public StringBuilder Desc { get; set; } = new StringBuilder();
        public bool Sharp { get; set; }
        public bool JavaScript { get; set; }
        public bool Distant { get; set; }

        public string AllInfo() => Name + Zp + Comp + Town + Resp1 + Req1 + Dat + Opt + Desc.ToString();
    }

    public class q
    {
        public string Name { get; set; }
        public int count { get; set; }
        public q(string s)
        { Name = s; count = 0; }

        public string NameRus() => Name.Replace("C", "С");
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

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string answer, webs;
        BackgroundWorker worker;
        Excel.Application excelApp;
        Excel.Workbook workBook;
        Excel.Worksheet workSheet;
        Excel.Range range;
        Process[] excelProcsOld;

        public IList<Record> spisok = new List<Record>();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        ObservableCollection<q> yap;


        delegate void spfunc();
        spfunc workerFunc;

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
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public MainWindow() 
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            //WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;

            InitializeComponent();
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

            //MyGrid.Items = new List<Record>();
            //foreach (var item in itemsToAdd)
            //{
            //    MyGrid.Items.Add(item);
            //}

            MyGrid.ItemsSource = spisok;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            PB.Maximum = 345;// ReadHHCountVac();
            PB.Value = 0;

            TextBox2.Text = "";
            webs = TextBox1.Text;
           
            WebBrowser.Navigate(webs);

            workerFunc = ReadHH_WB;
            worker.RunWorkerAsync();
        }

        
        
        //= (x, y) => Math.Sqrt(x * x + y * y);

        
        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            PB.Maximum = 345;// ReadHHCountVac();
            PB.Value = 0;

            workerFunc = LoadFromXLS;
            worker.RunWorkerAsync();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            PB.Maximum = yap .Count();
            PB.Value = 0;

            workerFunc = Analize;
            worker.RunWorkerAsync();
        }


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            workerFunc();
            //ReadHH();
            //SaveToXLS();
        }

        private void ReadHH()
        {
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);

            int i = 0, p = 0;
            HtmlWeb web;
            HtmlNodeCollection nodes;
            HtmlNode root;
            HtmlDocument document, document2;
            Record rec;
            string text;

            double value = 0;
            answer = "";


            web = new HtmlWeb();
            document = web.Load(webs);
            nodes = document.DocumentNode.SelectNodes("//div[@class='vacancy-serp-item ' or @class='vacancy-serp-item  vacancy-serp-item_premium']");
            while (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    rec = new Record();
                    rec.Name = node.SelectSingleNode(".//div[@class='search-item-name']")?.InnerText ?? "";
                    string link = node.SelectSingleNode(".//a[@class='bloko-link HH-LinkModifier']")?.Attributes["href"].Value;
                    rec.Zp = node.SelectSingleNode(".//div[@class='vacancy-serp-item__compensation']")?.InnerText ?? "";
                    rec.Comp = node.SelectSingleNode(".//div[@class='vacancy-serp-item__meta-info']")?.InnerText ?? "";
                    rec.Town = node.SelectSingleNode(".//span[@class='vacancy-serp-item__meta-info']")?.InnerText ?? "";
                    rec.Resp1 = node.SelectSingleNode(".//div[@data-qa='vacancy-serp__vacancy_snippet_responsibility']")?.InnerText ?? "";
                    rec.Req1 = node.SelectSingleNode(".//div[@data-qa='vacancy-serp__vacancy_snippet_requirement']")?.InnerText ?? "";
                    rec.Dat = node.SelectSingleNode(".//span[@class='vacancy-serp-item__publication-date']")?.InnerText ?? "";

                    text = node.SelectSingleNode(".//script[@data-name='HH/VacancyResponsePopup/VacancyResponsePopup']")?.Attributes["data-params"]?.Value ?? "0";

                    rec.Id = Regex.Match(text, @"\d+").Value;



                    document2 = web.Load(link);
                    rec.Opt = document2.DocumentNode.SelectSingleNode("(//div[@class='bloko-gap bloko-gap_bottom'])[3]")?.InnerText ?? "";
                    root = document2.DocumentNode.SelectSingleNode("//div[@class='g-user-content' or @data-qa='vacancy-description']");

                    foreach (HtmlNode node2 in root.DescendantsAndSelf())
                    {
                        if (!node2.HasChildNodes)
                        {
                            text = node2.InnerText;
                            if (!string.IsNullOrWhiteSpace(text))
                                rec.Desc.AppendLine(text);
                        }
                    }


                    //yap.ForEach<q>(p => p.count = spisok.Count(t => t.AllInfo().СontainsCI(p.Name) || t.AllInfo().СontainsCI(p.NameRus())));

                    rec.Sharp = rec.AllInfo().ContainsCI("C#") || rec.AllInfo().ContainsCI("С#") || rec.AllInfo().ContainsCI(".NET");
                    rec.JavaScript = rec.AllInfo().ContainsCI("JavaScript");
                    rec.Distant = rec.AllInfo().ContainsCI("удал");

                    spisok.Add(rec);


                    answer += ++i + ". ";
                    answer += "Name: " + rec.Name + "   ";
                    answer += "Link: " + link + "   ";
                    answer += "ZP: " + rec.Zp + "   ";
                    answer += "Comp: " + rec.Comp + "   ";
                    answer += "Town: " + rec.Town + "   ";
                    answer += "Resp1: " + rec.Resp1 + "   ";
                    answer += "Req1: " + rec.Req1 + "   ";
                    answer += "Dat: " + rec.Dat + "   ";
                    answer += "Opt: " + rec.Opt + "\r\n\r\n";
                    answer += "Desc: " + rec.Desc + "   ";

                    answer += "\r\n\r\n";

                    Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                    Pbprc= value/350*100;
                }

                document = web.Load(webs + "/page-" + ++p);
                nodes = document.DocumentNode.SelectNodes("//div[@class='vacancy-serp-item ']");

            }
        }



        private void LoadFromXLS()
        {
            int i, j, k;
            excelProcsOld = Process.GetProcessesByName("EXCEL");
            excelApp = new Excel.Application();
            object[,] tmp;
            Record rec;
            double value = 0;


            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);

            range = null;
            string path = Directory.GetCurrentDirectory() + "\\Spisok.xlsx";
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open XLS File";
            theDialog.Filter = "XLS files|*.xlsx";
            theDialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (theDialog.ShowDialog() == true) path = theDialog.FileName;



            workBook = excelApp.Workbooks.Open(path, 0, true, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            workSheet = (Excel.Worksheet)workBook.Worksheets.get_Item(1);

            range = workSheet.get_Range("A1", Missing.Value);
            range = range.get_End(Excel.XlDirection.xlDown);
            j = range.Row - 1;
            range = workSheet.get_Range("A1", Missing.Value);
            range = range.get_End(Excel.XlDirection.xlToRight);
            k = range.Column;

            range = workSheet.get_Range((Excel.Range)workSheet.Cells[2, 1], (Excel.Range)workSheet.Cells[j + 1, k]);
            tmp = range.Value2;
            spisok.Clear();

            //rec.Clear();
            //public IList<Record> spisok = new List<Record>();


            for (i = 1; i <= j; i++)
            {
                rec = new Record();
                rec.Name = tmp[i, 1]?.ToString();
                rec.Zp = tmp[i, 2]?.ToString();
                rec.Comp = tmp[i, 3]?.ToString();
                rec.Town = tmp[i, 4]?.ToString();
                rec.Resp1 = tmp[i, 5]?.ToString();
                rec.Req1 = tmp[i, 6]?.ToString();
                rec.Dat = tmp[i, 7]?.ToString();
                rec.Opt = tmp[i, 8]?.ToString();
                rec.Desc.Append(tmp[i, 9]?.ToString());
                rec.Id = tmp[i, 10].ToString();
                rec.Sharp = Convert.ToBoolean(tmp[i, 11]);
                rec.JavaScript = Convert.ToBoolean(tmp[i, 12]);
                rec.Distant = Convert.ToBoolean(tmp[i, 13]);

                spisok.Add(rec);
                Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                Pbprc = value / 350 * 100;
            }



            QuitExcel();
        }


        private void ReadHH_WB()
        {
        }


        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string HTML, text;
            HtmlDocument hDocument;

            mshtml.IHTMLDocument2 doc = WebBrowser.Document as mshtml.IHTMLDocument2;
            HTML = doc.body.outerHTML;

            hDocument = new HtmlDocument();
            hDocument.LoadHtml(HTML);

            text = hDocument.DocumentNode.SelectSingleNode("//div[@data-qa='vacancies-total-found']")?.InnerText ?? "";
            TextBox2.Text = text + "\r\n" + HTML;
        }




        //void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    pbCalculationProgress.Value = e.ProgressPercentage;
        //    if (e.UserState != null)
        //        lbResults.Items.Add(e.UserState);
        //}

        private void Analize()
        {
            double value = 0;
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);
           
            //yap.ForEach<q>(p => p.count = spisok.Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus())));

            foreach (q p in yap)
            {
                p.count = spisok.Count(t => t.AllInfo().ContainsCI(p.Name) || t.AllInfo().ContainsCI(p.NameRus()));

                Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                Pbprc = value / yap.Count() * 100;

            }





        }

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

        private double ReadHHCountVac()
        {
            HtmlWeb web;
            HtmlDocument document;
            string text;

            web = new HtmlWeb();
            document = web.Load(webs);

            //<div data-qa="vacancies-total-found" class="header__minor">Найдено 345 вакансий</div>


            text = document.DocumentNode.SelectSingleNode("//div[@data-qa='vacancies-total-found']")?.InnerText ?? "";
            text = Regex.Match(text, @"\d+").Value;
            return Double.Parse(text);

        }

        public void SaveToXLS()
        {
            excelProcsOld = Process.GetProcessesByName("EXCEL");
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);
            //[по вертикали, по горизонтали]
            excelApp = new Excel.Application();
            string path;
            path = Directory.GetCurrentDirectory() + "\\Spisok.xlsx";
            workBook = excelApp.Workbooks.Add();
            workSheet = (Excel.Worksheet)workBook.Worksheets.get_Item(1);
            workSheet.Cells[1, 1] = "Вакансия";
            workSheet.Cells[1, 2] = "ЗП";
            workSheet.Cells[1, 3] = "Компания";
            workSheet.Cells[1, 4] = "Город";
            workSheet.Cells[1, 5] = "ЧтоДелать";
            workSheet.Cells[1, 6] = "Требования1";
            workSheet.Cells[1, 7] = "Дата вак";
            workSheet.Cells[1, 8] = "Опыт";
            workSheet.Cells[1, 9] = "Описание";
            workSheet.Cells[1, 10] = "Id";
            workSheet.Cells[1, 11] = "C#";
            workSheet.Cells[1, 12] = "JavaScript";
            workSheet.Cells[1, 13] = "удаленно";
            int i = 2;
            double value = 0;

            foreach (Record rec in spisok)
            {
                workSheet.Cells[i, 1] = rec.Name;
                workSheet.Cells[i, 2] = rec.Zp;
                workSheet.Cells[i, 3] = rec.Comp;

                workSheet.Cells[i, 4] = rec.Town;
                workSheet.Cells[i, 5] = rec.Resp1;
                workSheet.Cells[i, 6] = rec.Req1;
                workSheet.Cells[i, 7] = rec.Dat;
                workSheet.Cells[i, 8] = rec.Opt;
                workSheet.Cells[i, 9] = rec.Desc.ToString();
                workSheet.Cells[i, 10] = rec.Id;
                workSheet.Cells[i, 11] = rec.Sharp;
                workSheet.Cells[i, 12] = rec.JavaScript;
                workSheet.Cells[i, 13] = rec.Distant;

                workSheet.Rows[i].RowHeight = 15;

                i++;
                Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
            }

            for (i = 1; i < 10; i++) workSheet.Columns.ColumnWidth = 30;

            excelApp.Application.ActiveWorkbook.SaveAs(path, Type.Missing,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            QuitExcel();

        }

        public void QuitExcel()
        {
                object misValue = System.Reflection.Missing.Value;
                IntPtr xAsIntPtr = new IntPtr(excelApp.Hwnd);
                if (excelApp != null)
                {

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);


                    foreach (Microsoft.Office.Interop.Excel.Worksheet sheet in workBook.Worksheets)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workBook.Worksheets);
                    excelApp.DisplayAlerts = false;

                    workBook.Close(false, misValue, misValue);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workBook);


                    excelApp.Application.Quit();
                    excelApp.Quit();
                    excelApp.DisplayAlerts = true;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

                    excelApp = null;
                    workBook = null;
                    workSheet = null;
                    range = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                }


                Process[] excelProcsNew = Process.GetProcessesByName("EXCEL");
                foreach (Process procNew in excelProcsNew)
                {
                    int exist = 0;
                    foreach (Process procOld in excelProcsOld)
                    {
                        if (procNew.Id == procOld.Id)
                        {
                            exist++;
                        }
                    }
                    if (exist == 0)
                    {
                        procNew.Kill();
                    }
                }
            }

  


    }


    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }
    }


}


/*
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
 * 
 * 
 */
