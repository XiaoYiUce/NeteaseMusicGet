using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web;
using static Core.Core;

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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" _ _        _                      __ __           _       ___         _   ");
            Console.WriteLine("| \\ | ___ _| |_ ___  ___  ___ ___ |  \\  \\ _ _  ___<_> ___ /  _>  ___ _| |_ ");
            Console.WriteLine("|   |/ ._> | | / ._><_> |<_-</ ._>|     || | |<_-<| |/ | '| <_/\\/ ._> | |  ");
            Console.WriteLine("|_\\_|\\___. |_| \\___.<___|/__/\\___.|_|_|_|`___|/__/|_|\\_|_.`____/\\___. |_| ");
            Console.WriteLine("[Info]项目GitHub地址:https://github.com/XiaoYiUce/NeteaseMusicGet");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Info]正在初始化");
            //判断平台
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true)
            {
                symbol = "\\";
                Console.WriteLine("[Info]当前平台为Windows");
            }
            else //如果不为Windows平台，统一使用Unix及类Unix路径
            {
                symbol = "/";
                Console.WriteLine("[Info]当前平台为Linux/OSX/类Unix");
            }

            Console.WriteLine("[Info]正在载入配置");
            if (File.Exists(Environment.CurrentDirectory + symbol + "config.json") == true) //判断config.json是否存在
            {
                StreamReader file = File.OpenText(Environment.CurrentDirectory + symbol + "config.json");
                JsonTextReader reader = new JsonTextReader(file);
                JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                Cookie = (string)jsonObject["Cookie"];
                Nickname = (string)jsonObject["Name"];
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[Info]欢迎回来," + Nickname + "!");
            }
            else //如果不为真，则执行登陆操作
            {
                Console.WriteLine("[Info]没有找到配置文件，执行初始化");
                login(symbol);
                /*HttpResponseMessage responseinfo = Common.httpClient.GetAsync(httpurl).Result; //访问url并取回数据*/
                /*var cookie = responseinfo.Headers.GetValues("Set-Cookie");*/
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[Info]欢迎使用," + Nickname + "!");
            }
            NeteaseMusicGet();
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
            Console.WriteLine("[Info]请选择登陆方式(1为邮箱，2为手机)");
            String loginmethod = Console.ReadLine();
            int method = Int32.Parse(loginmethod);
            if (method == 1) //如果所选模式为1
            {
                Console.WriteLine("[Info]请输入您的邮箱账号");
                username = Console.ReadLine();
                Console.WriteLine("[Info]请输入您的密码");
                password = HttpUtility.HtmlEncode(Console.ReadLine());
                url = "http://server2.odtm.tech:3000/login?email=" + username + "&password=" + password;
            }
            else if (method == 2)//如果所选模式为2
            {
                Console.WriteLine("[Info]请输入您的中国大陆手机号");
                String phonenumber = Console.ReadLine();
                Console.WriteLine("[Info]请输入您的密码");
                String password = HttpUtility.HtmlEncode(Console.ReadLine());
                url = "http://server2.odtm.tech:3000/login/cellphone?phone=" + phonenumber + "&password=" + password;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]您输入的数值无效，请输入正确数值!");
                Console.ForegroundColor = ConsoleColor.Green;
                login(symbol);
            }

            Console.WriteLine("[Info]正在登陆");
            var httpUri = new Uri(url);
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
            else if (result == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]网络错误!程序即将退出");
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]账号或密码错误!请重新输入");
                Console.ForegroundColor = ConsoleColor.Green;
                login(symbol);
            }
        }

        public static void NeteaseMusicGet()
        {
            Console.WriteLine("[Info]输入\"help\"获取帮助");
            Console.ForegroundColor = ConsoleColor.Gray;
            string operate = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green;
            if (operate == "search")
            {
                MusicSearch(symbol, Cookie);
            }
            else if(operate == "listget")
            {

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]没有找到对应的指令，输入help获取帮助");
                Console.ForegroundColor = ConsoleColor.Green;
                NeteaseMusicGet();
            }
        }

    }
}
