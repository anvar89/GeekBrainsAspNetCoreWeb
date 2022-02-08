using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lesson1_AsyncPprogramming
{
    internal class Program
    {
        static readonly CancellationTokenSource cts = new CancellationTokenSource();
        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            string url = "https://jsonplaceholder.typicode.com/posts/";
            string fileName = "result.txt";
            int startId = 4;
            int endId = 13;

            try
            {
                cts.CancelAfter(2000);
                DownloadPostsById(url, fileName, startId, endId, cts.Token).GetAwaiter().GetResult();
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                cts.Dispose();
            }

            Console.ReadKey();
        }

        static async Task DownloadPostsById(string url, string fileName, int startId, int endId, CancellationToken token)
        {

            for (int id = startId; id <= endId; id++)
            {
                try
                {
                    Post post = await GetPostAsync(new Uri(url + id), token);
                    await WriteTextAsync(fileName, PostPrint(post), token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

        }

        static async Task<Post> GetPostAsync(Uri uri, CancellationToken token)
        {

            HttpResponseMessage responseMessage = await client.GetAsync(uri, token);
            responseMessage.EnsureSuccessStatusCode();

            string text = await responseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Post>(text);

        }

        static async Task WriteTextAsync(string filePath, string text, CancellationToken token)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length, token);
            }
        }

        static string PostPrint(Post post) => post.UserId + Environment.NewLine +
                                              post.Id + Environment.NewLine +
                                              post.Title + Environment.NewLine +
                                              post.Body + Environment.NewLine + Environment.NewLine;
    }
}
