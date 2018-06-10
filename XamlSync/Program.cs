using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace XamlSync
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFile = args[0];
            FileInfo fi = new FileInfo(sourceFile);

            ResourceDictionary resourceDictionary;
            using (FileStream fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                XamlReader reader = new XamlReader();
                resourceDictionary = (ResourceDictionary)reader.LoadAsync(fs);
            }

            foreach (var file in fi.Directory.GetFiles("*.xaml"))
            {
                if (file.Name != fi.Name)
                {
                    Merge(resourceDictionary, file);
                }
            }

        }

        private static void Merge(ResourceDictionary resourceDictionary, FileInfo file)
        {
            ResourceDictionary targetDictionary;
            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                XamlReader reader = new XamlReader();
                targetDictionary = (ResourceDictionary)reader.LoadAsync(fs);
            }

            foreach (var key in resourceDictionary.Keys)
            {
                if (!targetDictionary.Contains(key))
                {
                    Console.WriteLine($"{key.ToString()} is not found, adding to the target resource dictionary");
                    targetDictionary.Add(key, resourceDictionary[key]);
                }
            }

            using (FileStream fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var writer = XmlWriter.Create(fs, new XmlWriterSettings() { Indent = true });
                XamlWriter.Save(targetDictionary, writer);
                writer.Close();
            }

        }
    }
}
