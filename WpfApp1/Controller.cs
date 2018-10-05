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




    class MyController : INotifyPropertyChanged
    {
        private readonly IMyWindow form;
        public MyController(IMyWindow form)
        {
            this.form = form;
        }

        string answer, webs;
       


        public IList<Record> spisok = new List<Record>();

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

    

        //private ILoader loader;

        public void Load(ILoader loader)
        {
            //loader = loader1;
            loader.Load(spisok);
        }

        //public string SourceIp { get; set; }
        //public Listen(string sourceIp)
        //{
        //    SourceIp = sourceIp;
        //}

        public void Navigate()
        {
            form.PBmax = 345;// ReadHHCountVac();
        
            webs = form.WebString;
            form.wb.Navigate(webs);
            //workerFunc = ReadHH_WB;
            //worker.RunWorkerAsync();
            TextBox2.Text = "";


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
                    Pbprc = value / 350 * 100;
                }

                document = web.Load(webs + "/page-" + ++p);
                nodes = document.DocumentNode.SelectNodes("//div[@class='vacancy-serp-item ']");

            }
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


    }
}
