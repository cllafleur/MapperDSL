﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MapperDslLib.Runtime
{
    internal class InstanceVisitor<T>
    {
        private string value;
        private List<PropertyInfo> navigation;

        public InstanceVisitor(string value)
        {
            this.value = value;
            BuildNavigation();
        }

        private (object, PropertyInfo) GetLastPropertyInstance(T obj)
        {
            object currentValue = obj;
            foreach (var item in navigation.Take(navigation.Count - 1))
            {
                currentValue = item.GetValue(currentValue);
            }
            return (currentValue, navigation[navigation.Count - 1]);
        }

        internal void SetInstance(T obj, object value)
        {
            var (o, p) = GetLastPropertyInstance(obj);
            var convertedValue = value == null ? null : Convert.ChangeType(value, p.PropertyType, CultureInfo.InvariantCulture);
            p.SetValue(o, convertedValue);
        }

        internal object GetInstance(T obj)
        {
            var (o, p) = GetLastPropertyInstance(obj);
            return p.GetValue(o);
        }

        private void BuildNavigation()
        {
            var navigation = new List<PropertyInfo>();
            var currentType = typeof(T);
            foreach (var identifier in this.value.Split('.'))
            {
                var property = currentType.GetProperty(identifier);
                if (property == null)
                {
                    throw new MapperVisitException($"Property '{identifier}' not found");
                }
                navigation.Add(property);
                currentType = property.PropertyType;
            }
            this.navigation = navigation;
        }
    }
}