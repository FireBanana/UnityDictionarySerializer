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


        //Might need to move to monobehavior for in-game update
        private void DisplayList<T>(FieldInfo dictField, MonoBehaviour instance) where T : MonoBehaviour
        {
            var type1 = dictField.FieldType.GetGenericArguments()[0];
            var type2 = dictField.FieldType.GetGenericArguments()[1];

            var listView = _uxmlTree.Query<ListView>().First();

            var script = (T)instance;

            //IDictionary dict = (IDictionary)dictField.GetValue(script);

            SerializerInterface.SaveData(new DictionarySerializedData() 
            { 
                SceneName = "Peener",
                ScriptInstanceId = 2,
                Dictionaries = new List<SerializableDictionary>() 
                {
                    new SerializableDictionary(),
                    new SerializableDictionary(),
                    new SerializableDictionary(),
                    new SerializableDictionary(),
                    new SerializableDictionary()
                }
            });

            var data = SerializerInterface.LoadData();

            if (data.HasValue)
            {
                var query = data.Value.DataList
                    .Where(x => x.ScriptInstanceId == 2)
                    .First();

                foreach (var entry in query.Dictionaries)
                {
                    listView.hierarchy.Add(ResolveElement(type1));
                    listView.hierarchy.Add(ResolveElement(type2));
                }
            }            

            rootVisualElement.MarkDirtyRepaint();
        }

        private VisualElement ResolveElement(Type type)
        {
            switch (type)
            {
                case Type intType when intType == typeof(int):
                    return new IntegerField();

                case Type floatType when floatType == typeof(float):
                    return new FloatField();

                case Type stringType when stringType == typeof(string):
                    return new TextField();

                default:
                    return new Label("Unknown Type");
            }
        }
   
    }
}