using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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



    class Controller
    {
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
