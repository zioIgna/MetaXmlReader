using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LettoreXml
{
    internal class Config
    {
        public Config(string fileName) {
            this.fileName = fileName;
            init();
        }

        Dictionary<string, string> dict = new Dictionary<string, string>();
        string fileName;

        string key = null, value = null;
        bool cdataStarted = false;
        IEnumerable<string> lines;
        private void init()
        {
            lines = File.ReadLines(fileName);

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
                //caso riga di CDATA
                else if (cdataStarted)
                {   // senza chiusura elemento
                    if (!lineHasClosingTag(curLine))
                    {
                        value += curLine;
                        //valueSaved = false;
                    } // con chiusura elemento
                    else
                    {
                        value += extractEndingValueLine(curLine);
                        addToDictionary(dict, key, value);
                        value = null;
                        cdataStarted = false;
                    }
                }
            }
            printDictionary();
        }

        private void printDictionary()
        {
            foreach (var kvp in dict)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
        }

        private void addToDictionary(Dictionary<string, string> dict, string key, string value)
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

        private static bool paramHasValue(string line)
        {
            return line.Contains("value=");
        }

        private static string getKey(string line)
        {
            string param = string.Empty;
            int paramStart = line.IndexOf("name=", 0);
            if (paramStart > -1)
            {
                paramStart = paramStart + 6;
                param = line.Substring(paramStart);
                int paramEnd = param.IndexOf("\"");
                param = param.Substring(0, paramEnd);
            }
            return param;
        }

        private static string getValue(string line)
        {
            string value = string.Empty;
            int keyStart = line.IndexOf("value=");
            if (keyStart > -1)
            {
                keyStart = keyStart + 7;
                value = line.Substring(keyStart);
                int valueEnd = value.IndexOf("\"");
                value = value.Substring(0, valueEnd);
            }
            return value;
        }
    }
}
