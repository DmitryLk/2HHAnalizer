using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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
    public interface ISaver
    {
        Task Save(IList<Record> spisok, CancellationToken token);
        event EventHandler<MyEventArgs> Changed;
    }

    class XLSSaver: XLSWorker, ISaver
    {
        public event EventHandler<MyEventArgs> Changed = delegate { };
        public XLSSaver() { }



        public async Task Save(IList<Record> Spisok, CancellationToken token)
        {
            int i;
            string path, text;
            MyEventArgs args = new MyEventArgs();
            DateTime tmpdate = new DateTime(1900, 1, 1);
            string worksheetName, worksheetName2;
            int num;
            Excel.Range range1, range2;

            excelProcsOld = Process.GetProcessesByName("EXCEL");
            excelApp = new Excel.Application();


            var data = new object[Spisok.Count+1, 21];
            


            path = Directory.GetCurrentDirectory() + "\\Spisok.xlsx";

            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open XLS File";
            theDialog.Filter = "XLS files|*.xlsx";
            theDialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (theDialog.ShowDialog() == true) path = theDialog.FileName;



            if (System.IO.File.Exists(path))
            {
                workBook = excelApp.Workbooks.Open(path, 0, false, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
                workSheet = (Excel.Worksheet)workBook.Worksheets.get_Item(workBook.Sheets.Count);
                worksheetName = workSheet.Name;
                worksheetName2 = DateTime.Now.ToString("ddMMyy");

                if (worksheetName.Contains("_"))
                {
                    text = Regex.Match(worksheetName, @"^.*?(?=_)").Value;
                    if (!Int32.TryParse(Regex.Match(worksheetName, @"(?<=_)\d+").Value, out num)) num = 0;
                    if (text == worksheetName2) worksheetName2 += "_" + ++num;
                }
                else
                    if (worksheetName == worksheetName2) worksheetName2 += "_1";

                workSheet = workBook.Worksheets.Add(After: workBook.Sheets[workBook.Sheets.Count]);



                workSheet.Name = worksheetName2;
            }
            else
            {
                workBook = excelApp.Workbooks.Add();
                workSheet = (Excel.Worksheet)workBook.Worksheets.get_Item(1);
                workSheet.Name = DateTime.Now.ToString("ddMMyy");
            }


            range1 = (Excel.Range)workSheet.Cells[1, 7];
            range2 = (Excel.Range)workSheet.Cells[Spisok.Count + 1, 7];
            range = workSheet.Range[range1, range2];
            range.NumberFormat = "DD/MM/YYYY";


            range1 = (Excel.Range)workSheet.Cells[1, 18];
            range2 = (Excel.Range)workSheet.Cells[Spisok.Count + 1, 19];
            range = workSheet.Range[range1, range2];
            range.NumberFormat = "DD/MM/YYYY";


            data[0, 0] = "Вакансия";
            data[0, 1] = "ЗП";
            data[0, 2] = "Компания";
            data[0, 3] = "Город";
            data[0, 4] = "ЧтоДелать";
            data[0, 5] = "Требования1";
            data[0, 6] = "Дата вак";
            data[0, 7] = "Опыт";
            data[0, 8] = "Описание";
            data[0, 9] = "Id";
            data[0, 10] = "link";
            data[0, 11] = "C#";
            data[0, 12] = "JavaScript";
            data[0, 13] = "SQL";
            data[0, 14] = "1C";
            data[0, 15] = "удаленно";
            data[0, 16] = "Closed";
            data[0, 17] = "BeginigDate";
            data[0, 18] = "LastCheckDate";
            data[0, 19] = "Период";
            data[0, 19] = "Период";
            data[0, 20] = "Интерес";



            args.MaxValue = Spisok.Count;
            args.Value2 = 0;
            i = 1;

            foreach (Record rec in Spisok)
            {
                data[i, 0] = rec.Name;
                data[i, 1] = rec.Zp;
                data[i, 2] = rec.Comp;
                data[i, 3] = rec.Town;
                data[i, 4] = rec.Resp1;
                data[i, 5] = rec.Req1;
                data[i, 6] = rec.Dat;
                data[i, 7] = rec.Opt;
                data[i, 8] = rec.Desc;
                data[i, 9] = rec.Id;
                data[i, 10] = rec.link;
                data[i, 11] = rec.Sharp;
                data[i, 12] = rec.JavaScript;
                data[i, 13] = rec.SQL;
                data[i, 14] = rec._1C;
                data[i, 15] = rec.Distant;
                data[i, 16] = rec.Closed;
                data[i, 17] = rec.BeginingDate;
                data[i, 18] = rec.LastCheckDate;
                data[i, 19] = (rec.LastCheckDate - rec.BeginingDate).TotalDays;

                if (rec.BeginingDate < rec.Dat) data[i, 19] = (rec.LastCheckDate - rec.BeginingDate).TotalDays; else data[i, 19] = (rec.LastCheckDate - rec.Dat).TotalDays;

                data[i, 20] = rec.Interes;


                //workSheet.Rows[i].RowHeight = 15;
                await Task.Delay(1);
                args.Value = i;
                Changed?.Invoke(this, args);

                i++;
            }

            //await Task.Delay(1);


            range1 = (Excel.Range)workSheet.Cells[1, 1];
            range2 = (Excel.Range)workSheet.Cells[Spisok.Count + 1, 21];
            range = workSheet.Range[range1, range2];

            range.Value2 = data;
            range.ColumnWidth = 30;
            range.RowHeight = 15;


            //range1 = (Excel.Range)workSheet.Cells[1, 16];
            //range2 = (Excel.Range)workSheet.Cells[Spisok.Count + 1, 17];
            //range = workSheet.Range[range1, range2];
            //range.NumberFormat = "DD/MM/YYYY";


            excelApp.Application.ActiveWorkbook.SaveAs(path, Type.Missing,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            QuitExcel();
        }
    }
}
