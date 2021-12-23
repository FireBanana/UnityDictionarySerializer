using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace DSerializer
{
    public static class SerializerInterface
    {
        public struct SerializableDictionary
        {
            public string                                   DictionaryName;
            public List<object>                             Keys;
            public List<object>                             Values;
        }

        public struct SerializedScript
        {
            public int                                      ScriptInstanceId;
            public string                                   SceneName;

            public List<SerializableDictionary>             Dictionaries;
        }

        public struct SerializedData
        {
            public List<SerializedScript>           DataList;
        }

        public static void SaveData(SerializedData data)
        {
            using (TextWriter stream = new StreamWriter(Application.dataPath + "/SerializedDictionaryData"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));

                serializer.Serialize(stream, data);
            }
        }

        public static SerializedData LoadData()
        {
            if (!File.Exists(Application.dataPath + "/SerializedDictionaryData"))
                return default(SerializedData);

            using (TextReader stream = new StreamReader(Application.dataPath + "/SerializedDictionaryData"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));

                return (SerializedData)serializer.Deserialize(stream);
            }
        }
    }
}