using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace digioz.Portal.Web.Helpers
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
            string lsResponse = "";

            try
            {
                MemoryStream loStream = new MemoryStream();
                XmlSerializer loMessageSerialize = new XmlSerializer(poObject.GetType());
                loMessageSerialize.Serialize(loStream, poObject);

                lsResponse = GetString(loStream);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lsResponse;
        }

        public static object DeserializeObject(string psObjectXML, Object poObject)
        {
            string loResponse = string.Empty;
            object loObject = new object();

            try
            {
                XmlSerializer loMessage = new XmlSerializer(poObject.GetType());
                loObject = loMessage.Deserialize(new StringReader(psObjectXML));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return loObject;
        }
    }
}