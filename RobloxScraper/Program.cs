using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using RobloxScraper.DbModels;
using RobloxScraper.RobloxModels;
using RobloxScraper.Telemetry;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobloxScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            StartScraper();
           
            
            /*
            //TestPerformance2(testHtml);
            HTMLparser parser = new HTMLparser(testHtml);
            HTMLchunk chunk = null;
            int i = 0;
            while ((chunk = parser.ParseNext()) != null)
            {
                switch (chunk.oType)
                {
                    // matched open tag, ie <a href="">
                    case HTMLchunkType.OpenTag:
                        if(chunk.oParams.ContainsKey("class"))
                        {
                            Console.Write(chunk.oHTML);
                        }

                        break;

                    // matched close tag, ie </a>
                    case HTMLchunkType.CloseTag:
                        break;

                    // matched normal text
                    case HTMLchunkType.Text:
                        break;

                    // matched HTML comment, that's stuff between <!-- and -->
                    case HTMLchunkType.Comment:
                        if (chunk.sTag == "span")
                        {
                            Console.Write(chunk.oHTML);
                        }
                        break;
                }
                i++;
            }*/



            Console.ReadKey(true);
            Console.ReadLine();
        }

        static string StringFold(string input, Func<char, string> proc)
        {
            return string.Concat(input.Select(proc).ToArray());
        }

        static string FoldProc(char input)
        {
            if (input >= 128)
            {
                return string.Format(@"\u{0:x4}", (int)input);
            }
            return input.ToString();
        }

        static string EscapeToAscii(string input)
        {
            return StringFold(input, FoldProc);
        }

        static void SetupEncodingProvider()
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        static void StartScraper()
        {
            SetupEncodingProvider();
            Config.Initialize(new IniFile());
            ForumsRepository respository = new ForumsRepository(new ForumsContext());

            TaskManager manager = new TaskManager(respository);
            DbManager dbManager = new DbManager(respository, manager);
            ConsoleOutput consoleOutput = new ConsoleOutput(manager, dbManager, respository);
            Task.Run(async () =>
            {
                await manager.Start();
            }).GetAwaiter().GetResult();
        }

        static void TestPerformance1(string html)
        {
            Stopwatch stopwatch = new Stopwatch();
            long totalTime = 0;
            int iterations = 1000;

            for(int i = 0; i < iterations; i++)
            {
                Console.SetCursorPosition(0, 0);
                stopwatch.Restart();
                HtmlParser parser = new HtmlParser();
                IHtmlDocument document = parser.Parse(html);
                totalTime += stopwatch.ElapsedTicks;
                Console.WriteLine($"{i}/{iterations}");
            }
            Console.WriteLine($"Ticks Per Parse: {totalTime/iterations}");
            Console.WriteLine($"ms Per Parse: {totalTime/iterations/10000}");
        }

        /*static void TestPerformance2(string html)
        {
            Stopwatch stopwatch = new Stopwatch();
            long totalTime = 0;
            int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                Console.SetCursorPosition(0, 0);
                stopwatch.Restart();
                HTMLparser parser = new HTMLparser(html);
                while (parser.ParseNext() != null) { }

                totalTime += stopwatch.ElapsedTicks;
                Console.WriteLine($"{i}/{iterations}");
            }
            Console.WriteLine($"Ticks Per Parse: {totalTime / iterations}");
            Console.WriteLine($"ms Per Parse: {totalTime / iterations / 10000}");
        }*/
        static string GetTestFile()
        {
            string path = @"testhtml.html";
            return File.ReadAllText(path);
        }


    }
}