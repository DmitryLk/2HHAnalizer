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
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using HtmlAgilityPack;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Threading;
using System.Globalization;
using System.Windows.Markup;

namespace WpfApp1
{
    public interface ILoader
    {
        Task Load(IList<Record> spisok, CancellationToken token, IModel m);
        event EventHandler<MyEventArgs> Changed;
        event EventHandler<MyEventArgs> StartPause;
        event EventHandler<MyEventArgs> EndPause;

        //void Load(IModel m);
        //void NavigateEv(object sender, RoutedEventArgs e);
    }

    enum XF
    {
        Name = 1,
        Subtract = 4,
        Multiply = 8,
        Divide = 16
    }


    public class XLSLoader : XLSWorker, ILoader//, INotifyPropertyChanged
    {
        public event EventHandler<MyEventArgs> Changed = delegate { };
        public event EventHandler<MyEventArgs> StartPause = delegate { };
        public event EventHandler<MyEventArgs> EndPause = delegate { };

        private readonly SynchronizationContext SC;

        public XLSLoader(SynchronizationContext SC)
        {
            this.SC = SC;
        }

        public async Task Load(IList<Record> Spisok, CancellationToken token, IModel m)
        {
           
            int i, j, k;
            excelProcsOld = Process.GetProcessesByName("EXCEL");
            excelApp = new Excel.Application();
            object[,] tmp;
            Record rec;
            MyEventArgs args = new MyEventArgs();
            //double value = 0;
            DateTime tmpdate = new DateTime(1900, 1, 1);
            //DateTime tmpDate;
            //String tmpString;
            //UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);

     

            range = null;
            string path = Directory.GetCurrentDirectory() + "\\Spisok.xlsx";
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open XLS File";
            theDialog.Filter = "XLS files|*.xlsx";
            theDialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (theDialog.ShowDialog() == true) path = theDialog.FileName;



            workBook = excelApp.Workbooks.Open(path, 0, true, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            workSheet = (Excel.Worksheet)workBook.Worksheets.get_Item(workBook.Sheets.Count);

            range = workSheet.get_Range("A1", Missing.Value);
            range = range.get_End(Excel.XlDirection.xlDown);
            j = range.Row - 1;
            range = workSheet.get_Range("A1", Missing.Value);
            range = range.get_End(Excel.XlDirection.xlToRight);
            k = range.Column;

            range = workSheet.get_Range((Excel.Range)workSheet.Cells[2, 1], (Excel.Range)workSheet.Cells[j + 1, k]);
            tmp = range.Value2;

            //SC.Post(new SendOrPostCallback(o => { Spisok.Clear(); }), 1);
            Spisok.Clear();


            //rec.Clear();
            //public IList<Record> spisok = new List<Record>();

            args.MaxValue = j;
            args.Value = 0;
            args.Value2 = 0;
            Changed?.Invoke(this, args);

            for (i = 1; i <= j; i++)
            {
                rec = new Record();
                rec.Name = tmp[i, (int)XF.Name]?.ToString();
                rec.Zp = tmp[i, 2]?.ToString();
                rec.Comp = tmp[i, 3]?.ToString();
                rec.Town = tmp[i, 4]?.ToString();
                rec.Resp1 = tmp[i, 5]?.ToString();
                rec.Req1 = tmp[i, 6]?.ToString();


                //tmpString = Regex.Replace(tmp[i, 7]?.ToString(), @"\u00A0", " ");
                //if (Regex.Match(tmpString, @"\d+").Length == 1) tmpString = "0" + tmpString;
                //DateTime.TryParseExact(tmpString, "dd MMMMM", null, DateTimeStyles.None, out tmpDate);
                //rec.Dat = tmpDate;
                rec.Dat = tmpdate.AddDays(Convert.ToDouble(tmp[i, 7]) - 2);
                rec.Opt = tmp[i, 8]?.ToString();

                rec.Desc = tmp[i, 9]?.ToString();
                if (rec.Desc[0] != '<')
                    rec.Desc = ConvertToFlowDocumentString(rec.Desc, new String[] { "Требования" });

                //ConvertToFlowDocumentString(tmp[i, 9]?.ToString(), new String[] { "Требования" })

             


                rec.Id = tmp[i, 10]?.ToString();
                rec.link = tmp[i, 11]?.ToString();
                rec.Sharp = Convert.ToBoolean(tmp[i, 12]);
                rec.JavaScript = Convert.ToBoolean(tmp[i, 13]);
                rec.SQL = Convert.ToBoolean(tmp[i, 14]);
                rec._1C = Convert.ToBoolean(tmp[i, 15]);
                rec.Distant = Convert.ToBoolean(tmp[i, 16]);
                rec.Closed = Convert.ToBoolean(tmp[i, 17]);
                rec.BeginingDate = tmpdate.AddDays(Convert.ToDouble(tmp[i, 18]) - 2);
                rec.LastCheckDate = tmpdate.AddDays(Convert.ToDouble(tmp[i, 19]) - 2);

                if (rec.BeginingDate < rec.Dat) rec.DaysLong = (rec.LastCheckDate - rec.BeginingDate).TotalDays; else rec.DaysLong = (rec.LastCheckDate - rec.Dat).TotalDays;

                if (k > 20) rec.Interes = Convert.ToBoolean(tmp[i, 21]); else rec.Interes = false;


                //SC.Post(new SendOrPostCallback(o => { Spisok.Add(rec); }), 1);
                Spisok.Add(rec);

                args.Value = i;
                Changed?.Invoke(this, args);

                await Task.Delay(1);

                //Application.Current.Dispatcher.BeginInvoke(new Action(() => { PB.Value= ++value; }));
                //System.Threading.Thread.Sleep(10);
                //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                //Pbprc = value / 350 * 100;
            }
            QuitExcel();


          
            if (Spisok.GroupBy(p => p.Id).Select(g => new { Name = g.Key, MyCount = g.Count() }).Max(n => n.MyCount)>1)
                throw new InvalidOperationException("2 одинаковые записи");



        }
    }

    public class CommonLoader
    {


        protected String ConvertToFlowDocumentString(String sbDescString, String[] boldWords)
        {
            int indexInRun;
            FlowDocument flowDocument = null;

            if (sbDescString[0] == '<') flowDocument = XamlReader.Parse(sbDescString) as FlowDocument;

            if (flowDocument == null)
            {
                flowDocument = new FlowDocument();
                flowDocument.Blocks.Clear();
                flowDocument.Blocks.Add(new Paragraph(new Run(sbDescString)));
            }

         

            if (boldWords != null)
            {
                TextPointer position = flowDocument.ContentStart;
                while (position != null)
                {
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        string textRun = position.GetTextInRun(LogicalDirection.Forward);

                        // Find the starting index of any substring that matches "word".
                        foreach (String word in boldWords)
                        {
                            indexInRun = textRun.IndexOf(word);
                            if (indexInRun >= 0)
                            {
                                TextPointer start = position.GetPositionAtOffset(indexInRun);
                                TextPointer end = start.GetPositionAtOffset(word.Length);
                                TextRange selection = new TextRange(start, end);
                                selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                            }

                        }
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

           


            sbDescString = XamlWriter.Save(flowDocument);
            return sbDescString;


        }


    }


    public class XLSWorker : CommonLoader
    {
        protected Excel.Application excelApp;
        protected Excel.Workbook workBook;
        protected Excel.Worksheet workSheet;
        protected Excel.Range range;
        protected Process[] excelProcsOld;

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

}
