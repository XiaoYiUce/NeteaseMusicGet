using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NeteaseMusicGet
{

    class Program
    {
        public static string username;
        public static string password;
        public static string url;
        public static string symbol;
        public static string Cookie;
        public static string Nickname;

        static void Main(string[] args)
        {
            //判断平台
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true)
            {
                symbol = "\\";
            }
            else //如果不为Windows平台，统一使用Unix及类Unix路径
            {
                symbol = "/";
            }

            if (File.Exists(Environment.CurrentDirectory+symbol+"config.json")==true) //判断config.json是否存在
            {
                StreamReader file = File.OpenText(Environment.CurrentDirectory + symbol + "config.json");
                JsonTextReader reader = new JsonTextReader(file);
                JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                Cookie = (string)jsonObject["Cookie"];
                Nickname = (string)jsonObject["Name"];
            }
            else //如果不为真，则执行登陆操作
            {
                login(symbol);             
                /*HttpResponseMessage responseinfo = Common.httpClient.GetAsync(httpurl).Result; //访问url并取回数据*/
                /*var cookie = responseinfo.Headers.GetValues("Set-Cookie");*/
            }

            Console.WriteLine("您好," + Nickname + "!");
            Core.Core.MusicSearch(symbol,Cookie);

            /*String MusicLink = */
            Console.WriteLine("如还需下载歌曲，请输入1，否则按任意键退出");
            String method = Console.ReadLine();

            if (method == "1")
            {
                Core.Core.MusicSearch(symbol,Cookie);
            }
            else
            {
                return;
            }


        }

        /// <summary>
        /// 用户信息实体
        /// </summary>
        public class UserInfo
        {
            public string Name { get; set; }
            public string Cookie { get; set; }
        }

        /// <summary>
        /// 网易云登陆模块，用于用户登陆并保存Cookie
        /// </summary>
        static void login(string symbol)
        {
            Console.WriteLine("请选择登陆方式(1为邮箱，2为手机)");
            String loginmethod = Console.ReadLine();
            int method = Int32.Parse(loginmethod);
            if (method == 1) //如果所选模式为1
            {
                Console.WriteLine("请输入您的邮箱账号");
                username = Console.ReadLine();
                Console.WriteLine("请输入您的密码");
                password = HttpUtility.HtmlEncode(Console.ReadLine());
                url = "http://server2.odtm.tech:3000/login?email=" + username + "&password=" + password;
            }
            else if (method == 2)//如果所选模式为2
            {
                Console.WriteLine("请输入您的中国大陆手机号");
                String phonenumber = Console.ReadLine();
                Console.WriteLine("请输入您的密码");
                String password = HttpUtility.HtmlEncode(Console.ReadLine());
                url = "http://server2.odtm.tech:3000/login/cellphone?phone=" + phonenumber + "&password=" + password;
            }
            else
            {
                Console.WriteLine("您输入的数值无效，请输入正确数值!");
                Console.ReadKey();
                login(symbol);
            }


            var httpUri=new Uri(url);
            var httphandler = new HttpClientHandler();
            httphandler.UseCookies = true;
            httphandler.CookieContainer = new CookieContainer();

            var responseContext = new HttpClient(httphandler).GetAsync(httpUri).Result;
            String result = responseContext.Content.ReadAsStringAsync().Result; //定义result变量
            JObject JsonReader = JObject.Parse(result); //反序列化对象
            int code = (int)JsonReader["code"];
            if (code == 200) //如果返回成功状态码
            {
                foreach (var cookieContext in httphandler.CookieContainer.GetCookies(httpUri).Cast<Cookie>().ToList())
                {
                    Cookie = Cookie + cookieContext.Name + "=" + cookieContext.Value + ";";  //定义Cookie
                }

                UserInfo info = new UserInfo();
                info.Name = (string)JsonReader["profile"]["nickname"];
                info.Cookie = Cookie;
                string FileSaveDir = Environment.CurrentDirectory + symbol + "config.json";
                File.WriteAllText(FileSaveDir, JsonConvert.SerializeObject(info)); //序列化并保存JSON
                Nickname = info.Name;
            }
            else
            {
                Console.WriteLine("账号或密码错误!请重新输入");
                login(symbol);
            }
        }


    }
}
