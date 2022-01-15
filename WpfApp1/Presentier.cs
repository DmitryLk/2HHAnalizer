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
    internal delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

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
        private CancellationToken token;
        //public event EventHandler<MyEventArgs> SpisokReady = delegate { };

        public bool? IntrstCheckBox { get; set; }
        public bool ActiveCheckBox { get; set; }
        public bool ClosedCheckBox { get; set; }
        public bool? SharpCheckBox { get; set; }
        public bool? JavaScriptCheckBox { get; set; }
        public bool? FrontCheckBox { get; set; }
        public bool? oneCCheckBox { get; set; }
        public bool DistantCheckBox { get; set; }
        public bool TodayChangesCheckBox { get; set; }
        public bool? AnyTextCheckBox { get; set; }
        public bool BigZPCheckBox { get; set; }
        public bool internationCheckBox { get; set; }

        public bool? MoscowCheckBox { get; set; }
        public bool? KrasnodarCheckBox { get; set; }
        public bool? RostovCheckBox { get; set; }
        public bool? SpbCheckBox { get; set; }
        public bool? LanguageCheckBox { get; set; }

        public object LangComboBox { get; set; }

        private string[] _frontString = new string[] { "фронт", "front", "script", "react", "angular", "vue", "веб", "web" };
        private string[] _internationString = new string[] { "международн", "америк", "сша", "европ" };

        public MyPresentier(IView View, IModel Model)
        {
            this.View = View;
            this.Model = Model;
            Model.Changed += ProgressBar_Change;

            Model.StartPause += AlertOn;
            Model.EndPause += AlertOff;
            View.AutoClick += TextBoldinginFlowDocument;

            IntrstCheckBox = null;
            ActiveCheckBox = true;
            ClosedCheckBox = true;
            SharpCheckBox = null;
            JavaScriptCheckBox = null;
            FrontCheckBox = null;
            oneCCheckBox = null;
            AnyTextCheckBox = null;
            BigZPCheckBox = false;
            internationCheckBox = false;

            DistantCheckBox = false;
            TodayChangesCheckBox = false;

            MoscowCheckBox = null;
            KrasnodarCheckBox = null;
            RostovCheckBox = null;
            SpbCheckBox = null;
            LanguageCheckBox = null;

            LangComboBox = null;
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

        private bool Filter(Record rec)
        {
            string anytext;
            string[] words;
            string searchString;
            bool anyTextBool;
            bool flFront;
            bool fl;
            string recall = rec.AllInfo();
            int languageIndex = 0;
            int java_index;
            int script_index;

            if (JavaScriptCheckBox == true && rec.JavaScript == false || JavaScriptCheckBox == false && rec.JavaScript == true) return false;

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
                searchString = recall;
                anytext = View.AnyTextString;
                words = anytext.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    if (word.Trim() != "")
                    {
                        anyTextBool = searchString.ContainsCI(word);
                        if (AnyTextCheckBox == true && anyTextBool == false || AnyTextCheckBox == false && anyTextBool == true) return false;
                    }
                }
            }

            if (IntrstCheckBox == true && rec.Interes !=true) return false;
	        if (IntrstCheckBox == false && rec.Interes == false) return false;
           
            
            if (DistantCheckBox) if (rec.Distant == false) return false;

            if (!ActiveCheckBox) if (rec.Closed == false) return false;
            if (!ClosedCheckBox) if (rec.Closed == true) return false;

            if (TodayChangesCheckBox)
            {
                if (rec.Closed == false && (DateTime.Now - rec.BeginingDate).TotalHours < 12) return true;
                if (rec.Closed == true && (DateTime.Now - rec.LastCheckDate).TotalHours < 36) return true;
                return false;
            }

            if (LanguageCheckBox != null)
            {
                languageIndex = rec.LanguageIndex;
                switch (LangComboBox)
                {
                    case "C#":
                        fl = false;
                        if (languageIndex == 7 || languageIndex == 8 || languageIndex == 9) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "C++":
	                    fl = false;
	                    if (languageIndex == 5 || languageIndex == 6) fl = true;
	                    if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
	                    break;

	                case "Go":
	                    fl = false;
	                    if (languageIndex == 16 || languageIndex == 17) fl = true;
	                    if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
	                    break;

                    case "Java":
                        fl = true;
                        java_index = recall.IndexOf("java", StringComparison.CurrentCultureIgnoreCase);
                        script_index = recall.IndexOf("script", StringComparison.CurrentCultureIgnoreCase);
                        if (java_index == -1) fl = false;
                        if (script_index - java_index >= 4 && script_index - java_index <= 6) fl = false;
                        if (script_index != -1 && script_index < java_index) fl = false;
                        if (languageIndex != 19) fl = false;
                        if (LanguageCheckBox == false && fl == true || LanguageCheckBox == true && fl == false) return false;
                        break;

                    case "JavaScript":
                        fl = true;
                        java_index = recall.IndexOf("java", StringComparison.CurrentCultureIgnoreCase);
                        script_index = recall.IndexOf("script", StringComparison.CurrentCultureIgnoreCase);
                        if (script_index == -1) fl = false;
                        if (script_index - java_index < 4 || script_index - java_index > 6) fl = false;
                        if (java_index != -1 && script_index > java_index) fl = false;
                        if (languageIndex != 19) fl = false;
                        if (LanguageCheckBox == false && fl == true || LanguageCheckBox == true && fl == false) return false;
                        break;

                    case "PHP":
                        fl = false;
                        if (languageIndex == 3) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Python":
                        fl = false;
                        if (languageIndex == 4) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "1C":
                        fl = false;
                        if (languageIndex == 1 || languageIndex == 2) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Perl":
                        fl = false;
                        if (languageIndex == 10) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Ruby":
                        fl = false;
                        if (languageIndex == 11) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Scala":
                        fl = false;
                        if (languageIndex == 12) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Cotlin":
                        fl = false;
                        if (languageIndex == 13) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Delphi":
                        fl = false;
                        if (languageIndex == 14) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Swift":
                        fl = false;
                        if (languageIndex == 15) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Golang":
                        fl = false;
                        if (languageIndex == 17) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "Lua":
                        fl = false;
                        if (languageIndex == 18) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "TypeScript":
                        fl = false;
                        if (languageIndex == 27) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;

                    case "CoffeeScript":
                        fl = false;
                        if (languageIndex == 28) fl = true;
                        if (LanguageCheckBox == true && fl == false || LanguageCheckBox == false && fl == true) return false;
                        break;
                }
            }

            if (MoscowCheckBox != null)
            {
                fl = false;
                if (rec.Town.ContainsCI("москва")) fl = true;
                if (MoscowCheckBox == true && fl == false || MoscowCheckBox == false && fl == true) return false;
            }
            if (KrasnodarCheckBox != null)
            {
                fl = false;
                if (rec.Town.ContainsCI("краснодар")) fl = true;
                if (KrasnodarCheckBox == true && fl == false || KrasnodarCheckBox == false && fl == true) return false;
            }
            if (RostovCheckBox != null)
            {
                fl = false;
                if (rec.Town.ContainsCI("ростов")) fl = true;
                if (RostovCheckBox == true && fl == false || RostovCheckBox == false && fl == true) return false;
            }
            if (SpbCheckBox != null)
            {
                fl = false;
                if (rec.Town.ContainsCI("петербург")) fl = true;
                if (SpbCheckBox == true && fl == false || SpbCheckBox == false && fl == true) return false;
            }

            if (internationCheckBox)
            {
                fl = false;
                foreach (var word in _internationString)
                {
                    if (recall.ContainsCI(word)) fl = true;
                }

                if (!fl) return false;
            }

            if (BigZPCheckBox)
            {
	            return CheckBigZp(rec.Zp);
            }

            return true;
        }

        private static bool CheckBigZp(string zp)
        {
	        string[] numbers;

            if (String.IsNullOrEmpty(zp)) return false;
	        if (zp.ContainsCI("usd") || zp.ContainsCI("eur"))
	        {
		        numbers = Regex.Split(zp.Replace(" ", "").Replace("\u00A0", "").Replace("\u202F", ""), @"\D+");
		        foreach (var number in numbers)
		        {
			        if (int.TryParse(number, out var num))
			        {
				        if (num >= 5000) return true;
			        }
		        }
            }

            //var l = new List<int>();
            //foreach (char c in zp)
            //{
            // l.Add((int)c);
            //}

            numbers = Regex.Split(zp.Replace(" ", "").Replace("\u00A0", "").Replace("\u202F", ""), @"\D+");
	        foreach (var number in numbers)
	        {
		        if (int.TryParse(number, out var num))
		        {
			        if (num >= 350000) return true;
		        }
	        }
            return false;
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
            bool flag, red, green;
            string text;

            string[] redwords = new string[] { "1C", "1С", "Angular", "AngularJS", "D3.js", "es5", "es6", "ExtJS", "JavaScript", "jQuery", "js", "linux", "marionettejs", "MongoDB", "Node.js", "PHP", "powershell", "Python", "React", "sharepoint", "typescript", "ubuntu", "unity", "unity3d", "unix", "Vue.js", "xquery" };
            string[] greenwords = new string[] { "English", "Английский", "Английскому", "Английского", "Английским" };

            ObservableCollection<AnaliseType> MyQ = Model.GetQData();

            foreach (AnaliseType word in MyQ)
            {
                position = flowDocument.ContentStart;
                flag = true;
                while (flag)
                {
	                red = redwords.Contains(word.Name);
	                green = greenwords.Contains(word.Name);
                    flag = BoldTextFromPosition(position, word.Name, red, green);
                }
            }

            text = XamlWriter.Save(flowDocument);
            (e.Rec).Desc = text;
        }

        private bool BoldTextFromPosition(TextPointer position, string word, bool red, bool green)
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
                        start = position.GetPositionAtOffset(indexInRun - 1);
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
                                if (green) selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Aqua);

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