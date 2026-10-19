using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace digioz.Portal.Utilities.Helpers
{
    public class Serializer
    {
        private static string GetString(MemoryStream psMemStream)
        {
            string lsReturn = string.Empty;

            psMemStream.Position = 0;
            using (StreamReader loReader = new StreamReader(psMemStream))
            {
                lsReturn = loReader.ReadToEnd();
            }
            return lsReturn;
        }

        public static string SerializeObject(Object poObject)
        {
            using MemoryStream loStream = new MemoryStream();
            XmlSerializer loMessageSerialize = new XmlSerializer(poObject.GetType());
            loMessageSerialize.Serialize(loStream, poObject);

            return GetString(loStream);
        }

        public static object DeserializeObject(string psObjectXML, Object poObject)
        {
            XmlSerializer loMessage = new XmlSerializer(poObject.GetType());
            return loMessage.Deserialize(new StringReader(psObjectXML));
        }
    }
}