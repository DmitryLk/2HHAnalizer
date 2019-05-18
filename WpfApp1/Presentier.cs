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
using System.Windows.Markup;


namespace WpfApp1
{

    delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

    public interface IPresentier
    {
        Task LoadFromXLS(SynchronizationContext SC);
        Task SaveToXLS();
        Task LoadFromWeb(string webs, WebBrowser wb, SynchronizationContext SC);
        Task AnalizeAsync(bool onlyFromNames);
        void Cancel();
        void CheckBoxesFilterUpdate(object sender);

        //event EventHandler<MyEventArgs> SpisokReady;
    }


    public class MyPresentier : IPresentier
    {
        private readonly IView View;
        private readonly IModel Model;
        private CancellationTokenSource _tokensource;
        CancellationToken token;
        //public event EventHandler<MyEventArgs> SpisokReady = delegate { };

        public bool IntrstCheckBox { get; set; }
        public bool ActiveCheckBox { get; set; }
        public bool ClosedCheckBox { get; set; }
        public bool? SharpCheckBox { get; set; }
        public bool? JavaScriptCheckBox { get; set; }
        public bool? FrontCheckBox { get; set; }
        public bool? oneCCheckBox { get; set; }
        public bool DistantCheckBox { get; set; }
        public bool TodayChangesCheckBox { get; set; }
        public bool? AnyTextCheckBox { get; set; }



        public MyPresentier(IView View, IModel Model)
        {
            this.View = View;
            this.Model = Model;
            Model.Changed += ProgressBar_Change;

            Model.StartPause += AlertOn;
            Model.EndPause += AlertOff;
            View.AutoClick += TextBoldinginFlowDocument;


            IntrstCheckBox = false;
            ActiveCheckBox = true;
            ClosedCheckBox = true;
            SharpCheckBox = null;
            JavaScriptCheckBox = null;
            FrontCheckBox = null;
            oneCCheckBox = null;
            AnyTextCheckBox = null;

            DistantCheckBox = false;
            TodayChangesCheckBox = false;


        }

        public ICollectionView SpisokFiltered
        {
            get { return CollectionViewSource.GetDefaultView(Model.GetData()); }
        }

        public ICollectionView GetFilteredData()
        {
            SpisokFiltered.Filter = new Predicate<object>(o => Filter(o as Record));
            SpisokFiltered.Refresh();

            return SpisokFiltered;

        }

        public void CheckBoxesFilterUpdate(object sender)
        {
            View.SpisokView(GetFilteredData());
        }


        string[] _frontString = new string[] { "фронт", "front", "script", "react", "angular", "vue", "веб", "web" };
        private bool Filter(Record rec)
        {
            string anytext;
            string[] words;
            string searchString;
            bool anyTextBool;
            bool flFront;


            if (JavaScriptCheckBox==true && rec.JavaScript == false || JavaScriptCheckBox == false && rec.JavaScript == true) return false;

            if (SharpCheckBox == true && rec.Sharp == false || SharpCheckBox == false && rec.Sharp == true) return false;


            if (FrontCheckBox != null)
            {
                flFront = false;
                foreach (var word in _frontString)
                {
                    if (rec.Name.ContainsCI(word))
                        flFront = true;
                }
                if (FrontCheckBox == true && flFront == false || FrontCheckBox == false && flFront == true) return false;
            }





            if (oneCCheckBox == true && rec._1C == false || oneCCheckBox == false && rec._1C == true) return false;


            if (AnyTextCheckBox != null)
            {
                searchString = rec.AllInfo();
                anytext = View.AnyTextString;
                words = anytext.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    if (word.Trim() != "")
                    {
                        anyTextBool = searchString.ContainsCI(word);
                        if (AnyTextCheckBox == true && anyTextBool == false || AnyTextCheckBox == false && anyTextBool == true) return false;
                    }
                        
                }

            }




            if (IntrstCheckBox) if (rec.Interes == false) return false;
            if (DistantCheckBox) if (rec.Distant == false) return false;

            if (!ActiveCheckBox) if (rec.Closed == false) return false;
            if (!ClosedCheckBox) if (rec.Closed == true) return false;

            if (TodayChangesCheckBox)
            {
                if (rec.Closed == false && (DateTime.Now - rec.BeginingDate).TotalHours < 12) return true;
                if (rec.Closed == true && (DateTime.Now - rec.LastCheckDate).TotalHours < 36) return true;
                return false;
            }
            return true;
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
            
