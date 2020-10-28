using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace EveryThingSampleTools.UWP.NetdiskOpenAPI
{
    internal static class NetdiskOpenAPI
    {
        public static class UrlEncodeToUpper
        {
            public static string UrlDecode(string str)
            {
                return Uri.UnescapeDataString(str);
            }
            public static string UrlEncode(string str)
            {
                return UrlEncode(str, System.Text.Encoding.UTF8);
            }
            public static string UrlEncode(string str, System.Text.Encoding e)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return str;
                }
                return System.Text.Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
            }
            private static byte[] UrlEncodeToBytes(string str, System.Text.Encoding e)
            {
                if (str == null)
                {
                    return null;
                }
                byte[] bytes = e.GetBytes(str);
                return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
            }
            private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
            {
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < count; i++)
                {
                    char ch = (char)bytes[offset + i];
                    //if (ch == ' ')
                    //{
                    //    num++;
                    //}
                    //else 
                    if (!IsSafe(ch))
                    {
                        num2++;
                    }
                }
                if ((!alwaysCreateReturnValue && (num == 0)) && (num2 == 0))
                {
                    return bytes;
                }
                byte[] buffer = new byte[count + (num2 * 2)];
                int num4 = 0;
                for (int j = 0; j < count; j++)
                {
                    byte num6 = bytes[offset + j];
                    char ch2 = (char)num6;
                    if (IsSafe(ch2))
                    {
                        buffer[num4++] = num6;
                    }
                    //else if (ch2 == ' ')
                    //{
                    //    buffer[num4++] = 43;
                    //}
                    else
                    {
                        buffer[num4++] = 37;
                        buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                        buffer[num4++] = (byte)IntToHex(num6 & 15);
                    }
                }
                return buffer;
            }
            private static bool IsSafe(char ch)
            {
                if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
                {
                    return true;
                }
                switch (ch)
                {
                    case '(':
                    case ')':
                    case '\'':
                    case '*':
                    case '-':
                    case '.':
                    case '_':
                    case '!':
                        return true;
                }
                return false;
            }
            private static char IntToHex(int n)
            {
                if (n <= 9)
                {
                    return (char)(n + 48);
                }
                return (char)((n - 10) + 65);
            }
        }
        public class Request
        {
            public Request(string access_token)
            {
                this.access_token = access_token;
            }

            public string access_token { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class NetdiskOpen
        {
            /// <summary>
            /// 
            /// </summary>
            public string request_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int errno { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int error_code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string error_msg { get; set; }
            /// <summary>
            /// UserAgent
            /// </summary>
            public static string UserAgent { get; set; } = "xpan";
            /// <summary>
            /// 
            /// </summary>
            public static bool IsProxy { get; set; } = false;
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <param name="headerCollection"></param>
            /// <returns></returns>
            public static T Request<T>(string url, WebHeaderCollection headerCollection) where T : class
            {
                return request<T>(url, null, headerCollection);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <param name="post"></param>
            /// <returns></returns>
            public static T Request<T>(string url, string post) where T : class
            {
                return request<T>(url, post, null);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <param name="post"></param>
            /// <param name="headerCollection"></param>
            /// <returns></returns>
            public static T Request<T>(string url, string post, WebHeaderCollection headerCollection) where T : class
            {
                return request<T>(url, post, headerCollection);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <returns></returns>
            public static Windows.Foundation.IAsyncOperation<T> RequestAsync<T>(string url) where T : class
            {
                return requestAsync<T>(url, null, null);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <param name="post"></param>
            /// <returns></returns>
            public static Windows.Foundation.IAsyncOperation<T> RequestAsync<T>(string url, string post) where T : class
            {
                return requestAsync<T>(url, post, null);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <param name="headerCollection"></param>
            /// <returns></returns>
            public static Windows.Foundation.IAsyncOperation<T> RequestAsync<T>(string url, WebHeaderCollection headerCollection) where T : class
            {
                return requestAsync<T>(url, null, headerCollection);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <param name="post"></param>
            /// <param name="headerCollection"></param>
            /// <returns></returns>
            public static Windows.Foundation.IAsyncOperation<T> RequestAsync<T>(string url, string post, WebHeaderCollection headerCollection) where T : class
            {
                return requestAsync<T>(url, post, headerCollection);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <returns></returns>
            public static T Request<T>(string url) where T : class
            {
                return request<T>(url, null, null);
            }

            private static Windows.Foundation.IAsyncOperation<T> requestAsync<T>(string url, string post = null, WebHeaderCollection headerCollection = null) where T : class
            {
                return AsyncInfo.Run(_ =>
                Task.Run(() =>
                {
                    return Task.FromResult(request<T>(url, post, headerCollection));
                })
                );
            }
            private static T request<T>(string url, string post = null, WebHeaderCollection headerCollection = null) where T : class
            {
                System.Net.WebRequest request = null;
                System.Net.WebResponse response = null;
                object res = null;
                try
                {
                    var uri = url;
                    request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
                    //  request.AllowAutoRedirect = true;
                    request.Method = "GET";
                    request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";

                    request.Headers[HttpRequestHeader.Accept] = "*/*";
                    request.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    if (headerCollection != null)
                        foreach (var key in headerCollection.AllKeys)
                        {
                            request.Headers[key] = headerCollection[key];
                        }
                    if (!IsProxy)
                        request.Proxy = null;
                    if (post != null)
                    {
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.Method = "POST";
                        //发送数据
                        string postData = string.Format(post);
                        var encoding = new UTF8Encoding();
                        byte[] bytepostData = encoding.GetBytes(postData);
                        request.Headers[HttpRequestHeader.ContentLength] = bytepostData.Length.ToString();
                        System.IO.Stream requestStm = request.GetRequestStream();
                        requestStm.Write(bytepostData, 0, bytepostData.Length);
                        requestStm.Dispose();
                    }
                    //响应
                    string text = string.Empty;
                    using (response = request.GetResponse() as System.Net.HttpWebResponse)
                    {

                        Type type = typeof(T);
                        using (System.IO.Stream responseStm = response.GetResponseStream())
                        {
                            var bys = new byte[1024];
                            MemoryStream outBuffer = new MemoryStream();
                            while (true)
                            {
                                int bytesRead = responseStm.Read(bys, 0, bys.Length);
                                if (bytesRead <= 0)
                                    break;
                                else
                                    outBuffer.Write(bys, 0, bytesRead);
                            }
                            bys = outBuffer.ToArray();
                            if (typeof(MemoryStream) == type)
                            {
                                res = outBuffer;
                                return (T)res;
                            }
                            else if (typeof(byte[]) == type)
                            {
                                res = bys;
                                return (T)res;
                            }
                            else
                            {
                                text = Encoding.UTF8.GetString(Tools.ZipHelper.Decompress(bys));
                            }
                        }
                        if (typeof(string) == type)
                        {
                            res = text;
                            return (T)res;
                        }
                        return ParseFromString<T>(text);
                    }
                }
                catch (Exception ex)
                {
                }
                return (T)res;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="url"></param>
            /// <returns></returns>
            public static async Task<WriteableBitmap> GetWriteableBitmapAsync(string url)
            {
                System.Net.HttpWebRequest request = null;
                System.Net.HttpWebResponse response = null;
                try
                {
                    var uri = url;
                    request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
                    //  request.AllowAutoRedirect = true;
                    request.Method = "GET";
                    request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";

                    request.Headers[HttpRequestHeader.Accept] = "*/*";
                    request.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    if (!IsProxy)
                        request.Proxy = null;
                    using (response = await request.GetResponseAsync() as System.Net.HttpWebResponse)
                    {

                        using (System.IO.Stream responseStm = response.GetResponseStream())
                        {
                            byte[] _data = new byte[2048]; int length = 0;
                            MemoryStream _ms = new MemoryStream();
                            while ((length = responseStm.Read(_data, 0, _data.Length)) > 0)
                            {
                                _ms.Write(_data, 0, length);
                            }
                            _ms.Seek(0, SeekOrigin.Begin);
                            var ras = _ms.AsRandomAccessStream();
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(ras);
                            var provider = await decoder.GetPixelDataAsync();
                            byte[] buffer = provider.DetachPixelData();
                            var bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                            await bitmap.PixelBuffer.AsStream().WriteAsync(buffer, 0, buffer.Length);
                            _ms.Dispose();
                            return bitmap;
                        }
                    }
                }
                catch { }
                return null;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="txt"></param>
            /// <returns></returns>
            public static T ParseFromString<T>(string txt)
            {
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(txt);
                }
                catch
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                    var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(txt));
                    var data = (T)serializer.ReadObject(ms);
                    return data;
                }
            }
        }
        /// <summary>
        /// 基础能力接口
        /// </summary>
        public class NetdiskBasic
        {
            private NetdiskBasic() { }
            /// <summary>
            /// 
            /// </summary>
            public class UserInfo : NetdiskOpen
            {
                /// <summary>
                /// 百度账号
                /// </summary>
                public string baidu_name { get; set; }
                /// <summary>
                /// 网盘账号
                /// </summary>
                public string netdisk_name { get; set; }
                /// <summary>
                /// 头像地址
                /// </summary>
                public string avatar_url { get; set; }
                /// <summary>
                /// 头像地址
                /// </summary>
                public int vip_type { get; set; }
                /// <summary>
                /// 用户ID
                /// </summary>
                public long uk { get; set; }
                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<UserInfo> GetUserInfoAsync(string access_token)
                {
                    var url = "https://pan.baidu.com/rest/2.0/xpan/nas?method=uinfo&access_token=" + access_token;
                    return RequestAsync<UserInfo>(url);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public static UserInfo GetUserInfo(string access_token)
                {
                    var url = "https://pan.baidu.com/rest/2.0/xpan/nas?method=uinfo&access_token=" + access_token;
                    return Request<UserInfo>(url);
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public class QuotaInfo : NetdiskOpen
            {
                /// <summary>
                /// 总空间大小，单位B
                /// </summary>
                public long total { get; set; }
                /// <summary>
                /// 已使用大小，单位B
                /// </summary>
                public long used { get; set; }
                /// <summary>
                /// 
                /// </summary>
                /// <returns>QuotaInfo</returns>
                public static Windows.Foundation.IAsyncOperation<QuotaInfo> GetQuotaInfoAsync(string access_token)
                {
                    var url = "https://pan.baidu.com/api/quota?chckfree=1&checkexpire=1&access_token=" + access_token;
                    return RequestAsync<QuotaInfo>(url);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public static QuotaInfo GetQuotaInfo(string access_token)
                {
                    var url = "https://pan.baidu.com/api/quota?chckfree=1&checkexpire=1&access_token=" + access_token;
                    return Request<QuotaInfo>(url);
                }
            }
            /// <summary>
            /// 只有请求参数带WEB且该条目分类为图片时，该KEY才存在，包含三个尺寸的缩略图URL
            /// </summary>
            public class Thumbs
            {
                /// <summary>
                /// 
                /// </summary>
                public string icon { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string url4 { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string url3 { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string url2 { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string url1 { get; set; }
            }
            /// <summary>
            /// 
            /// </summary>
            public class NetdiskFileInfo
            {
                /// <summary>
                /// 文件类型，1 视频、2 音频、3 图片、4 文档、5 应用、6 其他、7 种子
                /// </summary>
                public int category { get; set; }
                /// <summary>
                /// 文件在云端的唯一标识ID
                /// </summary>
                public ulong fs_id { get; set; }
                /// <summary>
                /// 文件在服务器创建时间
                /// </summary>
                public long server_ctime { get; set; }
                /// <summary>
                /// 文件在客户端修改时间
                /// </summary>
                public long local_mtime { get; set; }
                /// <summary>
                /// 文件大小B
                /// </summary>
                public ulong size { get; set; }
                /// <summary>
                /// 是否目录，0 文件、1 目录
                /// </summary>
                public int isdir { get; set; }
                /// <summary>
                /// 文件的绝对路径
                /// </summary>
                public string path { get; set; }
                /// <summary>
                /// 文件在客户端创建时间
                /// </summary>
                public long local_ctime { get; set; }
                /// <summary>
                /// 文件名称
                /// </summary>
                public string server_filename { get; set; }
                /// <summary>
                /// 文件在服务器修改时间
                /// </summary>
                public long server_mtime { get; set; }
                /// <summary>
                /// 只有请求参数带WEB且该条目分类为图片时，该KEY才存在，包含三个尺寸的缩略图URL
                /// </summary>
                public Thumbs thumbs { get; set; }
                /// <summary>
                /// 文件的md5值，只有是文件类型时，该KEY才存在
                /// </summary>
                public string md5 { get; set; }
            }
            /// <summary>
            /// 文件列表
            /// </summary>
            public class FileList : NetdiskOpen
            {
                /// <summary>
                /// 
                /// </summary>
                public enum OrderName
                {
                    /// <summary>
                    /// 大小
                    /// </summary>
                    size,
                    /// <summary>
                    /// 时间
                    /// </summary>
                    time,
                    /// <summary>
                    /// 文件名
                    /// </summary>
                    name,
                }
                /// <summary>
                /// 请求参数
                /// </summary>
                public class FileListRequest : Request
                {
                    public FileListRequest(string token) : base(token)
                    {

                    }
                    /// <summary>
                    /// 需要list的目录，以/开头的绝对路径，默认为 /
                    /// </summary>
                    public string dir = "/";
                    /// <summary>
                    /// 排序字段，time表示先按文件类型排序，后按修改时间排序，name表示先按文件类型排序，后按文件名称排序，size表示先按文件类型排序， 后按文件大小排序，默认为name
                    /// </summary>
                    public OrderName order = OrderName.size;
                    /// <summary>
                    /// 该KEY存在为降序
                    /// </summary>
                    public bool desc = false;
                    /// <summary>
                    /// 起始位置， 从0开始
                    /// </summary>
                    public int start = 0;
                    /// <summary>
                    /// 每页条目数，默认为1000，最大值为10000
                    /// </summary>
                    public int limit = 1000;
                    /// <summary>
                    /// 是否只返回文件夹，1 只返回目录条目，且属性只返回path字段、0 返回所有
                    /// </summary>
                    public int folder = 0;
                    /// <summary>
                    /// 默认0，为1时返回缩略图信息
                    /// </summary>
                    public int web = 0;
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileList> GetFileListAsync(FileListRequest request)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=list&dir={Uri.EscapeDataString(request.dir)}&order={request.order.ToString()}&start={request.start}&limit={request.limit}&folder={request.folder}&web={request.web}&access_token={request.access_token}";
                    if (request.desc)
                        url += "&desc=1";

                    return RequestAsync<FileList>(url);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static FileList GetFileList(FileListRequest request)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=list&dir={Uri.EscapeDataString(request.dir)}&order={request.order.ToString()}&start={request.start}&limit={request.limit}&folder={request.folder}&web={request.web}&access_token={request.access_token}";
                    if (request.desc)
                        url += "&desc=1";

                    return Request<FileList>(url);
                }

                /// <summary>
                /// 
                /// </summary>
                public class ListItem : NetdiskFileInfo
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public int dir_empty { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long oper_id { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public int share { get; set; }
                    /// <summary>
                    /// 是否目录，0 文件、1 目录
                    /// </summary>
                    public int empty { get; set; }
                }
                /// <summary>
                /// 
                /// </summary>
                public string guid_info { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public IList<ListItem> list { get; set; }
            }
            /// <summary>
            /// 
            /// </summary>
            public class SearchData : NetdiskOpen
            {
                /// <summary>
                /// 请求参数
                /// </summary>
                public class SearchRequest:Request
                {
                    /// <summary>
                    /// 默认0，为1时返回缩略图信息
                    /// </summary>
                    public int web = 0;
                    /// <summary>
                    /// 搜索关键字
                    /// </summary>
                    public string key;
                    /// <summary>
                    /// 搜索目录，默认根目录
                    /// </summary>
                    public string dir = "/";
                    /// <summary>
                    /// 是否递归，1 递归、0 不递归，默认0
                    /// </summary>
                    public int recursion = 0;
                    /// <summary>
                    /// 页数，从1开始，缺省则返回所有条目
                    /// </summary>
                    public int page = 0;
                    /// <summary>
                    /// 每页条目数，默认为1000，最大值为1000
                    /// </summary>
                    public int num = 1000;
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="key"></param>
                    /// <param name="access_token"></param>
                    public SearchRequest(string key,string access_token):base(access_token)
                    {
                        this.key = key;
                    }
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<SearchData> GetFileListAsync(SearchRequest request)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=search&key={request.key}&recursion={request.recursion}&web={request.web}&dir={request.dir}&access_token={request.access_token}";
                    if (request.page > 0)
                        url += "&page" + request.page;
                    return RequestAsync<SearchData>(url);
                }
                public static SearchData GetFileList(SearchRequest request)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=search&key={request.key}&recursion={request.recursion}&web={request.web}&dir={request.dir}&access_token={request.access_token}";
                    if (request.page > 0)
                        url += "&page" + request.page;
                    return Request<SearchData>(url);
                }
                /// <summary>
                /// 文件列表
                /// </summary>
                public IList<NetdiskFileInfo> list { get; set; }
            }
            /// <summary>
            /// 
            /// </summary>
            public class Filemetas : NetdiskOpen
            {
                /// <summary>
                /// 
                /// </summary>
                public class FilemetasRequest:Request
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="fsids"></param>
                    /// <param name="access_token"></param>
                    public FilemetasRequest(ulong[] fsids, string access_token):base(access_token)
                    {
                        this.fsids = fsids;
                    }
                    /// <summary>
                    /// 查询共享目录文件时需要，格式: /《share》uk-fsid，uk对应共享目录创建者ID，fsid对应共享目录的fsid
                    /// </summary>
                    public string path = null;
                    /// <summary>
                    /// fsid数组，数组中元素类型为uint64，大小上限100
                    /// </summary>
                    public ulong[] fsids;
                    /// <summary>
                    /// 是否需要缩略图地址，0 否、1 是，默认0
                    /// </summary>
                    public int thumb = 0;
                    /// <summary>
                    /// 是否需要文件下载地址dlink，0 否、1 是，默认0
                    /// </summary>
                    public int dlink = 0;
                    /// <summary>
                    /// 图片是否需要拍摄时间、原图分辨率等其他信息，0 否、1 是，默认0
                    /// </summary>
                    public int extra = 0;
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<Filemetas> GetFilemetasAsync(FilemetasRequest request)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/multimedia?method=filemetas&path={request.path}&fsids=[{string.Join(',', request.fsids)}]&thumb={request.thumb}&dlink=1&extra={request.extra}&access_token={request.access_token}";
                    return RequestAsync<Filemetas>(url);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static Filemetas GetFilemetas(FilemetasRequest request)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/multimedia?method=filemetas&path={request.path}&fsids=[{string.Join(',', request.fsids)}]&thumb={request.thumb}&dlink=1&extra={request.extra}&access_token={request.access_token}";
                    return Request<Filemetas>(url);
                }
                /// <summary>
                /// 
                /// </summary>
                public IList<ListItem> list { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public class ListItem : NetdiskFileInfo
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public string dlink { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long oper_id { get; set; }
                }
            }
            /// <summary>
            /// 文件操作
            /// </summary>
            public class FileManager : NetdiskOpen
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="fileManagerRequest"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileManager> GetFileManagerAsync(FileManagerRequest fileManagerRequest)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=filemanager&access_token={fileManagerRequest.access_token}&opera={fileManagerRequest.opera.ToString()}";
                    var post = $"async={fileManagerRequest.async}&filelist={UrlEncodeToUpper.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(fileManagerRequest.filelist))}";
                    return RequestAsync<FileManager>(url, post);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="fileManagerRequest"></param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static FileManager GetFileManager(FileManagerRequest fileManagerRequest)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=filemanager&access_token={fileManagerRequest.access_token}&opera={fileManagerRequest.opera.ToString()}";
                    var post = $"async={fileManagerRequest.async}&filelist={UrlEncodeToUpper.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(fileManagerRequest.filelist))}";
                    return Request<FileManager>(url, post);
                }
                /// <summary>
                /// 异步任务ID，async=2时返回
                /// </summary>
                public long taskid { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public class FileManagerRequest:Request
                {
                    /// <summary>
                    /// copy、move、rename、delete
                    /// </summary>
                    public enum Opera
                    {
                        /// <summary>
                        /// 复制
                        /// </summary>
                        copy,
                        /// <summary>
                        /// 移动
                        /// </summary>
                        move,
                        /// <summary>
                        /// 重命名
                        /// </summary>
                        rename,
                        /// <summary>
                        /// 删除
                        /// </summary>
                        delete
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    public class FilelistItem
                    {
                        /// <summary>
                        /// 文件绝对路径
                        /// </summary>
                        public string path { get; set; }
                        /// <summary>
                        /// 目标目录
                        /// </summary>
                        public string dest { get; set; }
                        /// <summary>
                        /// 文件新名字
                        /// </summary>
                        public string newname { get; set; }
                        /// <summary>
                        /// 遇到重复文件的处理策略，默认，直接返回失败
                        /// </summary>
                        public Ondup ondup { get; set; }
                        /// <summary>
                        /// 遇到重复文件的处理策略
                        /// </summary>
                        public enum Ondup
                        {
                            /// <summary>
                            /// 直接返回失败
                            /// </summary>
                            fail,
                            /// <summary>
                            /// 重命名文件
                            /// </summary>
                            newcopy,
                            /// <summary>
                            /// 覆盖
                            /// </summary>
                            overwrite,
                            /// <summary>
                            /// 跳过
                            /// </summary>
                            skip,
                        }
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="opera">copy、move、rename、delete</param>
                    /// <param name="filelist">文件列表</param>
                    /// <param name="async">0 同步、1 自适应、2 异步</param>
                    /// <param name="access_token"></param>
                    public FileManagerRequest(Opera opera, IList<FilelistItem> filelist, int async, string access_token):base(access_token)
                    {
                        this.opera = opera;
                        this.filelist = filelist;
                        this.async = async;
                    }
                    /// <summary>
                    /// copy、move、rename、delete
                    /// </summary>
                    public Opera opera { get; }
                    /// <summary>
                    /// 文件列表
                    /// </summary>
                    public IList<FilelistItem> filelist { get; }
                    /// <summary>
                    /// 0 同步、1 自适应、2 异步
                    /// </summary>
                    public int async { get; }
                }
                /// <summary>
                /// 文件信息
                /// </summary>
                public class Info
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public int errno { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string path { get; set; }
                }




                /// <summary>
                /// 
                /// </summary>
                public class TaskListItem
                {
                    /// <summary>
                    /// 错误码
                    /// </summary>
                    public int error_code { get; set; }
                }
                /// <summary>
                /// 
                /// </summary>
                public IList<TaskListItem> list { get; set; }
                /// <summary>
                /// success,fail
                /// </summary>
                public string status { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int task_errno { get; set; }
                /// <summary>
                /// max 100
                /// </summary>
                public int progress { get; set; }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="pathList"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileManager> DeleteFileAsync(IList<string> pathList, string access_token)
                {
                    var str = Newtonsoft.Json.JsonConvert.SerializeObject(pathList);
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=filemanager&access_token={access_token}&opera=delete";
                    var post = $"filelist={Uri.EscapeDataString(str)}";
                    return RequestAsync<FileManager>(url, post);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="pathList"></param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static FileManager DeleteFile(IList<string> pathList, string access_token)
                {
                    var str = Newtonsoft.Json.JsonConvert.SerializeObject(pathList);
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=filemanager&access_token={access_token}&opera=delete";
                    var post = $"filelist={Uri.EscapeDataString(str)}";
                    return Request<FileManager>(url, post);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="files"></param>
                /// <param name="async"></param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileManager> CopyFileAsync(IList<FileManagerRequest.FilelistItem> files, int async, string access_token)
                {
                    var request = new FileManagerRequest(FileManagerRequest.Opera.copy, files, async, access_token);
                    return GetFileManagerAsync(request);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="files"></param>
                /// <param name="async"></param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static FileManager CopyFile(IList<FileManagerRequest.FilelistItem> files, int async, string access_token)
                {
                    var request = new FileManagerRequest(FileManagerRequest.Opera.copy, files, async, access_token);
                    return GetFileManager(request);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="files"></param>
                /// <param name="async">0 同步、1 自适应、2 异步</param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileManager> MoveFileAsync(IList<FileManagerRequest.FilelistItem> files, int async, string access_token)
                {
                    var request = new FileManagerRequest(FileManagerRequest.Opera.move, files, async,access_token);
                    return GetFileManagerAsync(request);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="files"></param>
                /// <param name="async">0 同步、1 自适应、2 异步</param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static FileManager MoveFile(IList<FileManagerRequest.FilelistItem> files, int async, string access_token)
                {
                    var request = new FileManagerRequest(FileManagerRequest.Opera.move, files, async,access_token);
                    return GetFileManager(request);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="files"></param>
                /// <param name="async">0 同步、1 自适应、2 异步</param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileManager> RenameFileAsync(IList<FileManagerRequest.FilelistItem> files, int async, string access_token)
                {
                    var request = new FileManagerRequest(FileManagerRequest.Opera.rename, files, async, access_token);
                    return GetFileManagerAsync(request);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="files"></param>
                /// <param name="async">0 同步、1 自适应、2 异步</param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static FileManager RenameFile(IList<FileManagerRequest.FilelistItem> files, int async, string access_token)
                {
                    var request = new FileManagerRequest(FileManagerRequest.Opera.rename, files, async,access_token);
                    return GetFileManager(request);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="taskId"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileManager> GetTaskStatusAsync(long taskId, string access_token)
                {
                    var url = $"http://pan.baidu.com/api/taskquery?&taskid={taskId}&access_token={access_token}";
                    return RequestAsync<FileManager>(url);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="taskId"></param>
                /// <param name="access_token"></param>
                /// <returns></returns>
                public static FileManager GetTaskStatus(long taskId, string access_token)
                {
                    var url = $"http://pan.baidu.com/api/taskquery?&taskid={taskId}&access_token={access_token}";
                    return Request<FileManager>(url);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static class NetdiskVideo
        {
            /// <summary>
            /// 
            /// </summary>
            public class VideoStream : NetdiskOpen
            {
                /// <summary>
                /// 
                /// </summary>
                public enum M3U8Type
                {
                    /// <summary>
                    /// 视频ts
                    /// </summary>
                    M3U8_AUTO_480,
                    /// <summary>
                    /// 视频flv
                    /// </summary>
                    M3U8_FLV_264_480,
                    /// <summary>
                    /// 音频mp3
                    /// </summary>
                    M3U8_MP3_128,
                    /// <summary>
                    /// 音频ts
                    /// </summary>
                    M3U8_HLS_MP3_128,
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="path"></param>
                /// <param name="m3U8Type"></param>
                /// <param name="access_token"></param>
                /// <param name="adToken"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<VideoStream> GetAdTokenAsync(string path, M3U8Type m3U8Type, string access_token,string adToken = null)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=streaming&access_token={access_token}&path={Uri.EscapeDataString(path)}&type={m3U8Type.ToString()}&nom3u8=1";
                    if (adToken != null)
                        url += "&adToken=" + Uri.EscapeDataString(adToken);
                    return RequestAsync<VideoStream>(url);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="path"></param>
                /// <param name="m3U8Type"></param>
                /// <param name="access_token"></param>
                /// <param name="adToken"></param>
                /// <returns></returns>
                public static VideoStream GetAdToken(string path, M3U8Type m3U8Type, string access_token, string adToken = null)
                {
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=streaming&access_token={access_token}&path={Uri.EscapeDataString(path)}&type={m3U8Type.ToString()}&nom3u8=1";
                    if (adToken != null)
                        url += "&adToken=" + Uri.EscapeDataString(adToken);
                    return Request<VideoStream>(url);
                }
                /// <summary>
                /// 广告播放时长
                /// </summary>
                public int adTime { get; set; }
                /// <summary>
                /// 加载广告后返回的合法token，有效期10小时
                /// </summary>
                public string adToken { get; set; }
                /// <summary>
                /// 广告播放最短时长
                /// </summary>
                public int ltime { get; set; }
            }
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        public static class UploadFile
        {
            /// <summary>
            /// 
            /// </summary>
            public class UploadFileRequest:Request
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="path"></param>
                /// <param name="isdir"></param>
                /// <param name="access_token"></param>
                public UploadFileRequest(string path, int isdir, string access_token):base(access_token)
                {
                    this.path = path;
                    this.isdir = isdir;
                    this.block_list = new string[0];
                    rtype = 1;
                }
                /// <summary>
                /// 
                /// </summary>
                public long size { get; set; }
                /// <summary>
                ///  绝对路径
                /// </summary>
                public string path { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int isdir { get; set; }
                /// <summary>
                /// 文件命名策略，默认1; 0 为不重命名，返回冲突;1 为只要path冲突即重命名;2 为path冲突且block_list不同才重命名;3 为覆盖
                /// </summary>
                public int rtype { get; set; }
                /// <summary>
                /// uploadid， 非空表示通过superfile2上传
                /// </summary>
                public string uploadid { get; set; }
                /// <summary>
                /// 文件各分片MD5的json串
                /// MD5对应superfile2返回的md5，且要按照序号顺序排列
                /// </summary>
                public IList<string> block_list { get; set; }
            }
            /// <summary>
            /// 预上传
            /// </summary>
            public class FilePreCreate : NetdiskOpen
            {
                /// <summary>
                /// 
                /// </summary>
                public class FilePreCreateRequest : UploadFileRequest
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="path"></param>
                    /// <param name="isdir"></param>
                    /// <param name="access_token"></param>
                    public FilePreCreateRequest(string path, int isdir, string access_token) : base(path, isdir, access_token)
                    {
                    }
                    /// <summary>
                    /// 文件MD5
                    /// </summary>
                    public string content_md5 { get; set; }
                    /// <summary>
                    /// 文件校验段的MD5，校验段对应文件前256KB
                    /// </summary>
                    public string slice_md5 { get; set; }
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FilePreCreate> GetFilePreCreateAsync(FilePreCreateRequest request)
                {
                    var url = $"http://pan.baidu.com/rest/2.0/xpan/file?method=precreate&access_token={request.access_token}";
                    var post = $"path={Uri.EscapeDataString(request.path)}&isdir={request.isdir}&autoinit=1&rtype={request.rtype}&block_list={Newtonsoft.Json.JsonConvert.SerializeObject(request.block_list)}&content-md5={request.content_md5}&slice-md5={request.slice_md5}";
                    if (request.size > 0)
                        post += "&size=" + request.size;
                    return RequestAsync<FilePreCreate>(url, post);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static FilePreCreate GetFilePreCreate(FilePreCreateRequest request)
                {
                    var url = $"http://pan.baidu.com/rest/2.0/xpan/file?method=precreate&access_token={request.access_token}";
                    var post = $"path={Uri.EscapeDataString(request.path)}&isdir={request.isdir}&autoinit=1&rtype={request.rtype}&block_list={Newtonsoft.Json.JsonConvert.SerializeObject(request.block_list)}&content-md5={request.content_md5}&slice-md5={request.slice_md5}";
                    if (request.size > 0)
                        post += "&size=" + request.size;
                    return Request<FilePreCreate>(url, post);
                }
                /// <summary>
                /// 文件的绝对路径
                /// </summary>
                public string path { get; set; }
                /// <summary>
                /// 上传id
                /// </summary>
                public string uploadid { get; set; }
                /// <summary>
                /// 返回类型，1 文件在云端不存在、2 文件在云端已存在
                /// </summary>
                public int return_type { get; set; }
                /// <summary>
                /// block_list为空时，等价于[0]; [0,1,2]需要上传3个分片
                /// </summary>
                public IList<int> block_list { get; set; }
            }
            /// <summary>
            /// 分片上传
            /// </summary>
            public class FileSuperFile2
            {

            }

            /// <summary>
            /// 可以使用该接口创建文件夹
            /// </summary>
            public class FileCreate : NetdiskOpen
            {
                /// <summary>
                /// 
                /// </summary>
                public long fs_id { get; set; }
                /// <summary>
                ///  绝对路径
                /// </summary>
                public string path { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long ctime { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long mtime { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int isdir { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string name { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int category { get; set; }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static Windows.Foundation.IAsyncOperation<FileCreate> CreatNewItemAsync(FileCreateRequest request)
                {
                    string post = $"path={Uri.EscapeDataString(request.path)}&isdir={request.isdir}&block_list={Newtonsoft.Json.JsonConvert.SerializeObject(request.block_list)}&rtype={request.rtype}";
                    if (request.uploadid != null)
                        post += "&uploadid=" + request.uploadid;
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=create&access_token=" + request.access_token;
                    return RequestAsync<FileCreate>(url, post);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static FileCreate CreatNewItem(FileCreateRequest request)
                {
                    string post = $"path={Uri.EscapeDataString(request.path)}&isdir={request.isdir}&block_list={Newtonsoft.Json.JsonConvert.SerializeObject(request.block_list)}&rtype={request.rtype}";
                    if (request.uploadid != null)
                        post += "&uploadid=" + request.uploadid;
                    var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=create&access_token=" + request.access_token;
                    return Request<FileCreate>(url, post);
                }
                /// <summary>
                /// 
                /// </summary>
                public class FileCreateRequest : UploadFile.UploadFileRequest
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="path"></param>
                    /// <param name="isdir"></param>
                    /// <param name="access_token"></param>
                    public FileCreateRequest(string path, int isdir, string access_token) : base(path, isdir,access_token)
                    {
                    }
                }
            }
        }
        /// <summary>
        /// 附件
        /// </summary>
        //public class FileTransfer
        //{
        //    /// <summary>
        //    /// 创建附件
        //    /// </summary>
        //    public class ShareSet : NetdiskOpen
        //    {
        //        /// <summary>
        //        /// 附件id
        //        /// </summary>
        //        public long shareid { get; set; }
        //        /// <summary>
        //        /// 附件链接地址
        //        /// </summary>
        //        public string link { get; set; }
        //        /// <summary>
        //        /// 第三方的查看链接
        //        /// </summary>
        //        public string shorturl { get; set; }
        //        /// <summary>
        //        /// 附件链接的提取码
        //        /// </summary>
        //        public string pwd { get; set; }
        //        /// <summary>
        //        /// 附件创建者的uk
        //        /// </summary>
        //        public string uk { get; set; }
        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        /// <param name="req"></param>
        //        /// <returns></returns>
        //        public static async Task<ShareSet> GetShareSetAsync(Request req)
        //        {
        //            var url = "http://pan.baidu.com/rest/2.0/xpan/share/set?access_token=" + access_token;
        //            var post = $"schannel={req.schannel}&pwd={req.pwd}&period={req.period}&fid_list={string.Join(',' ,req.fid_list)}&channel_list=%5B%5D";

        //            return await RequestAsync<ShareSet>(url, post);
        //        }

        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        public class Request
        //        {
        //            /// <summary>
        //            /// fsid列表
        //            /// </summary>
        //            public IList<long> fid_list { get; set; }
        //            /// <summary>
        //            /// 分享途径，公开链接填0,私密链接填4
        //            /// </summary>
        //            public int schannel { get; set; }
        //            /// <summary>
        //            /// 附件链接有效期，0 永久、1 1天、7 7天，默认0
        //            /// </summary>
        //            public int period { get; set; }
        //            /// <summary>
        //            /// 密码
        //            /// </summary>
        //            public string pwd { get { return getRandomStr(4); } }
        //            private static string getRandomStr(int n, bool b = false)//b：是否有复杂字符，n：生成的字符串长度
        //            {

        //                string str = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //                if (b == true)
        //                {
        //                    str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";//复杂字符
        //                }
        //                StringBuilder SB = new StringBuilder();
        //                Random rd = new Random();
        //                for (int i = 0; i < n; i++)
        //                {
        //                    SB.Append(str.Substring(rd.Next(0, str.Length), 1));
        //                }
        //                return SB.ToString();

        //            }

        //        }
        //    }
        //}


        /// <summary>
        /// 
        /// </summary>
        public class OAuthToken
        {
            /// <summary>
            /// expires_in
            /// </summary>
            public long expires_in { get; set; }
            /// <summary>
            /// refresh_token
            /// </summary>
            public string refresh_token { get; set; }
            /// <summary>
            /// access_token
            /// </summary>
            public string access_token { get; set; }
            /// <summary>
            /// scope
            /// </summary>
            public string scope { get; set; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="code"></param>
            /// <param name="apiKey"></param>
            /// <param name="secretKey"></param>
            /// <returns></returns>
            public static Windows.Foundation.IAsyncOperation<OAuthToken> GetOAuthTokenFromCodeAsync(string code, string apiKey, string secretKey)
            {
                var url = $"https://openapi.baidu.com/oauth/2.0/token?grant_type=authorization_code&code={code}&client_id={apiKey}&client_secret={secretKey}&redirect_uri=obb";
                return NetdiskOpen.RequestAsync<OAuthToken>(url);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="code"></param>
            /// <param name="apiKey"></param>
            /// <param name="secretKey"></param>
            /// <returns></returns>
            public static OAuthToken GetOAuthTokenFromCode(string code, string apiKey, string secretKey)
            {
                var url = $"https://openapi.baidu.com/oauth/2.0/token?grant_type=authorization_code&code={code}&client_id={apiKey}&client_secret={secretKey}&redirect_uri=obb";
                return NetdiskOpen.Request<OAuthToken>(url);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="refresh"></param>
            /// <param name="apiKey"></param>
            /// <param name="secretKey"></param>
            /// <returns></returns>
            public static Windows.Foundation.IAsyncOperation<OAuthToken> GetOAuthTokenFromRefreshAsync(string refresh, string apiKey, string secretKey)
            {
                var url = $"https://openapi.baidu.com/oauth/2.0/token?grant_type=refresh_token&refresh_token={refresh}&client_id={apiKey}&client_secret={secretKey}";
                return NetdiskOpen.RequestAsync<OAuthToken>(url);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="refresh"></param>
            /// <returns></returns>
            public static OAuthToken GetOAuthTokenFromRefresh(string refresh, string apiKey, string secretKey)
            {
                var url = $"https://openapi.baidu.com/oauth/2.0/token?grant_type=refresh_token&refresh_token={refresh}&client_id={apiKey}&client_secret={secretKey}";
                return NetdiskOpen.Request<OAuthToken>(url);
            }
        }

        public class DupanShareOpenFileInfo : NetdiskOpen
        {
            public static async Task<DupanShareOpenFileInfo> GetShareResultUrlRootAsync(List<long> fidList, string access_token)
            {
                var pwd = getRandomStr(4);
                var post = $"schannel=4&pwd={pwd}&channel_list=%5B%5D&period=1&fid_list=" + Uri.EscapeDataString($"[{string.Join(",", fidList.ToArray())}]");
                var url = $"https://pan.baidu.com/share/set?access_token={access_token}";
                var res = await RequestAsync<DupanShareOpenFileInfo>(url, post);
                if (res != null)
                {
                    res.password = pwd;
                }
                return res;
            }
            public static DupanShareOpenFileInfo GetShareResultUrlRoot(List<ulong> fidList, string access_token)
            {
                var pwd = getRandomStr(4);
                var post = $"schannel=4&pwd={pwd}&channel_list=%5B%5D&period=1&fid_list=" + Uri.EscapeDataString($"[{string.Join(",", fidList.ToArray())}]");
                var url = $"https://pan.baidu.com/share/set?access_token={access_token}";
                var res = Request<DupanShareOpenFileInfo>(url, post);
                if (res != null)
                {
                    res.password = pwd;
                }
                return res;
            }
            public static async Task<DupanShareOpenFileInfo> CancelShareAsync(List<string> shareIds)
            {
                var url = $"https://pan.baidu.com/share/cancel?channel=chunlei&clienttype=0&web=1&channel=chunlei&web=1&app_id=250528&logid=&clienttype=0";
                var post = "shareid_list=" + Newtonsoft.Json.JsonConvert.SerializeObject(shareIds);
                var res = await RequestAsync<DupanShareOpenFileInfo>(url, post);
                return res;
            }
            public static string getRandomStr(int n, bool b = false)//b：是否有复杂字符，n：生成的字符串长度
            {

                string str = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                if (b == true)
                {
                    str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";//复杂字符
                }
                StringBuilder SB = new StringBuilder();
                Random rd = new Random();
                for (int i = 0; i < n; i++)
                {
                    SB.Append(str.Substring(rd.Next(0, str.Length), 1));
                }
                return SB.ToString();

            }
            public string password { get; set; }
            public string shareid { get; set; }
            public string link { get; set; }
            public string shorturl { get; set; }
            public string ctime { get; set; }
            public string expiredType { get; set; }
            public string premis { get; set; }
            public string show_msg { get; set; }
        }
    }
}