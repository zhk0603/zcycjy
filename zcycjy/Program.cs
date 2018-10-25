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
                    UserName = "戚晓",
                    IdCar = "450922199406244008"
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

            //var tourId = "49t91ej3";
            //var ss = Options.Downloader.GetPage(
            //    new Crawler.Site($"http://www.zcycjy.com/addDialogSession?tourId={tourId}&user_id=&evaluate=0")
            //    {
            //        Method = "post",
            //        Accept = "application/json, text/javascript, */*; q=0.01",
            //        ContentType = "application/json;charset=UTF-8",
            //        Cookie = cookie,
            //        Host = "www.zcycjy.com",
            //        Referer ="http://www.zcycjy.com/doLogout?date=padaqucot?23f?asd",
            //        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            //    });

            var site = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                Url = "http://www.zcycjy.com/doLogin",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded",
                Postdata = $"loginVo.uname={System.Web.HttpUtility.UrlEncode(Options.UserName)}&loginVo.utype=1&loginVo.idcard={Options.IdCar}",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });


            var site1 = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                Url = "http://www.zcycjy.com/queryMyCourse?showView=1",
                ContentType = "text/html;charset=UTF-8",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });

            var site2 = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                Url = "http://www.zcycjy.com/turnToCourseStudyNew?courseId=123&linkId=11748851&eduYear=2018",
                ContentType = "text/html;charset=UTF-8",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });


            var add = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "*/*",
                Url = "http://www.zcycjy.com/addIsStudyOtherVedio.do",
                Method = "POST",
                Postdata = "lessionId=565",
                ContentType = "application/x-www-form-urlencoded",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/doLogout",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
            });

            for (var i = 21; i <= 22; i++)
            {
                var s = Options.Downloader.GetPage(new Crawler.Site
                {
                    Accept = "*/*",
                    Url = "http://www.zcycjy.com/endStudy.do",
                    Method = "POST",
                    Postdata =
                        $"lessionId=565&second={173.441 * i}&courseId=123&customerId=333214&linkId=11748851&allStudyTime=0&isClose=true&eduYear=2018",
                    ContentType = "application/x-www-form-urlencoded",
                    Cookie = cookie,
                    Referer = "http://www.zcycjy.com/doLogout",
                    UserAgent =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
                });

                Thread.Sleep(1000);
            }


            return Task.FromResult(true);
        }
    }

    class LoginPipelineOption : PipelineOptions
    {
        public string UserName { get; set; }
        public string IdCar { get; set; }
    }
}
