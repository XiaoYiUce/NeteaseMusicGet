using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            System.Text.RegularExpressions.Regex reg1 = new Regex(@"^[0-9]\d*$");
            return reg1.IsMatch(str);
        }

        /// <summary>
        /// 匹配文件名中是否有不允许存在的符号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool FileNameCheck(string fileName)
        {
            Regex NameCheck = new Regex("[< > / \\ |: \" ?  *]"); //定义一个正则表达式
            if(fileName == null)
            {
                return (false);
            }
            else
            {
                return NameCheck.IsMatch(fileName);
            }
        }

        public static string FileName;
        public static string SingerName;
        public static string MusicName;
        public static string MusicDownloadLink;
        public static string MusicInfo;
        public static int MusicNumber;

        ///<summary>
        ///此方法用于下载音乐
        ///</summary>
        ///<param name="Cookie">身份验证Cookie</param>
        ///<param name="MusicName">歌曲名称</param>
        ///<param name="SingerName">歌手</param>
        ///<param name="symbol">判断是在Win环境下还是在类Unix环境</param>
        ///<param name="url">音乐URL</param>
        public static void MusicDownload(string url, string MusicName, string SingerName, string symbol, string Cookie) //MusicDownload子程序
        {
            WebClient client = new WebClient();
            string ProgramRunDirectory = Environment.CurrentDirectory;

            if(url == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]无法获取这首歌的下载地址，请检查您的账号权限");
                Console.ForegroundColor = ConsoleColor.Green;
                return;
            }

            if(FileNameCheck(MusicName) == true)
            {
                //如果匹配到对应字符
                MusicName=MusicName.Replace("<", "");
                MusicName=MusicName.Replace(">", "");
                MusicName=MusicName.Replace("/", "");
                MusicName=MusicName.Replace("\\", "");
                MusicName=MusicName.Replace("|", "");
                MusicName=MusicName.Replace(":", "");
                MusicName=MusicName.Replace("\"", "");
                MusicName=MusicName.Replace("*", "");
            }

            if (FileNameCheck(SingerName) == true)
            {
                //如果匹配到对应字符
                SingerName=SingerName.Replace("<", "");
                SingerName=SingerName.Replace(">", "");
                SingerName=SingerName.Replace("/", "");
                SingerName=SingerName.Replace("\\", "");
                SingerName=SingerName.Replace("|", "");
                SingerName=SingerName.Replace(":", "");
                SingerName=SingerName.Replace("\"", "");
                SingerName=SingerName.Replace("*", "");
            }

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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Work]正在下载"+MusicName+"-"+SingerName);
            string SaveDirectory = ProgramRunDirectory + symbol + FileName;
            client.DownloadFile(url, SaveDirectory);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Info]文件已保存至:" + SaveDirectory);
        }

        ///<summary>
        ///此方法用于检索音乐并执行下载，需填写symbol（平台路径符号）
        ///</summary>
        public static void MusicSearch(string symbol, string Cookie)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[Info]请输入您要搜索的歌曲关键词，输入main返回主菜单");
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

            if (SongName == "main")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                NeteaseMusicGet.Program.NeteaseMusicGet(); //返回主页面
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


            Console.WriteLine("[Info]请输入您需要下载的歌曲编号，输入search回到搜索，main回到主菜单");
            string MusicNumberTemp = Console.ReadLine();
            if (IsNumber(MusicNumberTemp) == true)
            {
                MusicNumber = Int32.Parse(MusicNumberTemp);
                if (-1 < MusicNumber)
                {
                    if (MusicNumber < List.Count)
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
                            MusicInfo = ""; //清空变量
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("请按任意键继续...");
                            Console.ReadKey();
                            MusicSearch(symbol, Cookie);
                        }
                        else
                        {
                            Console.WriteLine("[Info]歌曲下载地址为:" + MusicDownloadLink);
                            MusicDownload(MusicDownloadLink, MusicName, SingerName, symbol, Cookie);
                            NeteaseMusicGet.Program.NeteaseMusicGet();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        MusicInfo = ""; //清空变量
                        Console.WriteLine("[Err]您键入的数值有误!请重新搜索");
                        Console.ForegroundColor = ConsoleColor.Green;
                        MusicSearch(symbol, Cookie);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Err]您键入的数值有误!请重新搜索");
                    MusicInfo = ""; //清空变量
                    Console.ForegroundColor = ConsoleColor.Green;
                    MusicSearch(symbol, Cookie);
                }
            }
            else if(MusicNumberTemp == "main")
            {
                MusicInfo = ""; //清空变量
                Console.ForegroundColor = ConsoleColor.Green;
                NeteaseMusicGet.Program.NeteaseMusicGet();
            }
            else if(MusicNumberTemp == "search")
            {
                MusicInfo = ""; //清空变量
                MusicSearch(symbol, Cookie);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]您键入的数值有误!请重新搜索");
                Console.ForegroundColor = ConsoleColor.Green;
                MusicSearch(symbol, Cookie);
            }

        }

        /// <summary>
        /// 获取用户歌单并下载
        /// </summary>
        /// <param name="Cookie">身份验证Cookie</param>
        /// <param name="id">用户User ID</param>
        public static void MusicList(string Cookie,long id,string symbol)
        {
            HttpClient httpclient = new HttpClient(); //建立新的HttpClient实例
            httpclient.DefaultRequestHeaders.Add("cookie", Cookie); //请求头加入Cookie
            string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0"; //定义UA变量
            httpclient.DefaultRequestHeaders.Add("user-agent", ua); //加入UA头
            var Uri = new Uri("http://server2.odtm.tech:3000/user/playlist?uid="+id.ToString()); //将网址转为Uri
            HttpResponseMessage response = httpclient.GetAsync(Uri).Result;  //创建一个HttpResponseMessage容器
            if (response.StatusCode == HttpStatusCode.OK)
            {
                String MusicListJson = response.Content.ReadAsStringAsync().Result; //获取服务端返回JSON内容
                JObject Json = JObject.Parse(MusicListJson);
                if ((int)Json["code"] == 200) //如果服务器返回状态码200，则表示正常
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    int listnumber = Json["playlist"].Count();  //定义ListNumber变量（歌单数量）
                    for (int i = 0; listnumber > i; ++i) //进行条件循环，定义i为循环次数储存器，当已循环次数小于歌单数量时进行循环操作
                    {
                        Console.WriteLine("[Info]["+i+"]"+"歌单名称:"+(string)Json["playlist"][i]["name"]+" 创建者:"+ (string)Json["playlist"][i]["creator"]["nickname"]); //输出歌单信息
                    }
                    Console.WriteLine("[Info]请选择您要下载的歌单编号,输入main返回主菜单");
                    string numbertemp = Console.ReadLine(); //NumberTemp变量用于临时储存用户键入的数据
                    if (IsNumber(numbertemp) == true)
                    {
                        int ListNumber = int.Parse(numbertemp); //定义要下载的歌单编号
                        long ListID = (long)Json["playlist"][ListNumber]["id"];
                        var ListInfoUri = new Uri("http://server2.odtm.tech:3000/playlist/detail?id="+ListID.ToString()); //定义歌单详细信息Uri
                        HttpResponseMessage response1 = httpclient.GetAsync(ListInfoUri).Result;
                        if(response1.StatusCode == HttpStatusCode.OK) //判断服务器是否正常连接
                        { 
                            //如果返回状态码正常，则进行下一步操作
                            string listinfo = response1.Content.ReadAsStringAsync().Result;
                            JObject json2 = JObject.Parse(listinfo);
                            int ServerStatcode = (int)json2["code"];
                            if(ServerStatcode == 200)
                            {
                                Console.WriteLine("[Info]准备下载歌单:"+(string)json2["playlist"]["name"]+" 创建者:"+(string)json2["playlist"]["creator"]["nickname"]);
                                int MusicNumber = (int)json2["playlist"]["tracks"].Count(); //定义歌单音乐数量变量
                                Console.WriteLine("[Info]当前歌单共有:" + MusicNumber.ToString() + "首歌");
                                for (int i = 0; MusicNumber > i; ++i)
                                {
                                    SingerName = null;
                                    string MusicName = (string)json2["playlist"]["tracks"][i]["name"]; //定义歌曲名称变量
                                    UInt32 MusicID = (UInt32)json2["playlist"]["tracks"][i]["id"]; //定义歌曲的ID
                                    var SongInfoUri = new Uri("http://server2.odtm.tech:3000/song/url?id=" + MusicID.ToString());
                                    HttpResponseMessage response3 = httpclient.GetAsync(SongInfoUri).Result;
                                    string SongDownInfo = response3.Content.ReadAsStringAsync().Result;
                                    JObject SongDownInfoJson = JObject.Parse(SongDownInfo); //解析下载地址JSON
                                    string SongDownURL = (string)SongDownInfoJson["data"][0]["url"]; //最终的歌曲下载地址
                                    int SingerNumber = (int)json2["playlist"]["tracks"][i]["ar"].Count();
                                    SingerName = null;
                                    if (SingerNumber == 1) //判断歌手列表内是否只有一位歌手
                                    {
                                        //如果是
                                        SingerName = (string)json2["playlist"]["tracks"][i]["ar"][0]["name"];
                                    }
                                    else
                                    {
                                        //如果不只有一位
                                        for (int i2 = 0;SingerNumber>i2;++i2) //i2为循环次数储存变量，当循环次数大于歌手数量，停止循环
                                        {
                                            if (i2==SingerNumber-1)
                                            {
                                                SingerName = SingerName + (string)json2["playlist"]["tracks"][i]["ar"][i2]["name"];
                                            }
                                            else
                                            {
                                                SingerName = SingerName+ (string)json2["playlist"]["tracks"][i]["ar"][i2]["name"]+",";
                                            }
                                        }
                                    }

                                    MusicDownload(SongDownURL, MusicName,SingerName,symbol,Cookie);


                                }

                                Console.WriteLine("[Info]下载完成，按任意键返回主菜单");
                                Console.ReadKey();
                                NeteaseMusicGet.Program.NeteaseMusicGet();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("[Err]该歌单可能已经被删除");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("[Info]按任意键返回主菜单");
                                Console.ReadKey();
                                NeteaseMusicGet.Program.NeteaseMusicGet();
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[Err]服务器连接失败，错误码:"+response1.StatusCode.ToString());
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("[Info]按任意键返回主菜单");
                            Console.ReadKey();
                            NeteaseMusicGet.Program.NeteaseMusicGet();
                        }
                    }
                    else
                    {
                        if (numbertemp == "")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[Err]请填入正确的编号!");
                            MusicList(Cookie, id,symbol);
                        }

                        if (numbertemp.Trim() == string.Empty)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[Err]请填入正确的编号!");
                            MusicList(Cookie, id,symbol);
                        }

                        if(numbertemp == "main")
                        {
                            NeteaseMusicGet.Program.NeteaseMusicGet(); //返回主菜单
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Err]服务器返回错误，错误代码:" + (String)Json["code"]);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("按任意键继续");
                    NeteaseMusicGet.Program.NeteaseMusicGet();
                }

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Err]无法连接服务器，返回状态码:"+response.StatusCode.ToString());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[Info]请按任意键继续");
                Console.ReadKey();
                return;
            }
        }
    }

}
