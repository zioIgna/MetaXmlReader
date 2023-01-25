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

            Config config = new Config(fileName);

        }

    }

}
