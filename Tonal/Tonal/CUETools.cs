using System;
using CUETools.CTDB;
using CUETools.Processor;
using System.Runtime.InteropServices;
using System.IO;

namespace Tonal
{
    public class CUETools
    {
        private delegate void OnProgress(IntPtr context, Step step, double progress);
        private delegate void OnEnd(IntPtr context, bool exist, int confidence = 0, int offset = 0, string binPath = null);
        private delegate void OnError(IntPtr context, string error);

        private OnProgress _onProgress;
        private OnEnd _onEnd;
        private OnError _onError;
        private IntPtr _context;

        public void Repair(string cueFile, 
                           IntPtr onProgress, 
                           IntPtr onEnd, 
                           IntPtr onError, 
                           IntPtr context)
        {
            _onProgress = (OnProgress)Marshal.GetDelegateForFunctionPointer(onProgress, typeof(OnProgress));
            _onEnd = (OnEnd)Marshal.GetDelegateForFunctionPointer(onEnd, typeof(OnEnd));
            _onError = (OnError)Marshal.GetDelegateForFunctionPointer(onError, typeof(OnError));
            _context = context;

            CUEToolsProfile _profile = new CUEToolsProfile(null);
            CUESheet cueSheet = new CUESheet(_profile._config);
            cueSheet.CUEToolsProgress += new EventHandler<CUEToolsProgressEventArgs>(onCUEToolsProgress);
            cueSheet.CUEToolsSelection += new EventHandler<CUEToolsSelectionEventArgs>(onCUEToolsSelection);
            cueSheet.Open(cueFile);

            string outputPath = Path.GetDirectoryName(cueFile) + "/" + System.Guid.NewGuid() + ".wav";
            cueSheet.GenerateFilenames(AudioEncoderType.Lossless, "wav", outputPath);

            CUEToolsScript script = new CUEToolsScript("repair", new CUEAction[] { });
            string result = cueSheet.ExecuteScript(script);

            if (result.StartsWith("disk not present in database", StringComparison.CurrentCulture))
            {
                _onEnd(context, false);
            }
            else if (result.StartsWith("verified OK", StringComparison.CurrentCulture) ||
                     result.StartsWith("done", StringComparison.CurrentCulture))
            {
                string binPath = null;
                if (result.StartsWith("done", StringComparison.CurrentCulture))
                    binPath = outputPath;
                _onEnd(context, true, cueSheet.CTDB.SelectedEntry.conf, cueSheet.CTDB.SelectedEntry.offset, binPath);
            }
            else if (result.StartsWith("could not be verified", StringComparison.CurrentCulture))
            {
                _onEnd(context, true);
            }
            else if (result.StartsWith("database access error", StringComparison.CurrentCulture))
            {
                _onError(context, result);
            }

            cueSheet.CheckStop();
            cueSheet.Close();
        }

        private void onCUEToolsProgress(object sender, CUEToolsProgressEventArgs e)
        {
            string status = e.status;

            if (status.StartsWith("http://db.cuetools.net", StringComparison.CurrentCulture))
            {
                _onProgress(_context, Step.ContactDB, e.percent);
            }
            else if (status.StartsWith("Verifying", StringComparison.CurrentCulture))
            {
                _onProgress(_context, Step.Verify, e.percent);
            }
            else if (status.StartsWith("http://p.cuetools.net", StringComparison.CurrentCulture))
            {
                _onProgress(_context, Step.RepairAnalyze, e.percent);
            }
            else if (status.StartsWith("Writing", StringComparison.CurrentCulture))
            {
                _onProgress(_context, Step.Repair, e.percent);
            }
        }

        private void onCUEToolsSelection(object sender, CUEToolsSelectionEventArgs e)
        {
            int maxConfidence = 0;
            int index = -1;

            for (int i = 0; i < e.choices.Length; i++)
            {
                CUEToolsSourceFile sourceFile = (CUEToolsSourceFile)e.choices[i];
                DBEntry entry = (DBEntry)sourceFile.data;
                if (entry.conf > maxConfidence)
                {
                    index = i;
                    maxConfidence = entry.conf;
                }
            }
            e.selection = index;
        }
    }
}