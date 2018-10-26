using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Pipelines;

namespace zcycjy
{
    class Program
    {
        static void Main(string[] args)
        {
            var crawler = Crawler.CrawlerBuilder.Current
                .UsePipeline<LoginPipeline>(new LoginPipelineOption
                {
                    UserName = "*",
                    IdCar = "*"
                })
                .Builder();

            crawler.Run();

            Console.ReadKey();
        }
    }

    class LoginPipeline : CrawlerPipeline<LoginPipelineOption>
    {
        public LoginPipeline(LoginPipelineOption options) : base(options)
        {
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            var indexSite = Options.Downloader.GetPage(new Crawler.Site("http://www.zcycjy.com/turnToIndex"));

            var cookie = indexSite.Cookie;

            var site = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                Url = "http://www.zcycjy.com/doLogin",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded",
                Postdata =
                    $"loginVo.uname={System.Web.HttpUtility.UrlEncode(Options.UserName)}&loginVo.utype=1&loginVo.idcard={Options.IdCar}",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });


            var site1 = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                Url = "http://www.zcycjy.com/queryMyCourse?showView=1",
                ContentType = "text/html;charset=UTF-8",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });

            var courseId = 146;
            var linkId = 11748855;
            var year = 2018;

            var lessionId = 143;
            var lessionMinute = 46;
            var lessionCount = lessionMinute * 60 / 180;

            var site2 = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                Url = $"http://www.zcycjy.com/turnToCourseStudyNew?courseId={courseId}&linkId={linkId}&eduYear={year}",
                ContentType = "text/html;charset=UTF-8",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });

            var add = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "*/*",
                Url = "http://www.zcycjy.com/addIsStudyOtherVedio.do",
                Method = "POST",
                Postdata = "lessionId=" + lessionId,
                ContentType = "application/x-www-form-urlencoded",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });

            for (var i = 1; i <= lessionCount; i++)
            {
                var s = Options.Downloader.GetPage(new Crawler.Site
                {
                    Accept = "*/*",
                    Url = "http://www.zcycjy.com/endStudy.do",
                    Method = "POST",
                    Postdata =
                        $"lessionId={lessionId}&second={180 * i}&courseId={courseId}&customerId=333214&linkId={linkId}&allStudyTime=180&isClose=false&eduYear={year}",
                    ContentType = "application/x-www-form-urlencoded",
                    Cookie = cookie,
                    Referer = "http://www.zcycjy.com/doLogout",
                    UserAgent =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
                });

                Console.WriteLine(s.HtmlSource);
                Thread.Sleep(1000);
            }

            var end = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "*/*",
                Url = "http://www.zcycjy.com/endStudy.do",
                Method = "POST",
                Postdata =
                    $"lessionId={lessionId}&second={180 * lessionCount}&courseId={courseId}&customerId=333214&linkId={linkId}&allStudyTime=0&isClose=true&eduYear={year}",
                ContentType = "application/x-www-form-urlencoded",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });


            this.IsComplete = true;
            return Task.FromResult(false);
        }
    }

    class LoginPipelineOption : PipelineOptions
    {
        public string UserName { get; set; }
        public string IdCar { get; set; }
    }
}
