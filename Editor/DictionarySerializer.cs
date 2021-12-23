using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

using static DSerializer.SerializerInterface;

namespace DSerializer
{
    public class DictionarySerializer : EditorWindow
    {
        private VisualElement _uxmlTree;

        [MenuItem("Window/DictionarySerializer")]
        public static void ShowExample()
        {
            DictionarySerializer wnd = GetWindow<DictionarySerializer>();
            wnd.titleContent = new GUIContent("DictionarySerializer");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DictionarySerializer.uss");

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/DictionarySerializer.uxml");
            _uxmlTree = visualTree.Instantiate();

            _uxmlTree.Query<ObjectField>("field").First().RegisterValueChangedCallback(
                (changeEvent) =>
                {
                    _uxmlTree.Query<ListView>().First().hierarchy.Clear();

                    if (changeEvent.newValue == null)
                        return;

                    foreach (var field in changeEvent.newValue
                                            .GetType()
                                            .GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (field.FieldType.IsGenericType &&
                        field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        {
                            var mInfo = typeof(DictionarySerializer)
                                            .GetMethod("DisplayList", BindingFlags.Instance | BindingFlags.NonPublic)
                                            .MakeGenericMethod(changeEvent.newValue.GetType());

                            mInfo.Invoke(this, new object[] { field, (MonoBehaviour)(changeEvent.newValue) });
                        }
                    }
                }
            );

            root.Add(_uxmlTree);
        }


        private VisualElement MakeEntry(VisualElement field1, VisualElement field2)
        {
            var element = new VisualElement();
            element.AddToClassList("entry");

            element.Add(field1);
            element.Add(field2);

            return element;
        }

        //TODO Might need to move to monobehavior for in-game update
        private void DisplayList<T>(FieldInfo dictField, MonoBehaviour instance) where T : MonoBehaviour
        {
            var type1 = dictField.FieldType.GetGenericArguments()[0];
            var type2 = dictField.FieldType.GetGenericArguments()[1];

            var listView = _uxmlTree.Query<ListView>().First();

            var script = (T)instance;

            var data = SetUpData(script.GetInstanceID(), dictField.Name);

            var existingScript = data.DataList
                .FirstOrDefault(x => x.ScriptInstanceId == script.GetInstanceID());

            var existingDictionary = existingScript.Dictionaries
                .FirstOrDefault(x => x.DictionaryName == dictField.Name);

            for(int i = 0; i < existingDictionary.Keys.Count; i++)
            {
                listView.hierarchy.Add(
                    MakeEntry(
                        DictionarySerializerGenericExtensions.SetUpField(type1, existingDictionary.Keys[i]),
                        DictionarySerializerGenericExtensions.SetUpField(type2, existingDictionary.Values[i])
                        )
                    );
            }

            var space = new VisualElement();
            space.name = "space";

            listView.hierarchy.Add(space);
            listView.hierarchy.Add(new Label("Add New: "));

            var newEntryKey = DictionarySerializerGenericExtensions.SetUpField(type1, 0);
            var newEntryValue = DictionarySerializerGenericExtensions.SetUpField(type2, 0);
            listView.hierarchy.Add(MakeEntry(newEntryKey, newEntryValue));

            var addButton = new Button();
            addButton.Add(new Label("ADD"));
            addButton.clicked += () => 
            {
                //TODO remove constant saving and add an APPLY button to save changes

                existingDictionary.Keys.Add(DictionarySerializerGenericExtensions.GetFieldValue(type1, newEntryKey));
                existingDictionary.Values.Add(DictionarySerializerGenericExtensions.GetFieldValue(type2, newEntryValue));

                existingScript.Dictionaries.RemoveAll(x => x.DictionaryName == dictField.Name);
                existingScript.Dictionaries.Add(existingDictionary);

                data.DataList.RemoveAll(x => x.ScriptInstanceId == script.GetInstanceID());
                data.DataList.Add(existingScript);

                SerializerInterface.SaveData(data);
            };

            listView.hierarchy.Add(addButton);

            rootVisualElement.MarkDirtyRepaint();
        }
   
        private SerializedData SetUpData(int instanceID, string dictionaryName)
        {
            var data = SerializerInterface.LoadData();

            if (data.Equals(default(SerializedData)))
            {
                Debug.Log("No previous data found. A new structure will be created.");

                data.DataList = new List<SerializedScript>();
            }

            var existingScript = data.DataList
                    .FirstOrDefault(x => x.ScriptInstanceId == instanceID);

            if (existingScript.Equals(default(SerializedScript)))
            {
                Debug.Log("No previous script instance found. A new one will be created.");

                existingScript = new SerializedScript()
                {
                    ScriptInstanceId = instanceID,
                    SceneName = SceneManager.GetActiveScene().name,
                    Dictionaries = new List<SerializableDictionary>()
                };
            }

            var existingDictionary = existingScript.Dictionaries
                .FirstOrDefault(x => x.DictionaryName == dictionaryName);

            if (existingDictionary.Equals(default(SerializableDictionary)))
            {
                Debug.Log("No previous dictionary entry found. A new one will be created");

                existingDictionary = new SerializableDictionary()
                {
                    DictionaryName = dictionaryName,
                    Keys = new List<object>(),
                    Values = new List<object>()
                };

                existingScript.Dictionaries.Add(existingDictionary);
            }

            data.DataList.Add(existingScript);

            return data;
        }
    
    }
}