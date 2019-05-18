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
using System.Data;
using System.Windows.Markup;



namespace WpfApp1
{



    public interface IView
    {
        string WebString { get; set; }
        string AlertString { get; set; }
        string AnyTextString { get; set; }
        double PBmax { get; set; }
        void SpisokView(ICollectionView SpisokFiltered);
        void YapView(ObservableCollection<AnaliseType> qs);
        void PB_Update(MyEventArgs e);

        event EventHandler<MyEventArgs> AutoClick;


    }

    public partial class MainWindow : Window, IView
    {


        private IPresentier Presentier;
        private IModel Model;
        private readonly SynchronizationContext SC;
        public event EventHandler<MyEventArgs> AutoClick = delegate { };




        public string WebString
        {
            get { return TextBox1.Text; }
            set { TextBox1.Text = value; }
        }

        public string AnyTextString
        {
            get
            {
                string result = null; 
                SC.Send(new SendOrPostCallback(o =>  {result = AnyTextText.Text; }), null);
                return result;
                //return AnyTextText.Text;
            }
            set { AnyTextText.Text = value; }
        }

        public string AlertString
        {
            get { return Alert.Content.ToString(); }
            set
            {
                SC.Post(new SendOrPostCallback(o => { Alert.Content = value; }), 2);
            }
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


            this.DataContext = Presentier;



            //Spisok = Model.GetData();
            //Binding binding = new Binding();
            //binding.Source = Spisok;
            //MyGrid.SetBinding(DataGrid.ItemsSourceProperty, binding);



            //MyGrid.ItemsSource = Spisok;
            //MyGrid.DataContext = Spisok;

            //MyGrid.RowStyle.Triggers

            //MyGrid.DataContext = this;





        }

        public void Main()
        {
        }

