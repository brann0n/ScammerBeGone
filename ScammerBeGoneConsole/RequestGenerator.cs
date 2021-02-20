using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScammerBeGoneConsole
{
    public class RequestGenerator
    {


        public delegate void RequestStatusHandler(string requestInfo);

        public event RequestStatusHandler OnRequestCompleted;
        public event RequestStatusHandler OnRequestCreated;
        private double SizeInBytes;
        private double SizeInMB;
        private long total;
        private RestClient Client;


        /// <summary>
        /// Request generation class, give size in mb
        /// </summary>
        /// <param name="size">request size in mb</param>
        public RequestGenerator(double size)
        {
            this.SizeInBytes = size * 1024 * 1024;
            this.SizeInMB = size;
            this.total = 0;
        }

        public async Task Start(CustomStream data, int threadsize, int requestcount, string requestUrl)
        {
            Client = new RestClient(requestUrl);
            Client.Timeout = -1;
            Parallel.For(1, requestcount, new ParallelOptions { MaxDegreeOfParallelism = threadsize }, delegate (int i) { PerformRequest(data, i); });
        }

        private void PerformRequest(CustomStream data, int index)
        {
            using (MemoryStream s1 = data.GetStream())
            {
                OnRequestCreated?.Invoke($"starting request {index} with datalength: {data.Length}");
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.Files.Add(new FileParameter
                {
                    Name = "bestandje",
                    Writer = (s) => { s1.CopyTo(s); },
                    FileName = "kankerscammers.jpg",
                    ContentLength = data.Length
                });

                DateTime begin = DateTime.Now;
                IRestResponse response = Client.Execute(request);
                DateTime end = DateTime.Now;
                TimeSpan span = end - begin;
                string time = $" with time {span.TotalMilliseconds}ms";
                if (response == null) OnRequestCompleted?.Invoke("Request " + index + " completed with no result" + time);
                else if (response.StatusCode == System.Net.HttpStatusCode.OK) OnRequestCompleted?.Invoke("Request " + index + " completed OK" + time);
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) OnRequestCompleted?.Invoke("Request " + index + " completed with internal server error" + time);
                else OnRequestCompleted?.Invoke("Request " + index + " completed with other result: " + response.StatusCode + time);
                response = null;
                request = null;
                data = null;
                s1.Dispose();
                GC.Collect();
            }
        }

        public async Task<CustomStream> GenerateData()
        {
            const int blockSize = 1024 * 8;
            const int blocksPerMb = (1024 * 1024) / blockSize;

            byte[] data = new byte[blockSize];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                MemoryStream stream = new MemoryStream();

                for (int i = 0; i < SizeInMB * blocksPerMb; i++)
                {
                    crypto.GetBytes(data);
                    await stream.WriteAsync(data, 0, data.Length);
                }
                MemoryStream ms2 = new MemoryStream();
                ms2.Write(stream.GetBuffer(), 0, (int)stream.Length);
                return new CustomStream(ms2);
            }
        }
    }
}
