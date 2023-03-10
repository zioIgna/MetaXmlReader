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
            string imgName = "brux.jpg"; //venezia.jpg  //portrait.jpg  //inesistente.jpg
            string resizeDefinition = "thumbnail"; //thumbnail  //medium
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
            Console.Write("Enter image name to be edited or type 'exit': ");
            imgName = Console.ReadLine();
            Console.Write("Enter resizeDefinition: ");
            resizeDefinition= Console.ReadLine();
            while (imgName.CompareTo("exit") != 0)
            {
                imageResize.resize(imgName, resizeDefinition);
                Console.Write("Enter image name to be edited or type 'exit': ");
                imgName = Console.ReadLine();
                Console.Write("Enter resizeDefinition: ");
                resizeDefinition = Console.ReadLine();
            }

        }

    }

}
