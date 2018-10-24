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
using System.Windows.Controls.Primitives;




namespace WpfApp1
{


    
    public interface IView
    {
        string WebString { get; set; }
        double PBmax { get;  set; }
        void SpisokView(ObservableCollection<Record> Spisok);
        void YapView(ObservableCollection<q> qs);
        void PB_Update(MyEventArgs e);
      
        
    }

    public partial class MainWindow : Window, IView
    {


        private IPresentier Presentier;
        private IModel Model;
        private readonly SynchronizationContext SC;

        public ObservableCollection<Record> _spisok;

        public ObservableCollection<Record> Spisok
        {
            get { return _spisok; }
            set { _spisok = value; }
        }

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


        //====================================================
        public MainWindow() 
        {
            InitializeComponent();
            Model = new MyModel();
            Presentier = new MyPresentier(this, Model);
            SC = SynchronizationContext.Current;


            //Spisok = Model.GetData();
            //Binding binding = new Binding();
            //binding.Source = Spisok;
            //MyGrid.SetBinding(DataGrid.ItemsSourceProperty, binding);



            //MyGrid.ItemsSource = Spisok;
            //MyGrid.DataContext = Spisok;

            //MyGrid.RowStyle.Triggers

            //this.DataContext = this;





        }

        public void Main()
        {
        }

        private void Button1_Click(object sender, RoutedEventArgs e)  // Analize
        {
            Button1.IsEnabled = false;
            Presentier.AnalizeAsync();
        }

        private async void Button3_Click(object sender, RoutedEventArgs e)  //LoadFromWeb
        {
            Button3.IsEnabled = false;
            Button6.IsEnabled = true;
            TabControl1.SelectedIndex = 2;

            await Presentier.LoadFromWeb(TextBox1.Text, WebBrowser1, SC);
            //WebBrowser1.Navigate(TextBox1.Text);
            //await Application.Current.Dispatcher.BeginInvoke(new Action(() => { WebBrowser1.Navigate(TextBox1.Text); }));
        }

        private async void Button4_Click(object sender, RoutedEventArgs e)  //SaveToXLS
        {
            Button4.IsEnabled = false;
            await Presentier.SaveToXLS();
            Button4.IsEnabled = true;
        }

        private void Button5_Click(object sender, RoutedEventArgs e)  //LoadFromXLS
        {
            Button5.IsEnabled = false;
            Spisok = null;
            Presentier.LoadFromXLS(SC);
        }

        private void Button6_Click(object sender, RoutedEventArgs e)  //Cancel
        {
            Button6.IsEnabled = false;

            //SC.Post(new SendOrPostCallback(o => { PB.Value = 0; PB.Maximum = 1  ; }), e);
            //SC.Post(new SendOrPostCallback(o => { PBtext.Text = ""; }), e);
            //SC.Post(new SendOrPostCallback(o => { PB.Refresh(); }), e);



            Presentier.Cancel();
        }


        //====================================================

        public void SpisokView(ObservableCollection<Record> Spisok)
        {
            //MyGrid.row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            int e = 2;
            SC.Post(new SendOrPostCallback(o => { PB.Value = 0; PB.Maximum = 100; }), e);
            SC.Post(new SendOrPostCallback(o => { PBtext.Text = ""; }), e);
            SC.Post(new SendOrPostCallback(o => { PB.Refresh(); }), e);

            //MyGrid.ItemsSource = null;
            MyGrid.ItemsSource = Spisok;
            MyGrid.RowHeight = 20;
            MyGrid.Refresh();

            //this.DataContext = Spisok;



            //if (MyGrid.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            //{
            //    var row = (DataGridRow)MyGrid.ItemContainerGenerator.ContainerFromIndex(1);
            //    row.Background = Brushes.Red;
            //    //foreach (var dataItem in DisplayDataGrid.ItemsSource)
            //    //{
            //    //    var gridRow = DisplayDataGrid.ItemContainerGenerator.ContainerFromItem(dataItem) as DataGridRow;
            //    //}
            //    //DataGridRow rowColor = (DataGridRow)dataGridViewMyGroups.ItemContainerGenerator.ContainerFromIndex(number);
            //    //row.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(223, 227, 238));
            //}


            Button1.IsEnabled = true;
            Button4.IsEnabled = true;
            Button5.IsEnabled = true;
            Button3.IsEnabled = true;
            Button6.IsEnabled = false;
        }

