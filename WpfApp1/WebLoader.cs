using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Documents;
using System.Windows;


namespace WpfApp1
{

    class WebLoader : CommonLoader, ILoader
    {
        public event EventHandler<MyEventArgs> Changed = delegate { };
        public event EventHandler<MyEventArgs> StartPause = delegate { };
        public event EventHandler<MyEventArgs> EndPause = delegate { };

        private readonly string webs;
        private WebBrowser Wb { get; }
        private readonly SynchronizationContext SC;

        public WebLoader(string webs, WebBrowser wb, SynchronizationContext SC)
        {
            this.webs = webs;
            this.Wb = wb;
            this.SC = SC;
        }

        //private async Task<HtmlDocument> GetDocument2(string link)
        //{
        //    string HTML;
        //    HtmlDocument hDocument;
        //    object doc1;

        //    //MyWaitable wbwait = new MyWaitable();


        //    MyWebBrowserAwaitable PageLoading = new MyWebBrowserAwaitable(wb);
        //    wb.Navigate(new Uri(link));
        //    doc1 = await PageLoading;



        //    //mshtml.IHTMLDocument2 doc2 = wb.Document as mshtml.IHTMLDocument2;
        //    mshtml.IHTMLDocument2 doc2 = doc1 as mshtml.IHTMLDocument2;
        //    HTML = doc2.body.outerHTML;

        //    hDocument = new HtmlDocument();
        //    hDocument.LoadHtml(HTML);
        //    return hDocument;

        //}

        //https://krasnodar.hh.ru/search/vacancy?area=1&clusters=true&enable_snippets=true&specialization=1.221&page=1
        //https://krasnodar.hh.ru/search/vacancy?area=1&clusters=true&enable_snippets=true&specialization=1.221&page-1

        private async Task<HtmlDocument> GetDocument(string link)
        {
            string HTML;
            HtmlDocument hDocument = null;
            object doc1 = null;

            doc1 = await Wb.NavigateAsync(link, SC);

            if (doc1 is mshtml.IHTMLDocument2)
            {
                mshtml.IHTMLDocument2 doc2 = doc1 as mshtml.IHTMLDocument2;
                HTML = doc2.body.outerHTML;
                HTML = HTML.Replace("&nbsp;", " ");
                //HTML.Replace("&nbsp;", string.Empty);
                //HTML = HtmlEntity.DeEntitize(HTML);
                hDocument = new HtmlDocument();
                hDocument.LoadHtml(HTML);
            }

            return hDocument;

            //await Application.Current.Dispatcher.BeginInvoke(new Action(async () => { await wb.NavigateAsync(link); }));
            //await Application.Current.Dispatcher.BeginInvoke(new Action(() => { doc1 = wb.Document; }));
            //await wb.NavigateAsync(link, SC);
        }


