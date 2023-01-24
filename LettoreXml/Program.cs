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

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("<glz:"))
                    Console.WriteLine($"{trimmedLine}");
            }
//            Console.WriteLine(String.Join(Environment.NewLine, lines));
        }
    }
}
