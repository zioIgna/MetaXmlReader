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
            string fileName = "innerFolder/myConfig.xml";
            string query;
            dynamic val;
            string imgName = "venezia.jpg";
            string resizeDefinition = "thumbnail";
            //string valueType;

            Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), fileName));

            string fullFileName = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            //string fileName = @"C:\Users\Ignazio\Desktop\xml_per_prova\myConfig.xml";

            Config config = new Config(fileName); //fullName

            //do
            //{
            //    Console.Write("Enter key to be returned or type 'exit': ");
            //    query = Console.ReadLine();

            //    val = config.get(query);

            //    Console.WriteLine("Returned value equals: {0} and is of type: {1}", val ?? "null", val == null ? "null" : val.GetType());
            //} while (query.CompareTo("exit") != 0);

            ImageResize imageResize = new ImageResize(config);
            imageResize.resize(imgName, resizeDefinition);
        }

    }

}
