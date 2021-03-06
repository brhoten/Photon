﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Photon.Framework.Variables
{
    public class VariableSet
    {
        private readonly object variable;


        public VariableSet(object variable)
        {
            this.variable = variable;
        }

        public object GetValue(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var path = name.Split('.');

            return GetPathValue(variable, path, $"({variable.GetType()})");
        }

        public T GetValue<T>(string name)
        {
            return (T)GetValue(name);
        }

        public string this[string name] => GetValue(name)?.ToString();

        private object GetPathValue(object value, string[] path, string sourcePath)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path.Length == 0) return null;

            var partValue = GetPartValue(value, path[0], sourcePath);

            if (path.Length == 1) return partValue;

            var subPath = path.Skip(1).ToArray();
            var subSourcePath = $"{sourcePath}.{path[0]}";
            return GetPathValue(partValue, subPath, subSourcePath);
        }

        private object GetPartValue(object value, string part, string sourcePath)
        {
            var valueType = value.GetType();

            if (value is IDictionary<string, object> dictionary) {
                if (dictionary.TryGetValue(part, out var _val))
                    return _val;
            }
            else if (value is JObject jsonObject) {
                if (jsonObject.TryGetValue(part, out var _val))
                    return _val;
            }
            else {
                var partProperty = valueType.GetProperty(part);
                if (partProperty != null)
                    return partProperty.GetValue(value);

                var partField = valueType.GetField(part);
                if (partField != null)
                    return partField.GetValue(value);
            }

            throw new ArgumentException($"Value '{part}' not found at '{sourcePath}'!");
        }
    }
}
