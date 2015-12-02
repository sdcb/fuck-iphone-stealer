using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ListApple
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("request count(8 recommanded): ");
            var requestCount = int.Parse(Console.ReadLine());
            ServicePointManager.DefaultConnectionLimit = 10000;
            
            var alert = new AlertInSecond();

            var ss = new SemaphoreSlim(requestCount, requestCount);
            while (true)
            {
                ss.Wait();
                Task.Run(OneRequest).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.Faulted)
                    {
                        alert.AddOneFail();
                    }
                    else
                    {
                        var response = t.Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            alert.AddOneSuccess();
                        }
                        else
                        {
                            alert.AddOneFail();
                        }
                    }
                    
                    ss.Release();
                });
            }
        }

        public static async Task<HttpResponseMessage> OneRequest()
        {
            const string url = "http://app-xpid.com/zbht/save.asp";
            var client = new HttpClient();
            var body = PostContext.Create();
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8");
            client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            client.DefaultRequestHeaders.Add("Referer", "http://app-xpid.com/indexdh1.htm");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36");

            var response = await client.PostAsync(url, new FormUrlEncodedContent(body.Body()));
            return response;
        }
    }

    public class AlertInSecond
    {
        public AlertInSecond()
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }

        int successCount = 0;
        int failCount = 0;
        DateTime begin = DateTime.Now;

        public void AddOneSuccess()
        {
            Interlocked.Increment(ref successCount);
        }

        public void AddOneFail()
        {
            Interlocked.Increment(ref failCount);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"{(DateTime.Now-begin)} {successCount} success, {failCount} failed.");
        }
    }

    public class PostContext
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string A { get; set; }

        public int SubmitX { get; set; }

        public int SubmitY { get; set; }

        //public string Body()
        //{
        //    return $"u={UserName}&p={Password}&a=&Submit.x={SubmitX}&Submit.y={SubmitY}";
        //}

        public IEnumerable<KeyValuePair<string, string>> Body()
        {
            yield return new KeyValuePair<string, string>("u", UserName);
            yield return new KeyValuePair<string, string>("p", Password);
            yield return new KeyValuePair<string, string>("a", A);
            yield return new KeyValuePair<string, string>("Submit.X", SubmitX.ToString());
            yield return new KeyValuePair<string, string>("Submit.y", SubmitY.ToString());
        }

        public static Random r = new Random();

        private static readonly string[] commonEmails = new[]
            {
                "qq",
                "163",
                "live",
                "hotmail",
                "gmail",
                "outlook"
            };


        public static PostContext Create()
        {
            var email = commonEmails[r.Next(0, commonEmails.Length)];
            var username = $"{r.Next(1000000, 9999999)}{r.Next(1000000, 9999999)}".Substring(0, r.Next(9, 12));
            var password = r.Next(2) == 0 ?
                Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(6, 16) :
                $"{r.Next(10000, 99999)}{r.Next(10000, 99999)}";

            return new PostContext
            {
                UserName = $"{username}@{email}.com",
                Password = password,
                SubmitX = r.Next(1, 20),
                SubmitY = r.Next(1, 20)
            };
        }
    }
}
