using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Pipelines;
using HtmlAgilityPack;

namespace zcycjy
{
    class Program
    {

        static void Main(string[] args)
        {
            var crawler = Crawler.CrawlerBuilder.Current
                .UsePipeline<LoginPipeline>(new LoginPipelineOption
                {
                    UserName = ConfigurationManager.AppSettings["userName"],
                    IdCar = ConfigurationManager.AppSettings["idCar"],
                    CustomerId = ConfigurationManager.AppSettings["customerId"]
                })
                .Builder();

            crawler.Run();

            Console.ReadKey();
        }
    }

    class LoginPipeline : CrawlerPipeline<LoginPipelineOption>
    {
        private static string _userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.55 Safari/537.36 Edg/96.0.1054.34";

        public LoginPipeline(LoginPipelineOption options) : base(options)
        {
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            // 获取cookie
            var indexSite = Options.Downloader.GetPage(new Crawler.Site("http://www.zcycjy.com/jupUcIndex"));

            var cookie = indexSite.Cookie;

            var random = new Random(11);


            // 登录
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
                UserAgent = _userAgent
            });


            // 我的所有课程。
            var newQueryMyCourse = Options.Downloader.GetPage(new Crawler.Site
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                Url = "http://www.zcycjy.com/newQueryMyCourse",
                ContentType = "text/html;charset=UTF-8",
                Cookie = cookie,
                Referer = "http://www.zcycjy.com/newQueryMyCourse",
                UserAgent = _userAgent
            });


            var courseList = newQueryMyCourse.DocumentNode.SelectNodes("//a[@class='new_learn']");
            Console.WriteLine($"共找到{courseList.Count}个课程");

            foreach (var htmlNode in courseList)
            {
                var trNode = htmlNode.ParentNode.ParentNode;

                if ("是".Equals(trNode.SelectSingleNode("td[6]").InnerText.Trim()))
                {
                    Console.WriteLine($"课程 [ {trNode.SelectSingleNode("td[1]").InnerText.Trim()} ] 已经完成。");
                    continue;
                }

                Console.WriteLine($"正在学习 [ {trNode.SelectSingleNode("td[1]").InnerText.Trim()} ]");
                Console.WriteLine($"学分：{trNode.SelectSingleNode("td[3]").InnerText.Trim()}");
                Console.WriteLine($"学时：{trNode.SelectSingleNode("td[4]").InnerText.Trim()}");

                var href = htmlNode.GetAttributeValue("href", "");
                var match = Regex.Match(href, "('([^<]*)')");
                if (!match.Success)
                {
                    Console.WriteLine($"找不到课程ID");
                    continue;
                }

                var num = match.Value.Replace("'", "").Split(',');

                var courseId = num[0];
                var linkId = num[1];
                var eduYear = num[2];

                var lessionPage = Options.Downloader.GetPage(new Crawler.Site
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                    Method = "POST",
                    Postdata = $"keyCourseId={courseId}&linkId={linkId}&eduYear={eduYear}&v={random.NextDouble()}",
                    Url = "http://www.zcycjy.com/skipToStudyCoursePage ",
                    ContentType = "application/x-www-form-urlencoded",
                    Cookie = cookie,
                    Referer = "http://www.zcycjy.com/newQueryMyCourse",
                    UserAgent = _userAgent
                });

                var lessionList = lessionPage.DocumentNode.SelectNodes("//a[@class='startStudy']");
                Console.WriteLine(
                    $"[ {trNode.SelectSingleNode("td[1]").InnerText.Trim()} ] 共 [{lessionList.Count}] 小结");


                foreach (var node in lessionList)
                {
                    var pNode = node.ParentNode.ParentNode;

                    var onclick = node.GetAttributeValue("onclick", "");
                    match = Regex.Match(onclick, "('([^<]*)')");
                    if (!match.Success)
                    {
                        Console.WriteLine($"\t找不到 小结ID");
                        continue;
                    }

                    var lessionId = match.Value.Replace("'", "");


                    // http://www.zcycjy.com/doCourseStudy

                    var doCourseStudy = Options.Downloader.GetPage(new Crawler.Site
                    {
                        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                        Method = "POST",
                        Postdata = $"lessionId={lessionId}&linkId={linkId}&eduYear={eduYear}&v={random.NextDouble()}",
                        Url = "http://www.zcycjy.com/doCourseStudy ",
                        ContentType = "application/x-www-form-urlencoded",
                        Cookie = cookie,
                        Referer = "http://www.zcycjy.com/skipToStudyCoursePage",
                        UserAgent = _userAgent
                    });

                    var currentTime = 0D; // 秒
                    match = Regex.Match(doCourseStudy.DocumentNode.InnerHtml, "vh = '([^<]*)';");
                    if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
                    {
                        var tmp = match.Value.Substring(6); // vh = '540.0';
                        tmp = tmp.Substring(0, tmp.Length - 2);
                        double.TryParse(tmp, out currentTime);
                    }

                    var add = Options.Downloader.GetPage(new Crawler.Site
                    {
                        Accept = "*/*",
                        Url = "http://www.zcycjy.com/addIsStudyOtherVedio.do",
                        Method = "POST",
                        Postdata = "lessionId=" + lessionId,
                        ContentType = "application/x-www-form-urlencoded",
                        Cookie = cookie,
                        Referer = "http://www.zcycjy.com/doCourseStudy",
                        UserAgent = _userAgent
                    });

                    Console.WriteLine($"\t开始学习 [{pNode.SelectSingleNode("td[1]").InnerText.Trim()}]");
                    var lessionMinute = 2 + int.Parse(pNode.SelectSingleNode("td[2]").InnerText.Trim());

                    // start
                    var start = Options.Downloader.GetPage(new Crawler.Site
                    {
                        Accept = "*/*",
                        Url = "http://www.zcycjy.com/doSBCusStudyFinsh",
                        Method = "POST",
                        Postdata =
                            $"lessionId={lessionId}&second={currentTime}&courseId={courseId}&customerId={Options.CustomerId}&linkId={linkId}&allStudyTime=0&isClose=false&eduYear={eduYear}",
                        ContentType = "application/x-www-form-urlencoded",
                        Cookie = cookie,
                        Referer = "http://www.zcycjy.com/doCourseStudy",
                        UserAgent = _userAgent
                    });
                    Console.WriteLine($"\t\t开始添加进度： {start.HtmlSource}");

                    var second = 0d;
                    for (var i = 1 + Math.Floor(currentTime / 60); i <= lessionMinute; i++)
                    {
                        second = (60 * i);
                        var s = Options.Downloader.GetPage(new Crawler.Site
                        {
                            Accept = "*/*",
                            Url = "http://www.zcycjy.com/doSBCusStudyFinsh",
                            Method = "POST",
                            Postdata =
                                $"lessionId={lessionId}&second={second}&courseId={courseId}&customerId={Options.CustomerId}&linkId={linkId}&allStudyTime=60&isClose=false&eduYear={eduYear}",
                            ContentType = "application/x-www-form-urlencoded",
                            Cookie = cookie,
                            Referer = "http://www.zcycjy.com/doCourseStudy",
                            UserAgent = _userAgent
                        });

                        if (s.HtmlSource.Contains("error"))
                        {
                            Console.WriteLine($"\t\t添加进度失败：{s.HtmlSource}");
                            break;
                        }

                        Console.WriteLine($"\t\t添加进度：{Math.Ceiling(second / 60)} 分钟。 {s.HtmlSource}");
                        Thread.Sleep(60 * 1000);
                    }

                    var end = Options.Downloader.GetPage(new Crawler.Site
                    {
                        Accept = "*/*",
                        Url = "http://www.zcycjy.com/doSBCusStudyFinsh",
                        Method = "POST",
                        Postdata =
                            $"lessionId={lessionId}&second={second}&courseId={courseId}&customerId={Options.CustomerId}&linkId={linkId}&allStudyTime=0&isClose=true&eduYear={eduYear}",
                        ContentType = "application/x-www-form-urlencoded",
                        Cookie = cookie,
                        Referer = "http://www.zcycjy.com/doCourseStudy",
                        UserAgent = _userAgent
                    });
                    Console.WriteLine($"\t\t结束添加进度： {end.HtmlSource}");

                    Console.WriteLine($"\t结束学习 [{pNode.SelectSingleNode("td[1]").InnerText.Trim()}]");
                }


                Console.WriteLine("##################################################");
                Thread.Sleep(5000);
            }


            //var site2 = Options.Downloader.GetPage(new Crawler.Site
            //{
            //    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
            //    Url = $"http://www.zcycjy.com/skipToStudyCoursePage?keyCourseId={Options.CourseId}&linkId={Options.LinkId}&eduYear={Options.Year}&v="+ new Random().NextDouble(),
            //    ContentType = "text/html;charset=UTF-8",
            //    Cookie = cookie,
            //    Referer = "http://www.zcycjy.com/queryMyCourse?showView=1",
            //    UserAgent = _userAgent
            //});




            // http://www.zcycjy.com/doSBCusStudyFinsh lessionId=4704&second=59.948885&courseId=1196&customerId=333214&linkId=18600707&allStudyTime=60&isClose=false&eduYear=2020



            //var end = Options.Downloader.GetPage(new Crawler.Site
            //{
            //    Accept = "*/*",
            //    Url = "http://www.zcycjy.com/doSBCusStudyFinsh",
            //    Method = "POST",
            //    Postdata =
            //        $"lessionId={Options.LessionId}&second={second + 60}&courseId={Options.CourseId}&customerId={Options.CustomerId}&linkId={Options.LinkId}&allStudyTime=60&isClose=true&eduYear={Options.Year}",
            //    ContentType = "application/x-www-form-urlencoded",
            //    Cookie = cookie,
            //    Referer = "http://www.zcycjy.com/doCourseStudy",
            //    UserAgent = _userAgent
            //});

            //Console.WriteLine("结束：" + end.HtmlSource);

            this.IsComplete = true;
            return Task.FromResult(false);
        }
    }

    class LoginPipelineOption : PipelineOptions
    {
        public string UserName { get; set; }
        public string IdCar { get; set; }
        public string CustomerId { get; set; }
    }
}
