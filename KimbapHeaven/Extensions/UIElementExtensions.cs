﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KimbapHeaven
{
    public static class UIElementExtensions
    {
        public static T DeepClone<T>(this T source) where T : UIElement
        {

            T result;

            // Get the type
            Type type = source.GetType();

            // Create an instance
            result = Activator.CreateInstance(type) as T;

            CopyProperties<T>(source, result, type);

            DeepCopyChildren<T>(source, result);

            return result;
        }
        private static void DeepCopyChildren<T>(T source, T result) where T : UIElement
        {
            // Deep copy children.
            Panel sourcePanel = source as Panel;
            if (sourcePanel != null)
            {
                Panel resultPanel = result as Panel;
                if (resultPanel != null)
                {
                    foreach (UIElement child in sourcePanel.Children)
                    {
                        // RECURSION!
                        UIElement childClone = DeepClone(child);
                        resultPanel.Children.Add(childClone);
                    }
                }
            }
        }
        private static void CopyProperties<T>(T source, T result, Type type) where T : UIElement
        {
            // Copy all properties.

            IEnumerable<PropertyInfo> properties = type.GetRuntimeProperties();

            foreach (var property in properties)
            {
                if (property.Name != "Name")
                {
                    if ((property.CanWrite) && (property.CanRead))
                    {
                        object sourceProperty = property.GetValue(source);

                        UIElement element = sourceProperty as UIElement;
                        if (element != null)
                        {
                            UIElement propertyClone = element.DeepClone();

                            property.SetValue(result, propertyClone);
                        }
                        else
                        {
                            try
                            {
                                property.SetValue(result, sourceProperty);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }
                    }
                }
            }
        }
    }
}
