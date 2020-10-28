using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using static EveryThingSampleTools.UWP.NetdiskOpenAPI.NetdiskOpenAPI;

namespace EveryThingSampleTools.UWP.NetdiskOpenAPI
{

    /// <summary>
    /// 
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// expires_in
        /// </summary>
        public long ExpiresTimestamp { get; set; }
        /// <summary>
        /// refresh_token
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// access_token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs_id"></param>
        /// <returns></returns>
        public IAsyncOperation<Uri> GetDownloadUriAsync(ulong fs_id)
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<Uri>(() =>
                  {
                      var res = NetdiskBasic.Filemetas.GetFilemetas(new NetdiskBasic.Filemetas.FilemetasRequest(new ulong[1] { fs_id }, AccessToken));
                      if (res != null)
                      {
                          if (res.list?.Count > 0)
                          {
                              return new Uri(res.list.First().dlink + "&access_token=" + AccessToken);
                          }
                          else
                              throw new Exception("Fail");
                      }
                      else
                          throw new Exception("Net Error");
                  }
                  ));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class BaiduNetdiskAuth
    {
        private OAuth oAuth;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        public BaiduNetdiskAuth(string apiKey, string secretKey)
        {
            oAuth = new OAuth() { apiKey = apiKey, secretKey = secretKey };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Uri GetQRCodeUri()
        {
            return new Uri(oAuth.get_qr_code_url());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redirect_url"></param>
        /// <returns></returns>
        public Uri GetQRCodeUri(string redirect_url)
        {
            return new Uri(oAuth.get_qr_code_url(redirect_url));
        }
        private BaiduNetdiskAuth() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Windows.Foundation.IAsyncOperation<AuthToken> GetAuthTokenAsync(string code)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run(_ =>
                   Task.Run<AuthToken>(async () =>
                   {
                       var res = await NetdiskOpenAPI.OAuthToken.GetOAuthTokenFromCodeAsync(code, oAuth.apiKey, oAuth.secretKey);
                       if (res?.access_token == null)
                       {
                           return null;
                       }
                       return new AuthToken()
                       {
                           AccessToken = res.access_token,
                           ExpiresTimestamp = res.expires_in + EveryThingSampleTools.UWP.Tools.TimeTool.NowTimeStamp,
                           RefreshToken = res.refresh_token
                       };
                   }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="atoken"></param>
        /// <returns></returns>
        public Windows.Foundation.IAsyncOperation<AuthToken> GetAuthTokenAsync(AuthToken atoken)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run(_ =>
                   Task.Run<AuthToken>(async () =>
                   {
                       if (IsExpired(atoken))
                       {
                           var res = await NetdiskOpenAPI.OAuthToken.GetOAuthTokenFromRefreshAsync(atoken.RefreshToken, oAuth.apiKey, oAuth.secretKey);
                           if (res?.access_token == null)
                           {
                               return null;
                           }
                           return new AuthToken()
                           {
                               AccessToken = res.access_token,
                               ExpiresTimestamp = res.expires_in + EveryThingSampleTools.UWP.Tools.TimeTool.NowTimeStamp,
                               RefreshToken = res.refresh_token
                           };
                       }
                       else
                           return atoken;
                   }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="atoken"></param>
        /// <returns></returns>
        public BaiduNetdiskOpen GetBaiduNetdiskOpen(AuthToken atoken)
        {
            if (IsExpired(atoken))
                throw new Exception("Token is expired");
            return new BaiduNetdiskOpen(new OAuth() { AccessToken = atoken.AccessToken, apiKey = oAuth.apiKey, secretKey = oAuth.secretKey });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsExpired(AuthToken token)
        {
            return token.ExpiresTimestamp <= EveryThingSampleTools.UWP.Tools.TimeTool.NowTimeStamp;
        }

        /// <summary>
        /// 
        /// </summary>
        internal string token { get; private set; }
    }
    internal class OAuth
    {
        internal OAuth() { }
        /// <summary>
        /// 
        /// </summary>
        internal string get_qr_code_url(string redirect_url = "obb")
        {
            return $"https://openapi.baidu.com/oauth/2.0/authorize?response_type=code&client_id={apiKey}&scope=netdisk&redirect_uri={redirect_url}&display=popup&qrcode=1";
        }
        internal int vipType { get; set; }
        /// <summary>
        /// apiKey
        /// </summary>
        internal string apiKey { get; set; }
        /// <summary>
        /// secretKey
        /// </summary>
        internal string secretKey { get; set; }

        internal string AccessToken { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class BaiduNetdiskOpen
    {
        /// <summary>
        /// 
        /// </summary>
        public static string UserAgent
        {
            get { return NetdiskOpenAPI.NetdiskOpen.UserAgent; }
            set { NetdiskOpenAPI.NetdiskOpen.UserAgent = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static bool IsProxy { get { return NetdiskOpenAPI.NetdiskOpen.IsProxy; } set { NetdiskOpenAPI.NetdiskOpen.IsProxy = value; } }

        private OAuth Auth { get; }
        /// <summary>
        /// 
        /// </summary>
        internal BaiduNetdiskOpen(OAuth authToken)
        {
            Auth = authToken;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public Windows.Foundation.IAsyncOperation<UserInfo> GetUserInfoAsync()
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run(_ =>
               Task.Run(() =>
               {
                   var res = NetdiskOpenAPI.NetdiskBasic.UserInfo.GetUserInfo(Auth.AccessToken);
                   if (res != null && res.errno == 0)
                   {
                       return new UserInfo(Auth)
                       {
                           Name = res.netdisk_name ?? res.baidu_name,
                           Uk = res.uk,
                           VipType = res.vip_type,
                           AvatarUrl = res.avatar_url,
                       };
                   }
                   return null;
               }));
        }


       
    }
    /// <summary>
    /// 
    /// </summary>
    public class UserInfo
    {
        private OAuth Auth { get; }
        internal UserInfo(OAuth authToken)
        {
            Auth = authToken;
            Auth.vipType = VipType;
        }
        /// <summary>
        /// 
        /// </summary>
        public int VipType { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public long Uk { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public string AvatarUrl { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Windows.Foundation.IAsyncOperation<UserQuota> GetUserQuotaAsync()
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run(_ =>
               Task.Run<UserQuota>(() =>
               {
                   var res = NetdiskOpenAPI.NetdiskBasic.QuotaInfo.GetQuotaInfo(Auth.AccessToken);
                   if (res != null)
                   {
                       switch (res.errno)
                       {
                           case 0:
                               return new UserQuota() { total = res.total, used = res.used };
                           case -6:
                               throw new InvalidAuthException(-6, res.error_msg);
                           default:
                               throw new Exception("Fail");
                       }

                       
                   }
                   return null;
               }
               ));
        }
        /// <summary>
        /// Get root folder
        /// </summary>
        /// <returns></returns>
        public NetdiskFolder GetRootFolder()
        {
            return new NetdiskFolder(Auth) { Path = "/" };
        }
        /// <summary>
        /// 批量删除文件（夹）
        /// </summary>
        /// <param name="netdiskItems"></param>
        /// <returns></returns>
        public IAsyncAction DeleteItemsAsync(IList<INetdiskItem> netdiskItems)
        {
            return AsyncInfo.Run(_ =>
                       Task.Run(() =>
                       {
                           var list = new List<string>();
                           foreach (var it in netdiskItems)
                           {
                               list.Add(it.Path);
                           }
                           var res = NetdiskBasic.FileManager.DeleteFile(list, Auth.AccessToken);
                           if (res != null)
                           {
                               switch (res.errno)
                               {
                                   case 0:
                                       return;
                                   case -6:
                                       throw new InvalidAuthException(-6, res.error_msg);
                                   default:
                                       throw new Exception("Fail");
                               }
                           }
                           throw new Exception("Fail");

                       }));
        }
        /// <summary>
        /// 批量复制文件（夹）
        /// </summary>
        /// <param name="netdiskItems"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public IAsyncAction CopyItemsAsync(IList<INetdiskItem> netdiskItems, INetdiskFolder destinationFolder)
        {
            return AsyncInfo.Run(_ =>
                       Task.Run(() =>
                       {
                           var list = new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>();
                           foreach (var it in netdiskItems)
                           {
                               list.Add(new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem()
                               {
                                   dest = destinationFolder.Path,
                                   newname = it.Name,
                                   path = it.Path
                               });
                           }
                           var res = NetdiskBasic.FileManager.CopyFile(list, 0, Auth.AccessToken);
                           if (res != null)
                           {
                               switch (res.errno)
                               {
                                   case 0:
                                       return;
                                   case -6:
                                       throw new InvalidAuthException(-6, res.error_msg);
                                   default:
                                       throw new Exception("Fail");
                               }
                           }
                           throw new Exception("Net Error");

                       }));
        }
        /// <summary>
        /// 批量移动文件（夹）
        /// </summary>
        /// <param name="netdiskItems"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public IAsyncAction MoveItemsAsync(IList<INetdiskItem> netdiskItems, INetdiskFolder destinationFolder)
        {
            return AsyncInfo.Run(_ =>
                       Task.Run(() =>
                       {
                           var list = new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>();
                           foreach (var it in netdiskItems)
                           {
                               list.Add(new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem()
                               {
                                   dest = destinationFolder.Path,
                                   newname = it.Name,
                                   path = it.Path
                               });
                           }
                           var res = NetdiskBasic.FileManager.MoveFile(list, 0, Auth.AccessToken);
                           if (res != null)
                           {
                               switch (res.errno)
                               {
                                   case 0:
                                       return;
                                   case -6:
                                       throw new InvalidAuthException(-6, res.error_msg);
                                   default:
                                       throw new Exception("Fail");
                               }
                           }
                           throw new Exception("Fail");

                       }));
        }
        /// <summary>
        /// 批量分享
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<NetdiskItemShareInfo> ShareItemsAsync(IList<INetdiskItem> netdiskItems)
        {
            return AsyncInfo.Run(_ =>
                     Task.Run<NetdiskItemShareInfo>(() =>
                     {
                         var list = new List<ulong>();
                         foreach (var it in netdiskItems)
                             list.Add(it.Id);
                         var res = DupanShareOpenFileInfo.GetShareResultUrlRoot(list, Auth.AccessToken);
                         if (res != null)
                         {
                             switch (res.errno)
                             {
                                 case 0:
                                     return new NetdiskItemShareInfo() { Link = res.link, Password = res.password };
                                 case -6:
                                     throw new InvalidAuthException(-6, res.error_msg);
                                 default:
                                     throw new Exception("Fail");
                             }
                         }
                         else
                             throw new Exception("Net Error");
                     }
                     ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IAsyncOperation<IReadOnlyList<INetdiskItem>> SearchAsync(string key)
        {
            return AsyncInfo.Run(_ =>
                     Task.Run<IReadOnlyList<INetdiskItem>>(() =>
                     {
                         var res = NetdiskOpenAPI.NetdiskBasic.SearchData.GetFileList(new NetdiskBasic.SearchData.SearchRequest(key, Auth.AccessToken));
                         if (res != null)
                         {
                             switch (res.errno)
                             {
                                 case 0:
                                     {
                                         IReadOnlyList<INetdiskItem> resLists = null;
                                         var list = new INetdiskItem[res.list.Count];

                                         for (int i = 0; i < res.list.Count; i++)
                                         {
                                             var it = res.list[i];
                                             if (it.isdir > 0)
                                             {
                                                 list[i] = new NetdiskFolder(this.Auth)
                                                 {
                                                     Name = it.server_filename,
                                                     Path = it.path,
                                                     Id = it.fs_id,
                                                     DateModified = new DateTimeOffset(EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(it.server_mtime)),
                                                 };
                                             }
                                             else
                                             {
                                                 list[i] = new NetdiskFile(this.Auth)
                                                 {
                                                     Name = it.server_filename,
                                                     Path = it.path,
                                                     Md5 = it.md5,
                                                     Id = it.fs_id,
                                                     Size = it.size,
                                                     thumbs = it.thumbs,
                                                     Category = NetdiskFolder.GetCategory(it.category, it.server_filename),
                                                     DateModified = new DateTimeOffset(EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(it.server_mtime)),
                                                 };
                                             }
                                         }
                                         resLists = list;
                                         return resLists;
                                     }
                                 case -6:
                                     throw new InvalidAuthException(-6, res.error_msg);
                                       
                                 default:
                                     throw new Exception("Fail");
                             }
                         }
                         else
                             throw new Exception("Net Error");
                     }
                     ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="fs_id"></param>
        /// <param name="modifiedDate"></param>
        /// <returns></returns>
        public NetdiskFolder CreateNetdiskFolder(string name, string path, ulong fs_id, long modifiedDate)
        {
            return new NetdiskFolder(Auth)
            {
                Name = name,
                DateModified = EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(modifiedDate),
                Path = path,
                Id = fs_id,
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="fs_id"></param>
        /// <param name="modifiedDate"></param>
        /// <param name="size"></param>
        /// <param name="category"></param>
        /// <param name="icon"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        public NetdiskFile CreateNetdiskFile(string name, string path, ulong fs_id, long modifiedDate, ulong size, Category category, string icon, string md5)
        {
            return new NetdiskFile(Auth)
            {
                Name = name,
                DateModified = EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(modifiedDate),
                Path = path,
                Id = fs_id,
                Size = size,
                Category = category,
                thumbs = new NetdiskBasic.Thumbs() { url4 = icon},
                Md5 = md5
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Task<WriteableBitmap> GetWriteableBitmapAsync(string url)
        {
            return NetdiskOpen.GetWriteableBitmapAsync(url);
        }

    }
    /// <summary>
    /// 
    /// </summary>
    public class UserQuota
    {
        internal UserQuota() { }
        /// <summary>
        /// 总空间大小，单位B
        /// </summary>
        public long total { get; internal set; }
        /// <summary>
        /// 已使用大小，单位B
        /// </summary>
        public long used { get; internal set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class NetdiskFile : INetdiskFile, INetdiskItem, INetdiskItem2
    {
        private OAuth Auth { get; }
        internal NetdiskBasic.Thumbs thumbs { get; set; }

        internal NetdiskFile(OAuth Auth)
        {
            this.Auth = Auth;
        }

        /// <summary>
        /// get the name
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// get the path
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        /// get the fs_id
        /// </summary>
        public ulong Id { get; internal set; }
        /// <summary>
        /// Get size of file.
        /// </summary>
        public ulong Size { get; internal set; }
        /// <summary>
        /// get the file Type
        /// </summary>
        public string FileType { get { if (Name != null && Name.Contains('.') && !Name.EndsWith('.')) return Name.Substring(Name.LastIndexOf('.') + 1); return null; } }
        /// <summary>
        /// get the modified date
        /// </summary>
        public DateTimeOffset DateModified { get; internal set; }
        /// <summary>
        /// get the md5 of file.
        /// </summary>
        public string Md5 { get; internal set; }

        /// <summary>
        /// Get the category of file.
        /// </summary>
        public Category Category { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<NetdiskFolder> GetParentAsync()
        {
            return AsyncInfo.Run(_ =>
               Task.Run<NetdiskFolder>(() =>
               {
                   string folderPath = "/", folderName = "";
                   var index = Path.LastIndexOf('/');
                   if (index == 0)
                       return null;
                   else
                   {
                       folderPath = Path.Substring(0, index);
                       folderName = folderPath.Substring(folderPath.LastIndexOf('/'));
                       return new NetdiskFolder(Auth) { Name = folderName, Path = folderPath };
                   }
               }
            ));
        }
        /// <summary>
        /// Rename
        /// </summary>
        /// <param name="desiredName"></param>
        /// <returns></returns>
        public IAsyncAction RenameAsync(string desiredName)
        {
            return AsyncInfo.Run(_ =>
                     Task.Run(() =>
                     {
                         if (NetdiskFolder.CheckNameIsOk(desiredName) == false)
                             throw new ArgumentException("desiredName Contain illegal character");
                         var res = NetdiskBasic.FileManager.RenameFile(new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>()
                         { new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem()
                         { newname = desiredName,
                              path = this.Path
                         } }, 0, Auth.AccessToken);
                         if (res != null)
                         {
                             switch (res.errno)
                             {
                                 case 0:
                                     {
                                         var d = this.FileType;
                                         this.Name = desiredName;
                                         this.Path = Path.Substring(0, Path.LastIndexOf('/') + 1) + desiredName;
                                         //this.DateModified = new DateTimeOffset(DateTime.Now);
                                         if (d != FileType)
                                             this.Category = NetdiskFolder.GetCategory(0, Name);
                                         return;
                                     }
                                 case -6:
                                     throw new InvalidAuthException(-6, res.error_msg);
                                 default:
                                     throw new Exception("Fail");
                             }
                         }
                         else
                             throw new Exception("Net Error");
                         throw new Exception("Fail");
                     }
                     ));
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <returns></returns>
        public IAsyncAction DeleteAsync()
        {
            return AsyncInfo.Run(_ =>
                       Task.Run(() =>
                       {
                           var res = NetdiskBasic.FileManager.DeleteFile(new List<string>() { this.Path }, Auth.AccessToken);
                           if (res != null)
                           {
                               switch (res.errno)
                               {
                                   case 0:
                                       {
                                           
                                           return;
                                       }
                                   case -6:
                                       throw new InvalidAuthException(-6, res.error_msg);
                                   default:
                                       throw new Exception("Fail");
                               }
                           }
                           else
                               throw new Exception("Net Error");

                       }));
        }
        /// <summary>
        /// Copy
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public IAsyncAction CopyAsync(INetdiskFolder destinationFolder)
        {
            return AsyncInfo.Run(_ =>
                Task.Run(() =>
                {
                    var lists = new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>()
                    { new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem(){ dest=destinationFolder.Path,newname=Name,path=Path } };

                    var res = NetdiskBasic.FileManager.CopyFile(lists, 0, Auth.AccessToken);
                    if (res != null)
                    {
                        switch (res.errno)
                        {
                            case 0:
                                {

                                    return;
                                }
                            case -6:
                                throw new InvalidAuthException(-6, res.error_msg);
                            default:
                                throw new Exception("Fail");
                        }
                    }
                    else
                        throw new Exception("Net Error");
                }
                ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<NetdiskItemShareInfo> ShareAsync()
        {
            return AsyncInfo.Run(_ =>
                     Task.Run<NetdiskItemShareInfo>(() =>
                     {
                         var res = DupanShareOpenFileInfo.GetShareResultUrlRoot(new List<ulong>() { Id }, Auth.AccessToken);
                         if (res != null)
                         {
                             switch (res.errno)
                             {
                                 case 0:
                                     return new NetdiskItemShareInfo() { Link = res.link, Password = res.password };
                                 case -6:
                                     throw new InvalidAuthException(-6, res.error_msg);
                                 default:
                                     throw new Exception("Fail");
                             }
                         }
                         else
                             throw new Exception("Net Error");
                     }
                     ));
        }
        /// <summary>
        /// Move
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public IAsyncAction MoveAsync(INetdiskFolder destinationFolder)
        {
            return AsyncInfo.Run(_ =>
                Task.Run(() =>
                {
                    var lists = new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>()
                    { new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem(){ dest=destinationFolder.Path,newname=Name,path=Path } };

                    var res = NetdiskBasic.FileManager.MoveFile(lists, 0, Auth.AccessToken);
                    if (res != null)
                    {
                        switch (res.errno)
                        {
                            case 0:
                                return;
                            case -6:
                                throw new InvalidAuthException(-6, res.error_msg);
                            default:
                                throw new Exception("Fail");
                        }
                    }
                    else
                        throw new Exception("Net Error");
                }
                ));
        }

        /// <summary>
        /// is type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsOfType(NetdiskItemTypes type) => type == NetdiskItemTypes.File;
        private Uri downloadUri;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<Uri> GetDownloadUriAsync()
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<Uri>(() =>
                  {
                      if (downloadUri != null)
                          return downloadUri;
                      var res = NetdiskOpenAPI.NetdiskBasic.Filemetas.GetFilemetas(new NetdiskBasic.Filemetas.FilemetasRequest(new ulong[1] { this.Id }, Auth.AccessToken));
                      
                      if (res != null)
                      {
                          if (res.list?.Count > 0)
                          {
                              return downloadUri = new Uri(res.list.First().dlink + "&access_token=" + Auth.AccessToken);
                          }
                          else
                              throw new Exception("Fail");
                      }
                      else
                          throw new Exception("Net Error");
                  }
                  ));
        }
        /// <summary>
        /// Size can't more than 5MB
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<byte[]>GetFilebytesAsync()
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<byte[]>(async() =>
                  {
                      if (Size >= 5 * 1024 * 1024)
                          throw new Exception("Size can't more than 5MB");
                      var uri = await GetDownloadUriAsync();
                      
                      return NetdiskOpen.Request<byte[]>(uri.AbsoluteUri);
                  }
                  ));
        }
        /// <summary>
        /// Get Video
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<VideoStreamInfo> GetVideoStreamInfoAsync()
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<VideoStreamInfo>(() =>
                  {
                      if (Category != Category.Movie)
                          throw new Exception("This file is not movie file!");
                      if (Auth.vipType >= 2)
                      {
                          return new VideoStreamInfo(Auth, this) { LTime = 0 };
                      }
                      var it = NetdiskVideo.VideoStream.GetAdToken(Path, NetdiskVideo.VideoStream.M3U8Type.M3U8_AUTO_480, Auth.AccessToken);
                      if (it != null)
                      {
                          return new VideoStreamInfo(Auth, this) { AdToken = it.adToken, LTime = it.ltime };
                      }
                      else
                          throw new Exception("Net Error");
                  }
                  ));
        }
        /// <summary>
        /// Get Video
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<VideoStreamInfo> GetVideoStreamInfoAsync(string adToken)
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<VideoStreamInfo>(() =>
                  {
                      if (Category != Category.Movie)
                          throw new Exception("This file is not movie file!");
                      var it = NetdiskVideo.VideoStream.GetAdToken(Path, NetdiskVideo.VideoStream.M3U8Type.M3U8_AUTO_480, adToken);
                      if (it != null)
                      {
                          return new VideoStreamInfo(Auth, this) { AdToken = it.adToken, LTime = it.ltime };
                      }
                      else
                          throw new Exception("Net Error");
                  }
                  ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Uri GetMusicStreamUri()
        {
            if (Category != Category.Music)
                throw new Exception("This file is not music file!");
            var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=streaming&path={UrlEncodeToUpper.UrlEncode(Path)}&type=M3U8_HLS_MP3_128";
            url += "&access_token=" + Auth.AccessToken;
            return new Uri(url);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetThumbUrl()
        {
            if (thumbs != null)
            {
                if (thumbs.url4 != null)
                    return thumbs.url4;
                if (thumbs.url3 != null)
                    return thumbs.url3;
                if (thumbs.url2 != null)
                    return thumbs.url2;
                if (thumbs.url1 != null)
                    return thumbs.url1;
                if (thumbs.icon != null)
                    return thumbs.icon;
            }
            return null;
        }

    }
    /// <summary>
    /// 
    /// </summary>
    public class NetdiskFolder : INetdiskFolder, INetdiskItem, INetdiskItem2
    {
        internal NetdiskFolder(OAuth Auth)
        {
            this.Auth = Auth;
        }
        private OAuth Auth { get; }
        /// <summary>
        /// Get name
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// get path
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        /// fs_id
        /// </summary>
        public ulong Id { get; internal set; }
        /// <summary>
        /// get modified Date
        /// </summary>
        public DateTimeOffset DateModified { get; internal set; }

        private List<NetdiskFile> files;
        private List<NetdiskFolder> folders;
        /// <summary>
        /// Get Parent
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<NetdiskFolder> GetParentAsync()
        {
            return AsyncInfo.Run(_ =>
               Task.Run<NetdiskFolder>(() =>
            {
                string folderPath = "/", folderName = "";
                var index = Path.LastIndexOf('/');
                if (index == 0)
                    return null;
                else
                {
                    folderPath = Path.Substring(0, index);
                    folderName = folderPath.Substring(folderPath.LastIndexOf('/'));
                    return new NetdiskFolder(Auth) { Name = folderName, Path = folderPath };
                }
            }
            ));
        }
        /// <summary>
        /// Create Folder
        /// </summary>
        /// <param name="desiredName"></param>
        /// <returns></returns>
        public IAsyncOperation<NetdiskFolder> CreateFolderAsync(string desiredName)
        {
            return AsyncInfo.Run(_ =>
                Task.Run(() =>
                {
                    var newPath = this.Path + "/" + desiredName;
                    var res = UploadFile.FileCreate.CreatNewItem(new UploadFile.FileCreate.FileCreateRequest(newPath, 1, Auth.AccessToken));
                    if (res != null)
                    {
                        switch (res.errno)
                        {
                            case 0:
                                {
                                    return new NetdiskFolder(Auth) { Name = desiredName, Path = newPath, };
                                }
                            case -6:
                                throw new InvalidAuthException(-6, res.error_msg);
                            default:
                                throw new Exception("Fail");
                        }
                    }
                    throw new Exception("Net Error");
                }));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IAsyncOperation<NetdiskFile> GetFileAsync(string name)
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<NetdiskFile>(async () =>
                  {
                      var res = await GetFilesAsync();
                      if (res != null)
                      {
                          foreach (var it in res)
                          {
                              if (it.Name == name)
                                  return it;
                          }
                      }
                      return null;
                  }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IAsyncOperation<NetdiskFolder> GetFolderAsync(string name)
        {
            return AsyncInfo.Run(_ =>
                 Task.Run<NetdiskFolder>(async () =>
                 {
                     var res = await GetFoldersAsync();
                     if (res != null)
                     {
                         foreach (var it in res)
                         {
                             if (it.Name == name)
                                 return it;
                         }
                     }
                     return null;
                 }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IAsyncOperation<INetdiskItem> GetItemAsync(string name)
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<INetdiskItem>(async () =>
                  {
                      var res = await GetItemsAsync();
                      if (res != null)
                      {
                          foreach (var it in res)
                          {
                              if (it.Name == name)
                                  return it;
                          }
                      }
                      return null;
                  }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<IReadOnlyList<NetdiskFile>> GetFilesAsync()
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<IReadOnlyList<NetdiskFile>>(async () =>
                  {
                      var res = await GetItemsAsync();
                      if (res != null)
                      {
                          var resList = new NetdiskFile[files.Count];

                          for (var i = 0; i < files.Count; i++)
                          {
                              resList[i] = files[i];
                          }
                          return Array.AsReadOnly<NetdiskFile>(resList);
                      }
                      return null;
                  }
                  ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<IReadOnlyList<NetdiskFolder>> GetFoldersAsync()
        {
            return AsyncInfo.Run(_ =>
                 Task.Run<IReadOnlyList<NetdiskFolder>>(async () =>
                 {
                     var res = await GetItemsAsync();
                     if (res != null)
                     {
                         var resList = new NetdiskFolder[folders.Count];

                         for (var i = 0; i < files.Count; i++)
                         {
                             resList[i] = folders[i];
                         }
                         return Array.AsReadOnly<NetdiskFolder>(resList);
                     }
                     return null;
                 }
                 ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<IReadOnlyList<INetdiskItem>> GetItemsAsync()
        {
            return AsyncInfo.Run(_ =>
                Task.Run<IReadOnlyList<INetdiskItem>>(async () =>
                {
                    if (files == null || folders == null)
                    {
                        await this.RefreshAsync();
                    }

                    var list = new INetdiskItem[files.Count + folders.Count];
                    for (int i = 0; i < folders.Count; i++)
                    {
                        list[i] = folders[i];
                    }

                    for (int i = 0; i < files.Count; i++)
                    {
                        list[i + folders.Count] = files[i];
                    }
                    return Array.AsReadOnly<INetdiskItem>(list);// list.AsReadOnly();

                }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncAction RefreshAsync()
        {
            return AsyncInfo.Run(_ =>
                Task.Run(() =>
                {
                    var res = NetdiskOpenAPI.NetdiskBasic.FileList.GetFileList(new NetdiskBasic.FileList.FileListRequest(Auth.AccessToken) { dir = this.Path });

                    if (res != null)
                    {
                        switch (res.errno)
                        {
                            case 0:
                                {
                                    var _files = files;
                                    var _folders = folders;
                                    files = new List<NetdiskFile>();
                                    folders = new List<NetdiskFolder>();
                                    foreach (var it in res.list)
                                    {
                                        if (it.isdir == 0)
                                        {
                                            var _it = HaveNetdiskFile(_files, it.fs_id);
                                            _it.Name = it.server_filename;
                                            _it.Path = it.path;
                                            _it.Id = it.fs_id;
                                            _it.DateModified = new DateTimeOffset(EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(it.server_mtime));
                                            _it.Md5 = it.md5;
                                            _it.Size = it.size;
                                            _it.thumbs = it.thumbs;
                                            _it.Category = GetCategory(it.category, it.server_filename);
            
                                            files.Add(_it);
                                        }
                                        else
                                        {
                                            var _it = HaveNetdiskFolder(_folders, it.fs_id);
                                            _it.Name = it.server_filename;
                                            _it.Path = it.path;
                                            _it.Id = it.fs_id;
                                            _it.DateModified = new DateTimeOffset(EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(it.server_mtime));

                                            folders.Add(_it);
                                        }
                                        
                                    }
                                }
                                break;
                            case -6:
                                throw new InvalidAuthException(-6, res.error_msg);
                            case 2131:
                            case -9:
                                throw new ItemNotExistException(res.errno, res.error_msg);
                            default:
                                throw new Exception("Fail");
                        }
                    }
                    else
                        throw new Exception("Net Error");
                }));
        }

        private NetdiskFile HaveNetdiskFile(List<NetdiskFile> list, ulong fs_id)
        {
            if (list != null)
            foreach (var it in list)
                if (it.Id == fs_id)
                    return it;
            return new NetdiskFile(Auth);
        }
        private NetdiskFolder HaveNetdiskFolder(List<NetdiskFolder> list, ulong fs_id)
        {
            if (list != null)
                foreach (var it in list)
                    if (it.Id == fs_id)
                        return it;
            return new NetdiskFolder(Auth);
        }

        /// <summary>
        /// Rename
        /// </summary>
        /// <param name="desiredName"></param>
        /// <returns></returns>
        public IAsyncAction RenameAsync(string desiredName)
        {
            return AsyncInfo.Run(_ =>
                     Task.Run(() =>
                     {
                         if (CheckNameIsOk(desiredName) == false)
                             throw new ArgumentException("desiredName Contain illegal character");
                         var res = NetdiskBasic.FileManager.RenameFile(new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>()
                         { new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem()
                         { newname = desiredName,
                              path = this.Path
                         } }, 0, Auth.AccessToken);
                         if (res != null)
                         {
                             switch (res.errno)
                             {
                                 case 0:
                                     {
                                         this.Name = desiredName;
                                         this.Path = Path.Substring(0, Path.LastIndexOf('/') + 1) + desiredName;
                                         return;
                                     }
                                 case -6:
                                     throw new InvalidAuthException(-6, res.error_msg);
                                 default:
                                     throw new Exception("Fail");
                             }
                          
                         }
                         else
                             throw new Exception("Net Error");
                         throw new Exception("Fail");
                     }
                     ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<NetdiskItemShareInfo> ShareAsync()
        {
            return AsyncInfo.Run(_ =>
                     Task.Run<NetdiskItemShareInfo>(() =>
                     {
                         var res = DupanShareOpenFileInfo.GetShareResultUrlRoot(new List<ulong>() { Id }, Auth.AccessToken);
                         if (res != null)
                         {
                             switch (res.errno)
                             {
                                 case 0:
                                     return new NetdiskItemShareInfo() { Link = res.link, Password = res.password };
                                 case -6:
                                     throw new InvalidAuthException(-6, res.error_msg);
                                 default:
                                     throw new Exception("Fail");
                             }
                             
                                 
                         }
                         else
                             throw new Exception("Net Error");
                     }
                     ));
        }
        /// <summary>
        /// Delete
        /// </summary>
        /// <returns></returns>
        public IAsyncAction DeleteAsync()
        {
            return AsyncInfo.Run(_ =>
                       Task.Run(() =>
                       {
                           var res = NetdiskBasic.FileManager.DeleteFile(new List<string>() { this.Path }, Auth.AccessToken);
                           if (res != null)
                           {

                               switch (res.errno)
                               {
                                   case 0:
                                       return;
                                   case -6:
                                       throw new InvalidAuthException(-6, res.error_msg);
                                   default:
                                       throw new Exception("Fail");
                               }
                           }
                           else
                               throw new Exception("Net Error");

                       }));
        }
        /// <summary>
        /// Copy
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public IAsyncAction CopyAsync(INetdiskFolder destinationFolder)
        {
            return AsyncInfo.Run(_ =>
                Task.Run(() =>
                {
                    var lists = new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>()
                    { new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem(){ dest=destinationFolder.Path,newname=Name,path=Path } };

                    var res = NetdiskBasic.FileManager.CopyFile(lists, 0, Auth.AccessToken);
                    if (res != null)
                    {
                        switch (res.errno)
                        {
                            case 0:
                                return;
                            case -6:
                                throw new InvalidAuthException(-6, res.error_msg);
                            default:
                                throw new Exception("Fail");
                        }
                    }
                    else
                        throw new Exception("Net Error");
                }
                ));
        }
        /// <summary>
        /// Move
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public IAsyncAction MoveAsync(INetdiskFolder destinationFolder)
        {
            return AsyncInfo.Run(_ =>
                Task.Run(() =>
                {
                    var lists = new List<NetdiskBasic.FileManager.FileManagerRequest.FilelistItem>()
                    { new NetdiskBasic.FileManager.FileManagerRequest.FilelistItem(){ dest=destinationFolder.Path,newname=Name,path=Path } };

                    var res = NetdiskBasic.FileManager.MoveFile(lists, 0, Auth.AccessToken);
                    if (res != null)
                    {
                        switch (res.errno)
                        {
                            case 0:
                                return;
                            case -6:
                                throw new InvalidAuthException(-6, res.error_msg);
                            default:
                                throw new Exception("Fail");
                        }
                    }
                    else
                        throw new Exception("Net Error");
                }
                ));
        }
        /// <summary>
        /// is type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsOfType(NetdiskItemTypes type) => type == NetdiskItemTypes.Folder;


        internal static bool CheckNameIsOk(string name)
        {
            var bol = string.IsNullOrEmpty(name) || name.Contains('/')
                || name.Contains('\\') || name.Contains('|') || name.Contains(':')
                || name.Contains('*') || name.Contains('?') || name.Contains('"') || name.Contains('<')
                || name.Contains('>');
            return !bol;
        }
        internal static Category GetCategory(int category, string name)
        {
            if (category == 1)
            {
                return Category.Movie;

            }
            else if (IsPicFile(name) || category == 3)
            {
                return Category.Music;
            }
            else if (IsTxtFile(name))
            {
                return Category.Txt;
            }
            else if (IsPdfFile(name))
            {
                return Category.Pdf;
            }
            else if (IsZipFile(name))
            {
                return Category.Zip;
            }
            else if (IsXlsFile(name))
            {
                return Category.Xls;
            }
            else if (IsPptFile(name))
            {
                return Category.Ppt;
            }
            else if (IsMusicFile(name) || category == 2)
            {
                return Category.Music;
            }
            else if (IsDocFile(name))
            {
                return Category.Doc;
            }
            else
            {
                return Category.Other;
            }
        }
        private static bool IsPicFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".png") || name.EndsWith(".jpg"))
                return true;
            return false;
        }
        private static bool IsPdfFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".pdf"))
                return true;
            return false;
        }
        private static bool IsZipFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".zip") || name.EndsWith(".rar") || name.EndsWith(".7z"))
                return true;
            return false;
        }
        private static bool IsMusicFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".mp3") || name.EndsWith(".wma") || name.EndsWith(".wav") || name.EndsWith(".aac") || name.EndsWith(".wmv") || name.EndsWith(".flac"))
                return true;
            return false;
        }
        private static bool IsDocFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".doc") || name.EndsWith(".docx"))
                return true;
            return false;
        }
        private static bool IsTxtFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".txt") || name.EndsWith(".text") || name.EndsWith(".rtf"))
                return true;
            return false;
        }
        private static bool IsPptFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".ppt") || name.EndsWith(".pptx"))
                return true;
            return false;
        }
        private static bool IsXlsFile(string name)
        {
            if (name == null)
                return false;
            name = name.ToLower();
            if (name.EndsWith(".xls") || name.EndsWith(".xlsx"))
                return true;
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NetdiskItemShareInfo
    {
        internal NetdiskItemShareInfo()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public string Link { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public string Password { get; internal set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class VideoStreamInfo
    {
        private INetdiskFile file { get; }
        private OAuth auth { get; }

        internal VideoStreamInfo(OAuth auth, INetdiskFile file) { this.file = file; this.auth = auth; }
        /// <summary>
        /// 
        /// </summary>
        public string AdToken { get; internal set; }
        /// <summary>
        /// Least time
        /// </summary>
        public int LTime { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="videoType"></param>
        /// <returns></returns>
        public Uri GetStreamUri(VideoType videoType)
        {
            var url = $"https://pan.baidu.com/rest/2.0/xpan/file?method=streaming&path={UrlEncodeToUpper.UrlEncode(file.Path)}&adToken={UrlEncodeToUpper.UrlEncode(AdToken)}&type={videoType.ToString()}&access_token={auth.AccessToken}";
            return new Uri(url);
        }


        /// <summary>
        /// 
        /// </summary>
        public enum VideoType
        {
            /// <summary>
            /// 标清
            /// </summary>
            M3U8_AUTO_480,
            /// <summary>
            /// 超清
            /// </summary>
            M3U8_AUTO_720,
            /// <summary>
            /// 高清
            /// </summary>
            M3U8_AUTO_1080,
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class InvalidAuthException: Exception
    {
        internal InvalidAuthException(int code, string msg):base(msg)
        {
            Code = code;
            ErrorMessage = msg;
        }
        /// <summary>
        /// 
        /// </summary>
        public int Code { get;  }
        /// <summary>
        /// 
        /// </summary>
        public string ErrorMessage { get;  }
        
    }
    /// <summary>
    /// 
    /// </summary>
    public class ItemNotExistException: Exception
    {
        internal ItemNotExistException(int code, string msg) : base(msg)
        {
            Code = code;
            ErrorMessage = msg;
        }
        /// <summary>
        /// 
        /// </summary>
        public int Code { get; }
        /// <summary>
        /// 
        /// </summary>
        public string ErrorMessage { get; }
    }

}
