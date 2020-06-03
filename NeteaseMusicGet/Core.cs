using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Core
{

    public class Core
    {
        /// <summary>
        /// 检测输入字符是否为纯整数
        /// </summary>
        /// <param name="sNum"></param>
        /// <returns>返回值为true或者false</returns>
        static bool IsNumber(string str)
        {
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9]\d*$");
            return reg1.IsMatch(str);
        }

        public static string FileName;
        public static string SingerName;
        public static string MusicName;
        public static string MusicDownloadLink;
        public static string MusicInfo;
        public static int MusicNumber;

        public static void MusicDownload(string url, string MusicName, string SingerName, string symbol, string Cookie) //MusicDownload子程序
        {
            ///<summary>
            ///此子程序用于下载音乐，第一个值为音乐文件的URL，第二个参数为音乐名称，第三个参数为歌手名称，第四个名称为标识符
            ///</summary>

            WebClient client = new WebClient();
            string ProgramRunDirectory = Environment.CurrentDirectory;
            string patternflac = @"(.*)(\.flac)$";
            string patternncm = @"(.*)(\.ncm)$";
            string patternmp3 = @"(.*)(\.mp3)$";
            if (Regex.IsMatch(url, patternflac))
            {
                FileName = MusicName + "-" + SingerName + ".flac";
            }
            else if (Regex.IsMatch(url, patternncm))
            {
                FileName = MusicName + "-" + SingerName + ".ncm";
            }
            else if (Regex.IsMatch(url, patternmp3))
            {
                FileName = MusicName + "-" + SingerName + ".mp3";
            }

            string SaveDirectory = ProgramRunDirectory + symbol + FileName;
            Console.WriteLine("[Work]正在下载"+MusicName+"-"+SingerName);
            client.DownloadFile(url, SaveDirectory);
            Console.WriteLine("[Info]文件已保存至:" + SaveDirectory);
            /*String MusicLink = */
            Console.WriteLine("[Info]如还需下载歌曲，请输入1，否则按任意键退出");
            String method = Console.ReadLine();

            if (method == "1")
            {
                MusicSearch(symbol, Cookie);
            }
            else
            {
                return;
            }
        }

        ///<summary>
        ///此方法用于检索音乐并执行下载，需填写symbol（平台路径符号）
        ///</summary>
        public static void MusicSearch(string symbol, string Cookie)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[Info]请输入您要搜索的歌曲关键词");
            String SongName = Console.ReadLine();
            if (SongName == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]请填入正确的关键词!");
                MusicSearch(symbol, Cookie);
            }

            if (SongName.Trim() == string.Empty)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]请填入正确的关键词!");
                MusicSearch(symbol, Cookie);
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[Info]正在搜索");
            string SearchSongurl = "http://server2.odtm.tech:3000/search?keywords=" + SongName;
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(20); //设定超时时间为20ms
            string UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0"; //定义UA
            httpClient.DefaultRequestHeaders.Add("user-agent", UA); //将UA添加至请求头中
            httpClient.DefaultRequestHeaders.Add("Cookie", Cookie);//添加Cookie
            HttpResponseMessage response = httpClient.GetAsync(new Uri(SearchSongurl)).Result; //使用GET方法获取JSON
            String result = response.Content.ReadAsStringAsync().Result; //定义result变量
            JObject JsonReader = JObject.Parse(result);
            JArray List = JArray.Parse(JsonReader["result"]["songs"].ToString());
            for (int i = 0; List.Count > i; ++i)
            {
                JObject json = JObject.Parse(List[i].ToString());
                JArray Singer = JArray.Parse(JsonReader["result"]["songs"][i]["artists"].ToString());
                MusicInfo = MusicInfo + "[" + i + "]" + "曲名:" + json["name"] + " 歌手:";
                if (Singer.Count == 0)
                {
                    MusicInfo = MusicInfo + json["artists"][0]["name"] + "\r\n";
                }
                else
                {

                    for (int i2 = 0; Singer.Count > i2; ++i2)
                    {
                        JObject Singer2 = JObject.Parse(Singer[i2].ToString());
                        if (i2 == Singer.Count - 1)
                        {
                            MusicInfo = MusicInfo + Singer2["name"];
                        }
                        else
                        {
                            MusicInfo = MusicInfo + Singer2["name"] + "/";
                        }
                    }
                    MusicInfo = MusicInfo + "\r\n";
                }

            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[Info]返回结果如下:");
            Console.WriteLine(MusicInfo);
            Console.ForegroundColor = ConsoleColor.Green;

            MusicInfo = ""; //清空变量
            Console.WriteLine("[Info]请输入您需要下载的歌曲编号");
            string MusicNumberTemp = Console.ReadLine();
            if (IsNumber(MusicNumberTemp) == true)
            {
                MusicNumber = Int32.Parse(MusicNumberTemp);
                if (-1 < MusicNumber)
                {
                    if (MusicNumber < 30)
                    {
                        JObject json2 = JObject.Parse(List[MusicNumber].ToString());
                        String MusicID = (string)json2["id"];
                        MusicName = (string)json2["name"];
                        SingerName = "";
                        JArray Singer = JArray.Parse(JsonReader["result"]["songs"][MusicNumber]["artists"].ToString());
                        if (Singer.Count == 0)
                        {
                            SingerName = (string)json2["artists"][0]["name"];
                        }
                        else
                        {
                            for (int i2 = 0; Singer.Count > i2; ++i2)
                            {
                                JObject Singer2 = JObject.Parse(Singer[i2].ToString());
                                if (i2 == Singer.Count - 1)
                                {
                                    SingerName = SingerName + Singer2["name"];
                                }
                                else
                                {
                                    SingerName = SingerName + Singer2["name"] + ",";
                                }
                            }
                        }


                        Console.WriteLine("[Info]歌曲名称:" + MusicName + " 歌曲ID:" + MusicID + " 歌手:" + SingerName);
                        Console.WriteLine("[Info]准备开始下载"+MusicName+"-"+SingerName);
                        HttpResponseMessage response3 = httpClient.GetAsync(new Uri("http://server2.odtm.tech:3000/song/url?id=" + MusicID)).Result; //获取歌曲信息JSON
                        JObject JsonReader2 = JObject.Parse(response3.Content.ReadAsStringAsync().Result);
                        MusicDownloadLink = (string)JsonReader2["data"][0]["url"];
                        if (MusicDownloadLink == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[Err]无法获取下载地址，请检查您是否有网易云VIP权限或歌曲对应专辑");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("请按任意键继续...");
                            Console.ReadKey();
                            MusicSearch(symbol, Cookie);
                        }
                        else
                        {
                            Console.WriteLine("[Info]歌曲下载地址为:" + MusicDownloadLink);
                            MusicDownload(MusicDownloadLink, MusicName, SingerName, symbol, Cookie);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[Err]您键入的数值有误!请重新搜索");
                        Console.ForegroundColor = ConsoleColor.Green;
                        MusicSearch(symbol, Cookie);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Err]您键入的数值有误!请重新搜索");
                    Console.ForegroundColor = ConsoleColor.Green;
                    MusicSearch(symbol, Cookie);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]您键入的数值有误!请重新搜索");
                Console.ForegroundColor = ConsoleColor.Green;
                MusicSearch(symbol, Cookie);
            }

        }
    }

}
