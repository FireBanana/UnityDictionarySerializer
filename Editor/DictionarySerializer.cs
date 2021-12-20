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

            var data = SerializerInterface.LoadData();

            if (data.HasValue)
            {
                var query = data.Value.DataList
                    .Where(x => x.ScriptInstanceId == script.GetInstanceID())
                    .First();

                foreach (var entry in query.Dictionaries)
                {
                    listView.hierarchy.Add(MakeEntry(ResolveElement(type1), ResolveElement(type2)));
                }
            }

            listView.hierarchy.Add(new Label("Add New: "));
            listView.hierarchy.Add(MakeEntry(ResolveElement(type1), ResolveElement(type2)));

            var addButton = new Button();
            addButton.Add(new Label("ADD"));
            addButton.clicked += () => 
            {
                //TODO check if data not available
                var existingScript = data?.DataList.Find(x => x.ScriptInstanceId == script.GetInstanceID());

                if (existingScript == null)
                {
                    Debug.Log("No previous script found");
                    existingScript = new SerializedScript()
                    {
                        ScriptInstanceId = script.GetInstanceID(),
                        SceneName = SceneManager.GetActiveScene().name,
                        Dictionaries = new List<SerializableDictionary>()
                    };
                }

                var existingDictionary = existingScript?.Dictionaries.FirstOrDefault(x => x.DictionaryName == dictField.Name);

                if (existingDictionary.Equals(default(SerializableDictionary)))
                {
                    Debug.Log("No previous dictionary entry found");

                    existingDictionary = new SerializableDictionary()
                    {
                        DictionaryName = dictField.Name,
                        Keys = new List<object>(),
                        Values = new List<object>()
                    };
                }

                //TODO Add real field values
                existingDictionary?.Keys.Add("FIELD 1 HERE");
                existingDictionary?.Values.Add("FIELD 2 HERE");

                existingScript?.Dictionaries.Add(existingDictionary.Value);

                SerializerInterface.SaveData(existingScript.Value);
            };

            listView.hierarchy.Add(addButton);

            rootVisualElement.MarkDirtyRepaint();
        }

        private VisualElement ResolveElement(Type type)
        {
            switch (type)
            {
                case Type intType when intType == typeof(int):
                    {
                        var i = new IntegerField();
                        i.AddToClassList("entry-field");
                        return i;
                    }

                case Type floatType when floatType == typeof(float):
                    {
                        var i = new FloatField();
                        i.AddToClassList("entry-field");
                        return i;
                    }

                case Type stringType when stringType == typeof(string):
                    {
                        var i = new TextField();
                        i.AddToClassList("entry-field");
                        return i;
                    }

                default:
                    return new Label("Unknown Type");
            }
        }
   
    }
}