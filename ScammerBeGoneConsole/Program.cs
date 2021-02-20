using Konsole;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScammerBeGoneConsole
{
    public class Program
    {
        HighSpeedWriter Writer;
        Window MainWindow;
        IConsole MainConsole;
        IConsole RequestConsole;
        IConsole ResponseConsole;

        private RequestGenerator g;
        private int RequestSizeInMb = 8;

        static void Main(string[] args) => new Program().Init(args).GetAwaiter().GetResult();

        private Program()
        {
            this.Writer = new HighSpeedWriter();
            this.MainWindow = new Window(Writer);
            MainWindow.CursorVisible = false;
            MainConsole = Window.OpenBox("Scammers Be Gone", 0, 0, 110, 5, new BoxStyle() { ThickNess = LineThickNess.Single, Body = new Colors(ConsoleColor.Green, ConsoleColor.DarkGray) });
            RequestConsole = Window.OpenBox("Requests", 0, 5, 55, 25, new BoxStyle() { ThickNess = LineThickNess.Double, Body = new Colors(ConsoleColor.White, ConsoleColor.Blue) });
            ResponseConsole = Window.OpenBox("Responses", 55, 5, 55, 25, new BoxStyle() { ThickNess = LineThickNess.Double, Body = new Colors(ConsoleColor.White, ConsoleColor.Blue) });

            
        }

        private void G_OnRequestCreated(string requestInfo)
        {
            RequestConsole.WriteLine(requestInfo);
        }

        private void G_OnRequestCompleted(string requestInfo)
        {
            ResponseConsole.WriteLine(requestInfo);
        }

        private async Task Init(string[] args)
        {
            if(args.Length < 4)
            {
                MainConsole.WriteLine("Please restart the program with arguments like \r\n ..exe [threadsize] [requestcount] [requestsize] [targeturl]\r\n This program uploads data in json form, in future updates this can be changed with parameter", RequestSizeInMb);
            }
            else
            {
                int threadsize = int.Parse(args[0]);
                int requestcount = int.Parse(args[1]);
                double requestSize = double.Parse(args[2]);
                string requestUrl = args[3];


                g = new RequestGenerator(requestSize == 0 ? 10 : requestSize); //do requests with 10mb in size
                g.OnRequestCompleted += G_OnRequestCompleted;
                g.OnRequestCreated += G_OnRequestCreated;

                MainConsole.WriteLine($"Generating random data with size {requestSize}mb.");
                CustomStream data = await g.GenerateData();
                MainConsole.WriteLine($"Starting request service with {threadsize} threads and {requestcount} cycles on {requestUrl}");
                await g.Start(data, threadsize, requestcount, requestUrl);
            }
        }
    }
}
