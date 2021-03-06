﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
            XmlDocument xmlDoc = new XmlDocument();
            List<KeyValuePair<string, string>> keyvalues = new List<KeyValuePair<string, string>>();

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;



            using (FileStream fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                xmlDoc.Load(fs);
                int nodeIndex = 0;
                var nodes = xmlDoc.LastChild.ChildNodes;
               
                foreach(XmlNode node in nodes)
                {
                    nodeIndex++;
                    if(node.LocalName == "String")
                    {
                        string value = node.InnerText;
                        string key = node.Attributes[0].Value;
                        keyvalues.Add(new KeyValuePair<string, string>(key, value));
                    }
                    else if(node.LocalName == "#comment")
                    {
                        keyvalues.Add(new KeyValuePair<string, string>("#comment", node.Value));
                    }
                }

            }

            foreach (var file in fi.Directory.GetFiles("*.xaml"))
            {
                if (file.Name != fi.Name)
                {
                    Merge(keyvalues, file);
                }
            }

        }


        //static TranslatorService.TranslatorClient client = new TranslatorService.TranslatorClient(ConfigurationManager.AppSettings["API_KEY"], "en-US");
        static CognitiveServices.Translator.TranslateClient client =
            new CognitiveServices.Translator.TranslateClient(
                new CognitiveServices.Translator.Configuration.CognitiveServicesConfig()
                {
                    SubscriptionKey = ConfigurationManager.AppSettings["API_KEY"],
                });
        

        private static async Task<string> Translate(string originalString, string originalLang, string targetLang)
        {
            string[] lines = originalString.Split('\n');
            if (lines == null) return string.Empty ;
            List<string> results = new List<string>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var response = client.Translate(new CognitiveServices.Translator.Translate.RequestContent(line), new CognitiveServices.Translator.Translate.RequestParameter()
                    {
                        From = "en-US",
                        To = new string[] { targetLang },
                    });
                    results.Add(response[0].Translations[0].Text);
                }

            }
            string finalResult = string.Join("\n", results);
            Console.WriteLine($"Translation: from {originalLang} : {originalString} -> to {targetLang} : {finalResult} ");
            return finalResult;
        }

        private static void Merge(List<KeyValuePair<string, string>> keyvalues, FileInfo file)
        {
            Console.WriteLine("####### Checks file: " + file.Name);
            XmlDocument xmlDoc = new XmlDocument();
            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                xmlDoc.Load(fs);
                XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                xmlNamespaceManager.AddNamespace("s", "clr-namespace:System;assembly=mscorlib");
                xmlNamespaceManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
                XmlNode previousNode = null;
                foreach (var keyvalue in keyvalues)
                {
                    if (keyvalue.Key != "#comment")
                    {
                        var node = xmlDoc.SelectSingleNode($"//s:String[@x:Key='{keyvalue.Key}']", xmlNamespaceManager);
                        if (node == null)
                        {
                            var newNode = xmlDoc.CreateElement("s:String", "clr-namespace:System;assembly=mscorlib");
                            var attr = xmlDoc.CreateAttribute("x", "Key", "http://schemas.microsoft.com/winfx/2006/xaml");
                            attr.Value = keyvalue.Key;
                            newNode.Attributes.Append(attr);
                            string to = file.Name.Substring(file.Name.IndexOf(".") + 1, file.Name.LastIndexOf(".") - file.Name.IndexOf(".") -1);
                            string translation = Translate(keyvalue.Value, "en", to).Result;
                            if (translation.Contains("\n"))
                            {
                                var cdata = xmlDoc.CreateCDataSection(translation);
                                newNode.AppendChild(cdata);
                                var attribute = xmlDoc.CreateAttribute("xml:space");
                                attribute.Value = "preserve";
                                newNode.Attributes.Append(attribute);
                            }
                            else
                            {
                                newNode.InnerText = translation;
                            }
                            if (previousNode != null)
                            {
                                xmlDoc.LastChild.InsertAfter(newNode, previousNode);
                            }
                            else
                            {
                                xmlDoc.LastChild.AppendChild(newNode);
                            }
                            previousNode = newNode;
                        }
                        else
                        {
                            previousNode = node;
                        }
                    }
                    else
                    {
                        var node = xmlDoc.SelectSingleNode($"//comment()[. = '{keyvalue.Value}']");
                        if (node == null)
                        {
                            var newNode = xmlDoc.CreateComment(keyvalue.Value);
                            if (previousNode != null)
                            {
                                xmlDoc.LastChild.InsertAfter(newNode, previousNode);
                            }
                            else
                            {
                                xmlDoc.LastChild.AppendChild(newNode);
                            }
                            previousNode = newNode;
                        }
                        else
                        {
                            previousNode = node;
                        }
                    }
                }
            }

            using (FileStream fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var writer = XmlWriter.Create(fs, new XmlWriterSettings() { Indent = true, IndentChars = "    ", });
                xmlDoc.Save(writer);
                writer.Close();
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