        private void Button1_Click(object sender, RoutedEventArgs e)  // Analize
        {
            Button1.IsEnabled = false;
            Presentier.AnalizeAsync((bool)chbOnlyFromName.IsChecked);
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

        private async void Button5_Click(object sender, RoutedEventArgs e)  //LoadFromXLS
        {
            Button5.IsEnabled = false;

            MyGrid.ItemsSource = null;

            await Presentier.LoadFromXLS(SC);
            //MyGrid.ItemsSource = Spisok;
            //MyGrid.Refresh();
        }

        private void Button6_Click(object sender, RoutedEventArgs e)  //Cancel
        {
            Button6.IsEnabled = false;

            //SC.Post(new SendOrPostCallback(o => { PB.Value = 0; PB.Maximum = 1  ; }), e);
            //SC.Post(new SendOrPostCallback(o => { PBtext.Text = ""; }), e);
            //SC.Post(new SendOrPostCallback(o => { PB.Refresh(); }), e);



            Presentier.Cancel();
        }

        private void Bold_Click(object sender, RoutedEventArgs e)  //Cancel
        {

            TextPointer potStart = RichTextBox1.Selection.Start;
            TextPointer potEnd = RichTextBox1.Selection.End;

            TextRange range = new TextRange(potStart, potStart.GetPositionAtOffset(1));
            Object obj = range.GetPropertyValue(TextElement.FontWeightProperty);

            if (obj != DependencyProperty.UnsetValue)
            {
                range = new TextRange(potStart, potEnd);
                FontWeight fontWeight = (FontWeight)obj;
                if (fontWeight == FontWeights.Bold) range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                if (fontWeight == FontWeights.Normal) range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                String text = XamlWriter.Save(RichTextBox1.Document);
                ((Record)MyGrid.SelectedItem).Desc = text;
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)  //Cancel
        {

            TextPointer potStart = RichTextBox1.Selection.Start;
            TextPointer potEnd = RichTextBox1.Selection.End;

            TextRange range = new TextRange(potStart, potStart.GetPositionAtOffset(1));
            Object obj = range.GetPropertyValue(TextElement.ForegroundProperty);

            if (obj != DependencyProperty.UnsetValue)
            {
                range = new TextRange(potStart, potEnd);


                SolidColorBrush fontBrush = (SolidColorBrush)obj;


                if (fontBrush.Color == Brushes.Black.Color)
                    range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);

                //new SolidColorBrush(GetColorFromString(rule.FontColor, (Brush)tr.GetPropertyValue(TextElement.ForegroundProperty))));


                if (fontBrush.Color == Brushes.Red.Color) range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                String text = XamlWriter.Save(RichTextBox1.Document);
                ((Record)MyGrid.SelectedItem).Desc = text;
            }
        }


        private void Auto_Click(object sender, RoutedEventArgs e)
        {
            MyEventArgs args = new MyEventArgs();
            args.flowDocument = RichTextBox1.Document;
            args.Rec = (Record)MyGrid.SelectedItem;
            if (AutoClick != null) AutoClick(sender, args);


        }



    
        //if (++cnt>2)
        //textRun = position.GetTextInRun(LogicalDirection.Forward);
        //cnt = textRun.Length;
        //textRun = position.GetTextInRun(LogicalDirection.Forward);
        //while (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
        //    position = position.GetNextContextPosition(LogicalDirection.Forward);
        //textRun = position.GetTextInRun(LogicalDirection.Forward);
        //cnt = textRun.Length;

        //            if (boldWords != null)
        //            {
        //                TextPointer position = flowDocument.ContentStart;
        //                while (position != null)
        //                {
        //                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
        //                    {
        //                        string textRun = position.GetTextInRun(LogicalDirection.Forward);

        //                        // Find the starting index of any substring that matches "word".
        //                        foreach (String word in boldWords)
        //                        {
        //                            indexInRun = textRun.IndexOf(word, StringComparison.CurrentCultureIgnoreCase);

        //                            while (indexInRun != -1)
        //                            {
        //                                TextPointer start = position.GetPositionAtOffset(indexInRun);
        //        TextPointer end = start.GetPositionAtOffset(word.Length);
        //        TextRange selection = new TextRange(start, end);
        //        selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        //                                indexInRun = textRun.IndexOf(word, indexInRun + 1, StringComparison.CurrentCultureIgnoreCase);
        //                            }
        //}
        //position = position.GetPositionAtOffset(textRun.Length);
        //                    }
        //                    else
        //                    {
        //                        position = position.GetNextContextPosition(LogicalDirection.Forward);
        //                    }
        //                }
        //            }


        //                        if (boldWords != null)
        //            {
        //                position = flowDocument.ContentStart;
        //                text = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd).Text;

        //                // Find the starting index of any substring that matches "word".
        //                foreach (String word in boldWords)
        //                {
        //                    indexInText = text.IndexOf(word, StringComparison.CurrentCultureIgnoreCase);

        //                    while (indexInText != -1)
        //                    {
        //                        TextPointer start = position.GetPositionAtOffset(indexInText);
        //        TextPointer end = start.GetPositionAtOffset(word.Length);
        //        TextRange selection = new TextRange(start, end);
        //        selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        //                        indexInText = text.IndexOf(word, indexInText + 4, StringComparison.CurrentCultureIgnoreCase);
        //                    }
        //}
        //            }



        private void TodayChanges_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void TodayChanges_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void ClosedVacancy_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void ClosedVacancy_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void Sharp_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void Sharp_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void JavaScript_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void JavaScript_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void SQL_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void SQL_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void oCwo_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void oCwo_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void Distant_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void Distant_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void ActiveVacancy_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void ActiveVacancy_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void Intrst_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void Intrst_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void AnyText_Checked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void AnyText_Unchecked(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);



        private void JavaScript_Indeterminate(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void Sharp_Indeterminate(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void SQL_Indeterminate(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void oCwo_Indeterminate(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);
        private void AnyText_Indeterminate(object sender, RoutedEventArgs e) => Presentier.CheckBoxesFilterUpdate(sender);



        //private void JavaScript_Checked(object sender, RoutedEventArgs e) => MessageBox.Show(JavaScript.IsChecked.ToString());
        //private void JavaScript_Unchecked(object sender, RoutedEventArgs e) => MessageBox.Show(JavaScript.IsChecked.ToString());
        //private void JavaScript_Indeterminate(object sender, RoutedEventArgs e) => MessageBox.Show(JavaScript.IsChecked.ToString());





        //private void Test_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show(string.Format("Sit: {0}", Presentier.ActiveCheckBox));
        //}

        //====================================================
        private void MyGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String text;
            FlowDocument flowDocument = null;

            if (MyGrid.SelectedItem != null)
            {
                RichTextBox1.Document.Blocks.Clear();
                text = ((Record)MyGrid.SelectedItem).Desc;
                flowDocument = XamlReader.Parse(text) as FlowDocument;
                RichTextBox1.Document = flowDocument;
            }
        }




        public void SpisokView(ICollectionView SpisokFiltered)
        {
            int vacCount = SpisokFiltered.Cast<object>().Count();
            VacancyCount.Content = vacCount;

            if (vacCount != 0)
            {
                SC.Post(new SendOrPostCallback(o => { PB.Value = 0; PB.Maximum = 100; }), 2);
                SC.Post(new SendOrPostCallback(o => { PBtext.Text = ""; }), 2);
                SC.Post(new SendOrPostCallback(o => { PB.Refresh(); }), 2);
                MyGrid.ItemsSource = SpisokFiltered;
                MyGrid.RowHeight = 20;
                Button1.IsEnabled = true;
                Button4.IsEnabled = true;
                Button5.IsEnabled = true;
                Button3.IsEnabled = true;
                Button6.IsEnabled = false;
            }

            //MyGrid.ItemsSource = null;
            //MyGrid.Refresh();
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
        }

        public void YapView(ObservableCollection<AnaliseType> yap)
        {
            listbox1.ItemsSource = null;
            listbox1.ItemsSource = yap;
            //listbox1.Refresh();
            listbox1.Items.SortDescriptions.Clear();
            Button1.IsEnabled = true;
            AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
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
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextPointer position = RichTextBox1.Document.ContentStart;
            //String word = "Требования";
            //String t = new TextRange(RichTextBox1.Document.ContentStart, RichTextBox1.Document.ContentEnd).Text;


            //while (position != null)
            //{
            //    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            //    {
            //        string textRun = position.GetTextInRun(LogicalDirection.Forward);

            //        // Find the starting index of any substring that matches "word".
            //        int indexInRun = textRun.IndexOf(word);
            //        if (indexInRun > 0)
            //        {
            //            TextPointer start = position.GetPositionAtOffset(indexInRun);
            //            TextPointer end = start.GetPositionAtOffset(word.Length);
            //            TextRange selection = new TextRange(start, end);
            //            selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

            //        }
            //    }

            //    position = position.GetNextContextPosition(LogicalDirection.Forward);
            //}



            //string keyword = "Требования";
            //string newString = "!!!!NewString!!!!";
            //TextRange text = new TextRange(RichTextBox1.Document.ContentStart, RichTextBox1.Document.ContentEnd);
            //TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
            //while (current != null)
            //{
            //    string textInRun = current.GetTextInRun(LogicalDirection.Forward);
            //    if (!string.IsNullOrWhiteSpace(textInRun))
            //    {
            //        int index = textInRun.IndexOf(keyword);
            //        if (index != -1)
            //        {
            //            TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
            //            TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keyword.Length, LogicalDirection.Forward);
            //            TextRange selection = new TextRange(selectionStart, selectionEnd);
            //            selection.Text = newString;
            //            selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            //            RichTextBox1.Selection.Select(selection.Start, selection.End);
            //            RichTextBox1.Focus();
            //        }
            //    }
            //    current = current.GetNextContextPosition(LogicalDirection.Forward);
            //}



            //TextRange textRange = new TextRange(RichTextBox1.Document.ContentStart, RichTextBox1.Document.ContentEnd);
            //textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);

            //TextRange tr = new TextRange(RichTextBox1.Document.ContentStart), RichTextBox1.Document.ContentEnd); 


            //if (RichTextBox1.Document.Text.Contains("прогр"))
            //{
            //    RichTextBox1.Select(RichTextBox1.Text.IndexOf("hi"), "hi".Length);
            //    RichTextBox1.SelectionColor = Color.Aqua;
            //}
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

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void AnyTextText_TextChanged(object sender, TextChangedEventArgs e)
        {

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
            //(listbox1.ItemsSource as ObservableCollection<AnaliseType>).RemoveAt(0);
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
            //ObservableCollection<AnaliseType> yap = new ObservableCollection<AnaliseType>();
            //int size = numbers.Count(i => i % 2 == 0 && i > 10);
            //yap.ForEach<AnaliseType>(p => p.count2 = p.Name);
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

                //yap.ForEach<AnaliseType>(p => p.count = spisok.Count(t => t.AllInfo().ContainsCI(p.Name)));
            //foreach (AnaliseType p in yap)
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
            //Task<List<Record>> task;
            //task = Task<List<Record>>.Factory.StartNew(() => Model.LoadAsync(xlsloader), TaskCreationOptions.LongRunning);
            //Spisok = await task;
            //ObservableCollection<AnaliseType> yap;
            //yap = await Task.Run(() => Model.AnalizeAsync());
            //View.YapView(yap);
            //Task<List<Record>> task = new Task<List<Record>>(() => Model.LoadAsync(xlsloader));
            //Spisok = await task;
            //task =  Task.Factory.StartNew(() => Model.LoadAsync(xlsloader));
            //task = Task<List<Record>>.Factory.StartNew(() => Model.LoadAsync(xlsloader));
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
                            //FlowDocument content = XamlReader.Load(text) as FlowDocument;

            //MyGrid.
            //int countTopLevelBlocks = RichTextBox1.Document.Blocks.Count;
            //RichTextBox1.Document.Blocks.Remove(RichTextBox1.Document.Blocks.LastBlock);

                    if (text[0] == '<') flowDocument = XamlReader.Parse(text) as FlowDocument;

                if (flowDocument !=null)
                    RichTextBox1.Document = flowDocument;
                else
                    RichTextBox1.Document.Blocks.Add(new Paragraph(new Run(text)));

                //String t = new TextRange(RichTextBox1.Document.ContentStart, RichTextBox1.Document.ContentEnd).Text;

                //range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);


 * * 
 */
