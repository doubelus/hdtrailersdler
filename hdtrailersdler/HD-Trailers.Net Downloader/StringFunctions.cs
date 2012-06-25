using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using System.Text;

namespace HDTrailersNETDownloader
{
    class StringFunctions
    {
        public static string subStrBetween(String source, String from, String to = null)
        {
            try
            {
                int posS = source.IndexOf(from);
                if (posS == -1) return null;
                int len = source.Length - posS;
                if (to != null)
                {
                    int posE = source.IndexOf(to, posS + from.Length);
                    if (posE != -1) len = posE - posS;
                }
                String result = source.Substring(posS + from.Length, len - from.Length);
                return result;
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Exception in subStrBetween");
                Program.log.WriteLine(e.ToString());
                return null;
            }
        }
        public static string[] splitBetween(String source, String from, String to)
        {
            try
            {
                System.Collections.ArrayList al = new ArrayList();
                int posS = 0;
                while (posS != -1 && posS < source.Length)
                {
                    posS = source.IndexOf(from, posS + 1);
                    if (posS == -1) break;
                    int posE = source.IndexOf(to, posS + from.Length);
                    if (posE == -1) break;
                    int len = posE - posS;
                    al.Add((string)source.Substring(posS + from.Length, len - from.Length));
                }
                return (string[])al.ToArray(typeof(string));
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Exception in subBetween");
                Program.log.WriteLine(e.ToString());
                return new string[0];
            }
        }

        public static int countMach(string type, string fullname)
        {
            string[] typeArr = type.Split(new Char[] { ' ', ',', '(', ')', '-' });
            int match = 0;
            foreach (string subType in typeArr)
                if (subType.Length > 0 && fullname.Contains(subType))
                    match++;
            return match;
        }
    }
}
