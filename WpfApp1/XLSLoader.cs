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



namespace WpfApp1
{
    public interface ILoader
    {
        Task Load(ObservableCollection<Record> spisok, CancellationToken token, IModel m);
        event EventHandler<MyEventArgs> Changed;
        //void Load(IModel m);
        //void NavigateEv(object sender, RoutedEventArgs e);
    }



    public class XLSLoader : XLSWorker, ILoader//, INotifyPropertyChanged
    {
        public event EventHandler<MyEventArgs> Changed = delegate { };
        private readonly SynchronizationContext SC;

        public XLSLoader(SynchronizationContext SC)
        {
            this.SC = SC;
        }

        public async Task Load(ObservableCollection<Record> Spisok, CancellationToken token, IModel m)
        {
           
            int i, j, k;
            excelProcsOld = Process.GetProcessesByName("EXCEL");
            excelApp = new Excel.Application();
            object[,] tmp;
            Record rec;
            MyEventArgs args = new MyEventArgs();
            //double value = 0;
            DateTime tmpdate = new DateTime(1900, 1, 1);


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

            SC.Post(new SendOrPostCallback(o => { Spisok.Clear(); }), 1);
            

            //rec.Clear();
            //public IObservableCollection<Record> spisok = new ObservableCollection<Record>();

            args.MaxValue = j;
            args.Value = 0;
            args.Value2 = 0;
            Changed?.Invoke(this, args);

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
                rec.Id = tmp[i, 10]?.ToString();
                rec.link = tmp[i, 11]?.ToString();
                rec.Sharp = Convert.ToBoolean(tmp[i, 12]);
                rec.JavaScript = Convert.ToBoolean(tmp[i, 13]);
                rec.Distant = Convert.ToBoolean(tmp[i, 14]);
                rec.Closed = Convert.ToBoolean(tmp[i, 15]);
                rec.BeginingDate = tmpdate.AddDays(Convert.ToDouble(tmp[i, 16]) - 2);
                rec.LastCheckDate = tmpdate.AddDays(Convert.ToDouble(tmp[i, 17]) - 2);

                SC.Post(new SendOrPostCallback(o => { Spisok.Add(rec); }),1);
              






                args.Value = i;
                Changed?.Invoke(this, args);

                await Task.Delay(1);


                //Application.Current.Dispatcher.BeginInvoke(new Action(() => { PB.Value= ++value; }));
                //System.Threading.Thread.Sleep(10);
                //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                //Pbprc = value / 350 * 100;
            }
            QuitExcel();
        }
    }


    public class XLSWorker
    {
        protected Excel.Application excelApp;
        protected Excel.Workbook workBook;
        protected Excel.Worksheet workSheet;
        protected Excel.Range range;
        protected Process[] excelProcsOld;

        public virtual void QuitExcel()
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
