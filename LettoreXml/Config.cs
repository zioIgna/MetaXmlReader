using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LettoreXml
{
    internal class Config
    {
        string fileName;
        string filePath;
        public Config(string fileName) {
            this.fileName = fileName;
            filePath = Path.GetDirectoryName(fileName);
            init();
        }

        Dictionary<string, string> dict = new Dictionary<string, string>();

        string key = string.Empty, value = string.Empty, tempKey = string.Empty;
        bool cdataStarted = false;
        IEnumerable<string> lines;
        string prefix = string.Empty;
        private void init()
        {
            handleFileLines(fileName);
            printDictionary();
        }

        private void handleFileLines(string fileName)
        {
            lines = File.ReadLines(fileName);

            foreach (string line in lines)
            {
                var curLine = line.Trim();
                if (!string.IsNullOrEmpty(curLine))
                {
                    //tag di apertura di un elemento
                    if (curLine.StartsWith(prefix = "<glz:"))
                    {
                        curLine = removePrefix(prefix, curLine);
                        if (curLine.StartsWith("Param"))
                        {
                            tempKey = key + getKey(curLine);
                            if (paramHasValue(curLine))
                            {
                                value = getValue(curLine);
                                addEntryToDictionary(tempKey, value);
                            }
                            //caso inizio longtext:
                            else
                            {
                                value = extractFirstValueLine(curLine);
                                cdataStarted = true;
                            }
                        }
                        else if (curLine.StartsWith("Group"))
                        {
                            key = key + getKey(curLine) + "/";
                        }
                        else if (curLine.StartsWith("Import"))
                        {
                            string newFileName = getSourceFile(curLine);
                            if (!string.IsNullOrEmpty(newFileName))
                            {
                                newFileName = Path.Combine(filePath, newFileName);
                                handleFileLines(newFileName);
                            }
                        }
                    }
                    //tag di chiusura di un gruppo
                    else if (lineHasGroupClosingTag(curLine))
                    {
                        popKey();
                    }
                    //caso riga di CDATA successiva a prima riga
                    else if (cdataStarted)
                    {   // senza chiusura elemento
                        if (!lineHasClosingTag(curLine))
                        {
                            value += curLine;
                        } // con chiusura elemento
                        else
                        {
                            value += extractEndingValueLine(curLine);
                            addEntryToDictionary(tempKey, value);
                            cdataStarted = false;
                        }
                    }
                }
            }
        }

        private bool lineHasGroupClosingTag(string line)
        {
            return line.StartsWith("</glz:Group");
        }

        private string removePrefix(string prefix, string line)
        {
            string truncLine = string.Empty;
            truncLine = line.Remove(0, prefix.Length);
            return truncLine;
        }

        private void printDictionary()
        {
            foreach (var kvp in dict)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
        }

        private void popKey()
        {
            if(!string.IsNullOrEmpty(key))
            {
                if(key.LastIndexOf('/') == key.Length - 1)
                {
                    key = key.Remove(key.Length - 1);
                    key = key.Remove(key.LastIndexOf('/') + 1);
                }
            }
        }

        private bool keyExists(string groupKey, string paramKey)
        {
            string fullKey = groupKey + paramKey;
            return this.dict.ContainsKey(fullKey);
        }

        private void addEntryToDictionary(string tempKey, string value)
        {
            if (!dict.ContainsKey(tempKey))
            {
                dict.Add(tempKey, value);
            }
            else
            {
                if (tempKey.EndsWith("[]"))
                {
                    dict[tempKey] = dict[tempKey] + value;
                }
                else
                {
                    dict[tempKey] = value;
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

        //private static string getKey(string line)
        //{
        //    string param = string.Empty;
        //    int paramStart = line.IndexOf("name=", 0);
        //    if (paramStart > -1)
        //    {
        //        paramStart = paramStart + 6;
        //        param = line.Substring(paramStart);
        //        int paramEnd = param.IndexOf("\"");
        //        param = param.Substring(0, paramEnd);
        //    }
        //    return param;
        //}

        private static string getKey(string line)
        {
            return getRefValue(line, "name");
        }

        private static string getValue(string line)
        {
            return getRefValue(line, "value");
        }

        private static string getSourceFile(string line)
        {
            return getRefValue(line, "src");
        }

        //private static string getValue(string line)
        //{
        //    string value = string.Empty;
        //    int keyStart = line.IndexOf("value=");
        //    if (keyStart > -1)
        //    {
        //        keyStart = keyStart + 7;
        //        value = line.Substring(keyStart);
        //        int valueEnd = value.IndexOf("\"");
        //        value = value.Substring(0, valueEnd);
        //    }
        //    return value;
        //}

        private static string getRefValue(string line, string reference)
        {
            string refValue = string.Empty;
            int refValueStart = line.IndexOf(reference + "=");
            if (refValueStart > -1)
            {
                refValueStart = refValueStart + reference.Length + 2;
                refValue = line.Substring(refValueStart);
                int refValueEnd = refValue.IndexOf("\"");
                refValue = refValue.Substring(0, refValueEnd);
            }
            return refValue;
        }
    }
}