        public async Task Load(IList<Record> Spisok, CancellationToken token, IModel m)
        {
            HtmlNodeCollection nodes;
            HtmlNode root;
            HtmlDocument document2, hDocument;
            Record rec, rec2;
            //IEnumerable<Record> rec3;
            string text;
            MyEventArgs args = new MyEventArgs();
            Random rnd = new Random();
            StringBuilder SBDesc = new StringBuilder();
            string tmpString;
            char searchChar1, searchChar2;

            //await GetDocument(webs);

            //text = hDocument.DocumentNode.SelectSingleNode("//div[@data-qa='vacancies-total-found']")?.InnerText ?? "";
            //web = new HtmlWeb();
            //document = web.Load(webs);



            args.Value = 0;
            args.Value2 = 0;
            Changed?.Invoke(this, args);
            await Task.Delay(100);
            Spisok.ForEach<Record>(s => s.Closed = true);

            hDocument = await GetDocument(webs);

            searchChar1 = '/'; searchChar2 = '-';
            text = hDocument.DocumentNode.SelectSingleNode("//div[@data-qa='vacancies-total-found']")?.InnerText ?? "";
            text = Regex.Match(text, @"\d+").Value;

            if (!Int32.TryParse(text, out int _p))
            {
                searchChar1 = '&'; searchChar2 = '=';

                text = hDocument.DocumentNode.SelectSingleNode("//h1[@class='header' and @data-qa='page-title']")?.InnerText ?? "";
                text = Regex.Replace(text, @"<[^>]+>|&nbsp;", "").Trim();
                text = Regex.Replace(text, @" ", "").Trim();
                text = Regex.Match(text, @"\d+").Value;
                if (!Int32.TryParse(text, out _p)) throw new ArgumentException("Vacancy count error");

            }





            args.MaxValue = _p;

            _p = 0;
            nodes = hDocument.DocumentNode.SelectNodes("//div[@class='vacancy-serp-item ' or @class='vacancy-serp-item  vacancy-serp-item_premium']");
            while (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    text = node.SelectSingleNode(".//script[@data-name='HH/VacancyResponsePopup/VacancyResponsePopup']")?.Attributes["data-params"]?.Value ?? "0";
                    text = Regex.Match(text, @"(?<=vacancyId....)\d+").Value;

                    // теперь надо узнать есть ли уже такой Id в Spisok
                    rec2 = null;
                    //rec3 = null;
                    rec2 = (Spisok as List<Record>).Find(c => c.Id == text);
                    //rec3 = Spisok.Where(c => c.Id == text);

                    //if (rec3.Count<Record>() == 0)
                    if (rec2 == null)
                    {
                        // Новая запись
                        rec = new Record
                        {
                            Name = node.SelectSingleNode(".//div[@class='resume-search-item__name']")?.InnerText ?? "",
                            link = node.SelectSingleNode(".//a[@class='bloko-link HH-LinkModifier']")?.Attributes["href"].Value,
                            Zp = node.SelectSingleNode(".//div[@class='vacancy-serp-item__compensation']")?.InnerText ?? "",
                            Comp = node.SelectSingleNode(".//div[@class='vacancy-serp-item__meta-info']")?.InnerText ?? "",
                            Town = node.SelectSingleNode(".//span[@class='vacancy-serp-item__meta-info']")?.InnerText ?? "",
                            Resp1 = node.SelectSingleNode(".//div[@data-qa='vacancy-serp__vacancy_snippet_responsibility']")?.InnerText ?? "",
                            Req1 = node.SelectSingleNode(".//div[@data-qa='vacancy-serp__vacancy_snippet_requirement']")?.InnerText ?? ""
                        };


                        tmpString = node.SelectSingleNode(".//span[@class='vacancy-serp-item__publication-date']")?.InnerText ?? "";
                        if (Regex.Match(tmpString, @"\d+").Length == 1) tmpString = "0" + tmpString;
                        if (DateTime.TryParseExact(Regex.Replace(tmpString, @"\u00A0", " "), "dd MMMMM", null, DateTimeStyles.None, out DateTime tmpDate))
                            rec.Dat = tmpDate; 
                        else
                            rec.Dat = DateTime.Now;

                        rec.Id = text;
                        rec.BeginingDate = DateTime.Now;
                        rec.LastCheckDate = DateTime.Now;

                        if (rec.Dat > DateTime.Now)
                            rec.Dat = rec.Dat.AddYears(-1);



                        rec.DaysLong = (rec.LastCheckDate - rec.Dat).TotalDays;


                        // Случайная пауза.
                        StartPause?.Invoke(this, args);
                        await Task.Delay(rnd.Next(6000, 15000));
                        EndPause?.Invoke(this, args);

                        document2 = await GetDocument(rec.link);
                        if (document2 != null)
                        {
                            rec.Opt = document2.DocumentNode.SelectSingleNode("(//div[@class='bloko-gap bloko-gap_bottom'])[3]")?.InnerText ?? "";
                            root = document2.DocumentNode.SelectSingleNode("//div[@class='g-user-content' or @data-qa='vacancy-description']");
                            SBDesc.Clear();
                            foreach (HtmlNode node2 in root.DescendantsAndSelf())
                            {
                                if (!node2.HasChildNodes)
                                {
                                    text = node2.InnerText;
                                    if (!string.IsNullOrWhiteSpace(text))
                                        SBDesc.AppendLine(text);
                                }
                            }
                            rec.Desc = ConvertToFlowDocumentString(SBDesc.ToString(), new String[] { "Требования" });
                        }



                        //rec.Desc = XamlWriter.Save(SBDesc.ToString());
                        //rec.Desc = SBDesc.ToString();
                        // Преобразовать SBDesc.ToString() во FlowDocument
                        //yap.ForEach<AnaliseType>(p => p.count = spisok.Count(t => t.AllInfo().СontainsCI(p.Name) || t.AllInfo().СontainsCI(p.NameRus())));
                        rec.Sharp = rec.AllInfo().ContainsCI("C#") || rec.AllInfo().ContainsCI("С#") || rec.AllInfo().ContainsCI(".NET");
                        rec.JavaScript = rec.AllInfo().ContainsCI("JavaScript");
                        rec.SQL = rec.AllInfo().ContainsCI("SQL");
                        rec._1C = rec.AllInfo().ContainsCI("1C") || rec.AllInfo().ContainsCI("1С");
                        rec.Distant = rec.AllInfo().ContainsCI("удал");
                        rec.Closed = false;

                        //SC.Post(new SendOrPostCallback(o => { Spisok.Add(rec); }), 1);
                        Spisok.Add(rec);
                        args.Value2++;
                    }
                    else
                    {
                        // Уже есть
                        // rec2 = rec3.First<Record>();
                        rec2.LastCheckDate = DateTime.Now;
                        rec2.Closed = false;
                        if (rec2.BeginingDate < rec2.Dat) rec2.DaysLong = (rec2.LastCheckDate - rec2.BeginingDate).TotalDays; else rec2.DaysLong = (rec2.LastCheckDate - rec2.Dat).TotalDays;
                    }

                    //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                    //Pbprc = value / 350 * 100;

                    args.Value++;
                    Changed?.Invoke(this, args);
                    token.ThrowIfCancellationRequested();

                }

                // Случайная пауза.
                StartPause?.Invoke(this, args);
                await Task.Delay(rnd.Next(6000, 15000));
                EndPause?.Invoke(this, args);


                hDocument = await GetDocument(webs + searchChar1 + "page" + searchChar2 + ++_p);
                nodes = hDocument.DocumentNode.SelectNodes("//div[@class='vacancy-serp-item ' or @class='vacancy-serp-item  vacancy-serp-item_premium']");
            }

