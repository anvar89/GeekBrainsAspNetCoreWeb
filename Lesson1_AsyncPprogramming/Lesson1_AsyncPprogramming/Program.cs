using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lesson1_AsyncPprogramming
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string url = "https://jsonplaceholder.typicode.com/posts/";
            string fileName = "result.txt";
            int startId = 4;
            int endId = 13;

            using (var client = new HttpClient())
            using (var cts = new CancellationTokenSource())
            {
                var posts = new List<Task<Post>>();

                cts.CancelAfter(2000);
                
                for (int id = startId; id <= endId; id++)
                {
                    posts.Add(GetPostAsync(new Uri(url + id), client, cts.Token));
                }

                await Task.WhenAll(posts);

                posts.ForEach(post => File.AppendAllText(fileName, PostPrint(post.Result)));
            }

        }


        static async Task<Post> GetPostAsync(Uri uri, HttpClient client, CancellationToken token)
        {

            HttpResponseMessage responseMessage = await client.GetAsync(uri, token);
            responseMessage.EnsureSuccessStatusCode();

            string text = await responseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Post>(text);

        }

        static string PostPrint(Post post) => post.UserId + Environment.NewLine +
                                              post.Id + Environment.NewLine +
                                              post.Title + Environment.NewLine +
                                              post.Body + Environment.NewLine + Environment.NewLine;
    }
}
