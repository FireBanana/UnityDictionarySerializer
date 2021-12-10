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
            var currentData = LoadData();

            using (TextWriter stream = new StreamWriter(Application.dataPath + "/SerializedDictionaryData"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));

                if (currentData.HasValue)
                {
                    currentData?.DataList.Add(data);
                    serializer.Serialize(stream, currentData);
                }
                else
                {
                    var newData = new SerializedData();

                    newData.DataList = new List<DictionarySerializedData>();
                    newData.DataList.Add(data);

                    serializer.Serialize(stream, newData);
                }
            }
        }

        public static SerializedData? LoadData()
        {
            if (!File.Exists(Application.dataPath + "/SerializedDictionaryData"))
                return null;

            using (TextReader stream = new StreamReader(Application.dataPath + "/SerializedDictionaryData"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));

                return (SerializedData)serializer.Deserialize(stream);
            }
        }
    }
}