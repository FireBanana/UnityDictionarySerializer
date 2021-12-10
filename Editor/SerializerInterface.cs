using System;
using System.Collections.Generic;
using System.IO;
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

        public struct DictionarySerializedData
        {
            public int                                      ScriptInstanceId;
            public string                                   SceneName;

            public List<SerializableDictionary>             Dictionaries;
        }

        public struct SerializedData
        {
            public List<DictionarySerializedData>           DataList;
        }

        public static void SaveData(DictionarySerializedData data)
        {
            using (TextWriter stream = new StreamWriter(Application.dataPath + "/SerializedDictionaryData"))
            {
                var currentData          = LoadData();
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));

                currentData.DataList.Add(data);

                serializer.Serialize(stream, data);
            }
        }

        public static SerializedData LoadData()
        {
            using (StreamReader stream = new StreamReader(Application.dataPath + "/SerializedDictionaryData"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));

                return (SerializedData)serializer.Deserialize(stream.BaseStream);
            }
        }
    }
}