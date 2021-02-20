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

            g = new RequestGenerator(RequestSizeInMb); //do requests with 10mb in size
            g.OnRequestCompleted += G_OnRequestCompleted;
            g.OnRequestCreated += G_OnRequestCreated;
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
            if(args.Length == 0)
            {
                MainConsole.WriteLine("Please restart the program with your posturl as the first parameter. This program uploads data in json form, in future updates this can be changed with parameter", RequestSizeInMb);
            }
            else
            {
                MainConsole.WriteLine("Generating random data with size {0}", RequestSizeInMb);
                MemoryStream data = await g.GenerateData();
                MainConsole.WriteLine("Starting request service...");
                StreamReader reader = new StreamReader(data);
                reader.BaseStream.Position = 0;
                await g.Start(await reader.ReadToEndAsync(), args[0]);
            }
        }
    }
}
