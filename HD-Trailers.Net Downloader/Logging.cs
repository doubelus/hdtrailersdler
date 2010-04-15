using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HDTrailersNETDownloader
{
    class Logging
    {
        private bool verboseLogging;
        private bool physicalLog;
        private FileStream logFS;
        private StreamWriter sw;

        public Logging()
        {
            verboseLogging = false;
            physicalLog = false;
        }

        public void Init(bool verbose, bool physLog) 
        {
            verboseLogging = verbose;
            physicalLog = physLog;
            if (physicalLog)
            {   
                if (!File.Exists("HD-Trailers.NET Downloader.log"))
                    logFS = new FileStream("HD-Trailers.NET Downloader.log",FileMode.Create);
                else
                    logFS = new FileStream("HD-Trailers.NET Downloader.log",FileMode.Append);

                sw = new StreamWriter(logFS);                
            }
        }

        public void WriteLine(string text)
        {
            if ((text != null) && (text.Length > 0))
            {
                string msg = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + text;
                Console.WriteLine(msg);
                PhysicalLogWriteLine(msg);
            }
            else
            {
                Console.WriteLine();
                PhysicalLogWriteLine(null);
            }
        }

        public void ConsoleWrite(string text)
        {
            Console.Write(text);
        }

        public void VerboseWrite(string text)
        {
            if (this.verboseLogging)
                this.WriteLine(text);
        }

        /// <summary>
        /// write to physical log if required and flush log file
        /// </summary>
        /// <param name="text">text to display, or null if only linefeed to output</param>
        private void PhysicalLogWriteLine(string text)
        {
            if ((!physicalLog) || (sw == null))
                return;

            if (text == null)
                sw.WriteLine();
            else
                sw.WriteLine(text);

            sw.Flush();
        }

    }
}
