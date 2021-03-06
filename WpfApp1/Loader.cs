﻿using System;
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


namespace WpfApp1
{
    public interface IxlsWorker
    {
        void LoadAsync(List<Record> spisok);
        event EventHandler<MyEventArgs> Changed;
        //void Load(IModel m);
        //void NavigateEv(object sender, RoutedEventArgs e);
    }

   

    public class XLSLoader : IxlsWorker//, INotifyPropertyChanged
    {

        Excel.Application excelApp;
        Excel.Workbook workBook;
        Excel.Worksheet workSheet;
        Excel.Range range;
        Process[] excelProcsOld;
        List<Record> spisok;

        public XLSLoader() {}

        public event EventHandler<MyEventArgs> Changed = delegate { };
        

        public void LoadAsync(List<Record> spisok)
        {
            this.spisok = spisok;
            int i, j, k;
            excelProcsOld = Process.GetProcessesByName("EXCEL");
            excelApp = new Excel.Application();
            object[,] tmp;
            Record rec;
            MyEventArgs args = new MyEventArgs();
            //double value = 0;


            //UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);

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

            args.MaxValue = j;

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


                args.Value = i;
                Changed?.Invoke(this, args);




                //Application.Current.Dispatcher.BeginInvoke(new Action(() => { PB.Value= ++value; }));
                //System.Threading.Thread.Sleep(10);
                //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                //Pbprc = value / 350 * 100;
            }



            QuitExcel();
        }



        public void SaveToXLS()
        {
            excelProcsOld = Process.GetProcessesByName("EXCEL");
            //UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);
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
            //double value = 0;

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
                //Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
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

}