            args.Value = 0;
            args.Value2 = 0;
            Changed?.Invoke(this, args);
            await Task.Delay(100);


        }


    


        /*
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
        */
        /*
        public void LoadAsync(List<Record> Spisok)
        {

            //UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);

            int i = 0, p = 0;
            HtmlWeb web;
            HtmlNodeCollection nodes;
            HtmlNode root;
            HtmlDocument document, document2;
            Record rec;
            string text;

            //double value = 0;
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


                    //yap.ForEach<AnaliseType>(p => p.count = spisok.Count(t => t.AllInfo().СontainsCI(p.Name) || t.AllInfo().СontainsCI(p.NameRus())));

                    rec.Sharp = rec.AllInfo().ContainsCI("C#") || rec.AllInfo().ContainsCI("С#") || rec.AllInfo().ContainsCI(".NET");
                    rec.JavaScript = rec.AllInfo().ContainsCI("JavaScript");
                    rec.Distant = rec.AllInfo().ContainsCI("удал");

                    Spisok.Add(rec);


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

                    //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                    //Pbprc = value / 350 * 100;
                }

                document = web.Load(webs + "/page-" + ++p);
                nodes = document.DocumentNode.SelectNodes("//div[@class='vacancy-serp-item ']");

            }
        }
        */
        /*
         *         //web = new HtmlWeb();
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


        //public class AsyncWebBrowser
//{
//    protected WebBrowser m_WebBrowser;

//    private ManualResetEvent m_MRE = new ManualResetEvent(false);

//    public void SetBrowser(WebBrowser browser)
//    {
//        this.m_WebBrowser = browser;
//        browser.LoadCompleted += new LoadCompletedEventHandler(WebBrowser_LoadCompleted);
//    }

//    public Task NavigateAsync(string url)
//    {
//        Navigate(url);

//        return Task.Factory.StartNew((Action)(() => {
//            m_MRE.WaitOne();
//            m_MRE.Reset();
//        }));
//    }

//    public void Navigate(string url)
//    {
//        m_WebBrowser.Navigate(new Uri(url));
//    }

//    void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
//    {
//        m_MRE.Set();
//    }
//}




//public class MyWaitable
//{
//    public WebBrowserAwaiter GetAwaiter() => new WebBrowserAwaiter();
//}

//public class WebBrowserAwaiter : INotifyCompletion
//{
//    //object obj;
//    //public WebBrowserAwaiter(object obj)
//    //{
//    //    this.obj = obj;
//    //}

//    public WebBrowserAwaiter()
//    {
//        this.IsCompleted = false;
//    }


//    public void OnCompleted(Action continuation)
//    {}

//    public bool IsCompleted
//    {get;}

//    public void GetResult()
//    {}
//}

//public static class AwaiterExtensions
//{
//    public static WebBrowserAwaiter GetAwaiter(this WebBrowser obj)
//    {
//        return new WebBrowserAwaiter(obj);
//    }
//}

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
            /*
             *         private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
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

                    public void Navigate()
        {
            View.PBmax = 345;// ReadHHCountVac();
        /*
            

        }


             */





    }




}
