using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DSerializer
{
    public static class DictionarySerializerGenericExtensions
    {
        public static VisualElement SetUpField(Type type, object value)
        {
            switch (type)
            {
                case Type intType when intType == typeof(int):
                    {
                        var i = new IntegerField();
                        i.value = (int)value;
                        i.AddToClassList("entry-field");
                        return i;
                    }

                case Type floatType when floatType == typeof(float):
                    {
                        var i = new FloatField();
                        i.value = (float)value;
                        i.AddToClassList("entry-field");
                        return i;
                    }

                case Type stringType when stringType == typeof(string):
                    {
                        var i = new TextField();
                        i.value = (string)value;
                        i.AddToClassList("entry-field");
                        return i;
                    }

                default:
                    return new Label("Unknown Type");
            }
        }

        public static object GetFieldValue(Type type, VisualElement entry)
        {
            switch (type)
            {
                case Type intType when intType == typeof(int):
                    {
                        var f = (IntegerField)entry;
                        return f.value;
                    }

                case Type floatType when floatType == typeof(float):
                    {
                        var f = (FloatField)entry;
                        return f.value;
                    }

                case Type stringType when stringType == typeof(string):
                    {
                        var f = (TextField)entry;
                        return f.value;
                    }

                default:
                    return new Label("Unknown Type");
            }
        }
    }
}