        public void YapView(ObservableCollection<q> yap)
        {
            listbox1.ItemsSource = yap;
            Button1.IsEnabled = true;
        }

        public void PB_Update(MyEventArgs e)
        {
            //Application.Current.Dispatcher.BeginInvoke(new Action(() => { PB.Value = e.Value; PB.Maximum = e.MaxValue; }));
            SC.Post(new SendOrPostCallback(o => { PB.Value = e.Value; PB.Maximum = e.MaxValue; }), e);
            SC.Post(new SendOrPostCallback(o => { PBtext.Text = e.Value + " (" + e.Value2 + ") / " + e.MaxValue; }), e);
            SC.Post(new SendOrPostCallback(o => { PB.Refresh(); }), e);
            
            //synchronizationContext.Post(new SendOrPostCallback(o => {MyGrid.ItemsSource = Spisok;}),Spisok);
        }

        //====================================================

      

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
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

        private void MyGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()+1).ToString();
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
 *             //Spisok = await Task.Run( () => Model.LoadAsync(xlsloader));
            //Task<ObservableCollection<Record>> task;
            //task = Task<ObservableCollection<Record>>.Factory.StartNew(() => Model.LoadAsync(xlsloader), TaskCreationOptions.LongRunning);
            //Spisok = await task;
            //ObservableCollection<q> yap;
            //yap = await Task.Run(() => Model.AnalizeAsync());
            //View.YapView(yap);
            //Task<ObservableCollection<Record>> task = new Task<ObservableCollection<Record>>(() => Model.LoadAsync(xlsloader));
            //Spisok = await task;
            //task =  Task.Factory.StartNew(() => Model.LoadAsync(xlsloader));
            //task = Task<ObservableCollection<Record>>.Factory.StartNew(() => Model.LoadAsync(xlsloader));
            //MyEventArgs args = new MyEventArgs();
            //Spisok = Model.Load(xlsloader);
            //Spisok = await Task.Run( () => Model.LoadAsync(xlsloader));
            //xlsloader.Changed += new EventHandler(PB_Change);
            //args.Spisok = Model.LoadAsync(xlsloader, PB);
            //if (SpisokReady != null) SpisokReady(this, args);

            //Presentier.SpisokReady += SpisokView;
        //Button4.Click += Presentier.NavigateEv;
        //Button4.Click += new RoutedEventHandler(Button4_Click);
        //Button4.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button4_Click));
        //worker = new BackgroundWorker();
        //worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        //worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        //WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;
        //private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    TextBox2.Text = answer;
        //    MyGrid.ItemsSource = spisok;
        //}
        //void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    workerFunc();
        //    //ReadHH();
        //    //SaveToXLS();
        //}

                //Application.Current.Dispatcher.BeginInvoke(new Action(() => { MyGrid.ItemsSource = Spisok; }));
            //MyGrid.ItemsSource = e.Spisok;
            //synchronizationContext.Post(new SendOrPostCallback(o => {MyGrid.ItemsSource = Spisok;}),Spisok);
            //MyGrid.ItemsSource = Spisok;
            //PB.Maximum = yap.Count();
            //PB.Value = 0;
            //workerFunc = Analize;
            //worker.RunWorkerAsync();
            //PBtext.DataContext = this;
            //Binding binding = new Binding();
            //listbox1.SetBinding(ListBox.ItemsSourceProperty, binding);
            //listbox1.DataContext = yap;


                //MyGrid.ItemsSource = Spisok;

 * * 
 */
