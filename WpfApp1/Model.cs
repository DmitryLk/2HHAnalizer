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




namespace WpfApp1
{
    public interface IModel
    {
        void Load(IList<Record> r);
    }


    class MyModel : IModel
    {
        Excel.Application excelApp;
        Excel.Workbook workBook;
        Excel.Worksheet workSheet;
        Excel.Range range;
        Process[] excelProcsOld;


        private readonly IView View;
        public MyModel(IView View)
        {
            this.View = View;
        }


        List<Record> spisok;

        public void Load(IList<Record> spisok)
        {
            this.spisok = (List<Record>)spisok;

            //PB.Maximum = 345;// ReadHHCountVac();
            //PB.Value = 0;
            workerFunc = LoadFromXLS;
            worker.RunWorkerAsync();
        }

        private void LoadFromXLS()
        {



            int i, j, k;
            excelProcsOld = Process.GetProcessesByName("EXCEL");
            excelApp = new Excel.Application();
            object[,] tmp;
            Record rec;
            double value = 0;


            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(PB.SetValue);

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
                Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });
                Pbprc = value / 350 * 100;
            }



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