            //IList<Record> Spisok = null;
            //Task<List<Record>> task = Task<List<Record>>.Run(() => Model.LoadAsync(loader));
            try
            {
                _tokensource = new CancellationTokenSource();
                token = _tokensource.Token;
                await Task.Run(() => Model.LoadAsync(loader, token), token);
            }
            catch (OperationCanceledException)
            {
                //Spisok = Model.GetData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


            View.SpisokView(GetFilteredData());
            
            
        }

        public void Cancel()
        {
            _tokensource.Cancel();
        }

        public async Task AnalizeAsync(bool onlyFromNames)
        {
            ObservableCollection<AnaliseType> yap;
            yap = await Task.Run(() => Model.AnalizeAsync(GetFilteredData(), onlyFromNames));
            View.YapView(yap);
        }

        public void ProgressBar_Change(object sender, MyEventArgs e)
        {
            
            View.PB_Update(e);
            //System.Threading.Thread.Sleep(10);
           
        }

        public void AlertOn(object sender, MyEventArgs e)
        {
            View.AlertString = "ПАУЗА";
        }

        public void AlertOff(object sender, MyEventArgs e)
        {
            View.AlertString = "";
        }


        public void TextBoldinginFlowDocument(object sender, MyEventArgs e)
        {


            FlowDocument flowDocument = e.flowDocument;
            TextPointer position;
            bool flag, red;
            string text;

            string[] redwords = new string[] { "1C", "1С", "Angular", "AngularJS", "D3.js", "es5", "es6", "ExtJS", "JavaScript", "jQuery", "js", "linux", "marionettejs", "MongoDB", "Node.js", "PHP", "powershell", "Python", "React", "sharepoint", "typescript", "ubuntu", "unity", "unity3d", "unix", "Vue.js", "xquery" };


            ObservableCollection<AnaliseType> MyQ = Model.GetQData();


            foreach (AnaliseType word in MyQ)
            {
                position = flowDocument.ContentStart;
                flag = true;
                while (flag)
                {
                    red = redwords.Contains(word.Name);
                    flag = BoldTextFromPosition(position, word.Name, red);
                }
            }

            text = XamlWriter.Save(flowDocument);
            (e.Rec).Desc = text;
        }

        private bool BoldTextFromPosition(TextPointer position, string word, bool red) 
        {
            TextRange range;
            Object obj;
            int indexInRun;
            string textRun;
            FontWeight fontWeight;
            TextPointer start, end;
            TextRange selection;
            string pattern = @"[a-zA-Zа-яА-Я0-9]";
            bool fl;


            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    textRun = position.GetTextInRun(LogicalDirection.Forward);

                    indexInRun = textRun.IndexOf(word, StringComparison.CurrentCultureIgnoreCase);
                    while (indexInRun != -1)
                    {
                        //проверка окаймляющих символов - должны быть nonword
                        fl = false; // -  не найдены word символы 
                        // символ перед словом
                        start = position.GetPositionAtOffset(indexInRun-1);
                        range = new TextRange(start, start.GetPositionAtOffset(1));
                        if (Regex.IsMatch(range.Text, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase)) fl = true;

                        // символ после слова
                        start = position.GetPositionAtOffset(indexInRun + word.Length);
                        range = new TextRange(start, start.GetPositionAtOffset(1));
                        if (Regex.IsMatch(range.Text, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase)) fl = true;

                        //конец проверка окаймляющих символов - должны быть nonword



                        if (!fl)
                        {
                            start = position.GetPositionAtOffset(indexInRun);
                            range = new TextRange(start, start.GetPositionAtOffset(1));
                            obj = range.GetPropertyValue(TextElement.FontWeightProperty);
                            fontWeight = (FontWeight)obj;
                            if (fontWeight == FontWeights.Normal)
                            {
                                end = start.GetPositionAtOffset(word.Length);
                                selection = new TextRange(start, end);
                                selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                                if (red) selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);

                                return true;
                            }
                        }


                        indexInRun = textRun.IndexOf(word, indexInRun + 1, StringComparison.CurrentCultureIgnoreCase);
                    }
                    position = position.GetPositionAtOffset(textRun.Length);
                }
                else
                {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
            return false;

        }
        //private ILoader loader;


        //public string SourceIp { get; set; }
        //public Listen(string sourceIp)
        //{
        //    SourceIp = sourceIp;
        //}


    }
}
