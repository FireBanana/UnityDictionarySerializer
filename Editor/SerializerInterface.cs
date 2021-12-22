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

        public static void SaveData(SerializedScript data)
        {
            var currentData = LoadData();

            using (TextWriter stream = new StreamWriter(Application.dataPath + "/SerializedDictionaryData"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));

                if (currentData.HasValue)
                {
                    var script = currentData?.DataList.First(x => x.ScriptInstanceId == data.ScriptInstanceId);

                    if(script != null)
                    {
                        var index = currentData.Value.DataList.IndexOf(script.Value);
                        currentData.Value.DataList[index] = data;
                    }
                    else
                        currentData.Value.DataList.Add(data);             

                    serializer.Serialize(stream, currentData);
                }
                else
                {
                    var newData = new SerializedData();

                    newData.DataList = new List<SerializedScript>();
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