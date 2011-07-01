using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace HDTrailersNETDownloader
{
    class Serializer
    {
        String path;
        Object classType;
        XmlSerializer xmlSerial;
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        public Serializer(String xmlPath, Object toClass)
        {
            try
            {
                path = xmlPath;
                ns.Add("", "");
                classType = toClass;
                xmlSerial = new XmlSerializer(toClass.GetType());
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Unhandled exception in Serializer. Application will close ....");
                Program.log.WriteLine("Exception: " + e.ToString());
            }
        }

        public Object FromFile()
        {
            if (!File.Exists(path)) return null;
            try
            {
                TextReader r = new StreamReader(path);
                Object obj = xmlSerial.Deserialize(r);
                r.Close();
                return obj;
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Unhandled exception in Serializer FromFile. Application will close ....");
                Program.log.WriteLine("Exception: " + e.ToString());
                return null;
            }
        }

        public bool ToFile()
        {
            try
            {
                TextWriter w = new StreamWriter(path);
                xmlSerial.Serialize(w, classType, ns);
                w.Close();
                return true;
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Unhandled exception in Serializer ToFile. Application will close ....");
                Program.log.WriteLine("Exception: " + e.ToString());
                return false;
            }
        }
    }
}
