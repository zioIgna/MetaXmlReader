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

            string filePath = string.Empty;
            string fileName = "myConfig.xml";

            Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), fileName));

            string fullFileName = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            //string fileName = @"C:\Users\Ignazio\Desktop\xml_per_prova\myConfig.xml";

            Config config = new Config(fullFileName);

        }

    }

}
