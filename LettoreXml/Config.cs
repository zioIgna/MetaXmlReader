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
        
        Dictionary<string, string> dict = new Dictionary<string, string>();

        string key = string.Empty, value = string.Empty, tempKey = string.Empty;
        bool cdataStarted = false;
        IEnumerable<string> lines;
        string prefix = string.Empty;
        public Config(string fileName) {
            this.fileName = fileName;
            filePath = Path.GetDirectoryName(fileName);
            init();
        }

        private void init()
        {
            handleFileLines(fileName);
            printDictionary();
        }

        public dynamic get(string key)
        {
            if(!dict.ContainsKey(key))
            {
                return null;
            }
            int i = 0;
            bool val = false;
            string ret = string.Empty;
            if (key.EndsWith("[]"))
            {
                string[] arr = dict[key].Split(',');
                return arr;
            }
            else if (int.TryParse(dict[key], out i))
            {
                return i;
            }
            else if (Boolean.TryParse(dict[key], out val))
            {
                return val;
            }
            else
            {
                ret = dict[key];
                return ret;
            }
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
                    dict[tempKey] = dict[tempKey] + "," + value;
                }
                else
                {
                    dict[tempKey] = value;
                }
            }
        }
        private bool lineHasClosingTag(string curLine)
        {
            return curLine.EndsWith("</glz:Param>");
        }

        private string extractEndingValueLine(string curLine)
        {
            int indOfSpecialChars = curLine.IndexOf("]]");
            string value = curLine.Substring(0, indOfSpecialChars);
            return value;
        }

        private string extractFirstValueLine(string curLine)
        {
            string value = null;
            value = curLine.Substring(curLine.IndexOf("<![CDATA[") + 9);
            return value;
        }

        private bool paramHasValue(string line)
        {
            return line.Contains("value=");
        }

        private string getKey(string line)
        {
            return getRefValue(line, "name");
        }

        private string getValue(string line)
        {
            return getRefValue(line, "value");
        }

        private string getSourceFile(string line)
        {
            return getRefValue(line, "src");
        }

        private string getRefValue(string line, string reference)
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
