using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VectorStinger.Foundation.Utilities.CrossUtil
{
    public static class Serializer
    {

        static Dictionary<string, XmlSerializer> poolSerializer = new Dictionary<string, XmlSerializer>();
        static Dictionary<string, XmlSerializer> poolDeserializer = new Dictionary<string, XmlSerializer>();
        /// <summary>
        /// Serializa el objeto a XML
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializarToXml(this object obj)
        {
            string resultXml = string.Empty;
            if (obj == null)
            {
                return resultXml;
            }

            XmlAttributeOverrides xOver = new XmlAttributeOverrides();
            XmlAttributes attrs = new XmlAttributes();

            using (StringWriter strWriter = new StringWriter())
            {
                List<Type> lTypesCollection = new List<Type>();
                foreach (var item in obj.GetType().GetProperties())
                {
                    if (item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        //lTypesCollection.Add(item.PropertyType.GetGenericArguments()[0]);
                        attrs = new XmlAttributes();
                        attrs.XmlIgnore = true;
                        xOver.Add(obj.GetType(), item.Name, attrs);
                    }

                    if (item.PropertyType.IsClass && !item.PropertyType.IsGenericType)
                    {
                        lTypesCollection.Add(item.PropertyType);
                    }
                }
                if (!poolSerializer.ContainsKey(obj.GetType().FullName))
                {
                    XmlSerializer serializer = new XmlSerializer(obj.GetType(), xOver, lTypesCollection.ToArray(), null, null);
                    poolSerializer.Add(obj.GetType().FullName, serializer);
                }
                poolSerializer[obj.GetType().FullName].Serialize(strWriter, obj);

                /*XmlSerializer serializer = new XmlSerializer(obj.GetType(), xOver, lTypesCollection.ToArray(), null, null);
                serializer.Serialize(strWriter, obj);*/
                resultXml = strWriter.ToString();
                strWriter.Close();
                GC.Collect();
            }

            return resultXml;
        }

        /// <summary>
        /// Deserializa un objeto y lo convierte a un tipo generics
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlSerializado"></param>
        /// <returns></returns>
        public static T DeserializeFromXml<T>(this string xmlSerializado)
        {
            try
            {
                if (!poolDeserializer.ContainsKey(typeof(T).FullName))
                {
                    XmlSerializer xmlSerz = new XmlSerializer(typeof(T));
                    poolDeserializer.Add(typeof(T).FullName, xmlSerz);
                }
                using (StringReader strReader = new StringReader(xmlSerializado))
                {
                    object obj = poolDeserializer[typeof(T).FullName].Deserialize(strReader);
                    return (T)obj;
                }

            }
            catch { return default; }
        }
    }
}
