using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScammerBeGoneConsole
{
    public class CustomStream
    {
        private MemoryStream internalStream;

        public int Length => (int)internalStream.Length;

        private static object locker = new object();
        public CustomStream(MemoryStream stream)
        {
            internalStream = stream;
        }

        public MemoryStream GetStream()
        {
            MemoryStream streamer = new MemoryStream();
            lock (locker)
            {
                internalStream.Seek(0, SeekOrigin.Begin);
                internalStream.CopyTo(streamer);
                internalStream.Seek(0, SeekOrigin.Begin);
            }
            streamer.Seek(0, SeekOrigin.Begin);
            return streamer;
        }
    }
}
