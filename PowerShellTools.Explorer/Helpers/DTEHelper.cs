﻿using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using EnvDTE80;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Explorer
{
    internal static class DTEHelper
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        internal static DTE2 GetDTE()
        {
            string progId = @"^!VisualStudio\.DTE\.\d{2}\.\d\:" + System.Diagnostics.Process.GetCurrentProcess().Id + "$";
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                IMoniker[] moniker = new IMoniker[1];
                IntPtr numberFetched = IntPtr.Zero;
                while (enumMonikers.Next(1, moniker, numberFetched) == 0)
                {
                    IMoniker runningObjectMoniker = moniker[0];

                    string name = null;

                    try
                    {
                        if (runningObjectMoniker != null)
                        {
                            runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    if (!string.IsNullOrEmpty(name) && Regex.IsMatch(name, progId))
                    {
                        Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            return (DTE2)runningObject;
        }

        internal static void OpenUrlInVSHost(string url)
        {
            DTE2 dte = GetDTE();

            if (dte != null)
            {
                dte.ItemOperations.Navigate(url, vsNavigateOptions.vsNavigateOptionsDefault);
            }
        }

        internal static Window GetMainWindow()
        {
            DTE2 dte = GetDTE();
            return dte.MainWindow;
        }
    }
}
