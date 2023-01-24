using System;
using System.Collections.Generic;
using System.IO;

namespace LettoreXml
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string fileName = @"C:\Users\Ignazio\Desktop\xml_per_prova\myConfig.xml";

            IEnumerable<string> lines = File.ReadLines(fileName);

            List<BasicElem> glzElements = new List<BasicElem>();
            Dictionary<string,string> dict = new Dictionary<string,string>();
            string key = null, value = null;
            bool cdataStarted = false, valueSaved = false;

            foreach (string line in lines)
            {
                var curLine = line.Trim();
                if (string.IsNullOrEmpty(curLine))
                {
                    //si può ignorare la riga
                }
                else if (curLine.StartsWith("<glz:"))
                {
                    curLine = curLine.Remove(0, 5);
                    if (curLine.StartsWith("Param"))
                    {
                        key = getKey(curLine);
                        if (paramHasValue(curLine))
                        {
                            value = getValue(curLine);
                            addToDictionary(dict, key, value);
                        }
                        else //caso longtext:
                        {
                            value = extractFirstValueLine(curLine);
                            cdataStarted = true;
                        }

                    }
                }
                //caso riga di CDATA senza chiusura elemento
                else if (cdataStarted)
                {
                    if(!lineHasClosingTag(curLine))
                    {
                        value += curLine;
                        //valueSaved = false;
                    }
                    else
                    {
                        value += extractEndingValueLine(curLine);
                        addToDictionary(dict, key, value);
                        value = null;
                        cdataStarted = false;
                    }
                }
                 
                    
                 Console.WriteLine(getKey(curLine) + " " + getValue(curLine));
                //string trimmedLine = line.Trim();
                //if (trimmedLine.StartsWith("<glz:"))
                //    Console.WriteLine($"{trimmedLine}");
            }
            //            Console.WriteLine(String.Join(Environment.NewLine, lines));
            foreach (var kvp in dict)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }

            static void addToDictionary(Dictionary<string, string> dict, string key, string value)
            {
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                }
                else
                {
                    if (key.EndsWith("[]"))
                    {
                        dict[key] = dict[key] + value;
                    }
                    else
                    {
                        dict[key] = value;
                    }
                }
            }
        }

        private static bool lineHasClosingTag(string curLine)
        {
            return curLine.EndsWith("</glz:Param>");
        }

        private static string extractEndingValueLine(string curLine)
        {
            int indOfSpecialChars = curLine.IndexOf("]]");
            string value = curLine.Substring(0, indOfSpecialChars);
            return value;
        }

        private static string extractFirstValueLine(string curLine)
        {
            string value = null;
            value = curLine.Substring(curLine.IndexOf("<![CDATA[") + 9);
            return value;
        }

        public static bool paramHasValue(string line)
        {
            return line.Contains("value=");
        }
        public static string getKey(string line)
        {
            string param = string.Empty;
            int paramStart = line.IndexOf("name=", 0);
            if (paramStart > -1)
            {
                paramStart = paramStart + 6;
                param = line.Substring(paramStart);
                int paramEnd = param.IndexOf("\"");
                param = param.Substring(0,paramEnd);
            }
            //string param;
            //int firstQuotes = line.IndexOf('"');
            //string lineWithoutFirstQuotes = line.Substring(0, firstQuotes);
            //int secondQuotes = lineWithoutFirstQuotes.IndexOf("\"");
            //param = lineWithoutFirstQuotes.Substring(0, secondQuotes);
            return param;
        }

        public static string getValue(string line)
        {
            string value = string.Empty;
            int keyStart = line.IndexOf("value=");
            if (keyStart > -1)
            {
                keyStart = keyStart + 7;
                value = line.Substring(keyStart);
                int valueEnd = value.IndexOf("\"");
                value= value.Substring(0,valueEnd);
            }
            return value;
        }
    }

}
