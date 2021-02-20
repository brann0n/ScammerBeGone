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
        private int SizeInBytes;
        private int SizeInMB;
        private long total;
        private RestClient Client;


        /// <summary>
        /// Request generation class, give size in mb
        /// </summary>
        /// <param name="size">request size in mb</param>
        public RequestGenerator(int size)
        {
            this.SizeInBytes = size * 1024 * 1024;
            this.SizeInMB = size;
            this.total = 0;
        }

        public async Task Start(string data, string urlToPost)
        {
            Client = new RestClient(urlToPost);
            Client.Timeout = -1;
            Parallel.For(1, 5*20, new ParallelOptions {MaxDegreeOfParallelism = 20 }, delegate (int i) { PerformRequest(data, i); });
        }

        private void PerformRequest(string data, int index)
        {
            OnRequestCreated?.Invoke($"starting request {index} with datalength: {data.Length}");

            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", $"{{\"data\":\"{data}\"}}", ParameterType.RequestBody);
            IRestResponse response = Client.Execute(request);

            if(response == null) OnRequestCompleted?.Invoke("Request " + index + " completed with no result");
            else if(response.StatusCode == System.Net.HttpStatusCode.OK) OnRequestCompleted?.Invoke("Request " + index + " completed OK");
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) OnRequestCompleted?.Invoke("Request " + index + " completed with internal server error");
            else OnRequestCompleted?.Invoke("Request " + index + " completed with other result: " + response.StatusCode);
        }


        public async Task<MemoryStream> GenerateData()
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

                return stream;
            }
        }
    }
}
