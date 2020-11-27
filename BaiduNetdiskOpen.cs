using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;
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
        /// <param name="redirect_uri"></param>
        /// <returns></returns>
        public Windows.Foundation.IAsyncOperation<AuthToken> GetAuthTokenAsync(string code, string redirect_uri)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run(_ =>
                   Task.Run<AuthToken>(async () =>
                   {
                       var res = await NetdiskOpenAPI.OAuthToken.GetOAuthTokenFromCodeAsync(code, oAuth.apiKey, oAuth.secretKey, redirect_uri);
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
                           Name = string.IsNullOrEmpty(res.netdisk_name) ? res.baidu_name: res.netdisk_name,
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
    public sealed class UserInfo
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
            if (rootFolder == null)
                rootFolder = new NetdiskFolder(Auth, null) { path = "/" ,Name = "All"}; ;
            return rootFolder;
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
                                       foreach (var it in netdiskItems)
                                       {
                                           if (it is NetdiskFile file)
                                               file.parent?.DeleteItem(it);
                                           else if (it is NetdiskFolder folder)
                                               folder.parent?.DeleteItem(it);
                                       }
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
                                       var q = destinationFolder.RefreshAsync();
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
                                       foreach (var it in netdiskItems)
                                       {
                                           if (it is NetdiskFile file)
                                               file.parent?.DeleteItem(it);
                                           else if (it is NetdiskFolder folder)
                                               folder.parent?.DeleteItem(it);
                                                
                                           (destinationFolder as NetdiskFolder)?.AddItem(it);
                                       }
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
                                         var array = new INetdiskItem[res.list.Count];
                                         int i = 0;
                                         foreach(var it in res.list)
                                         {
                                             if (it.isdir > 0)
                                             {
                                                 array[i] = new NetdiskFolder(this.Auth, null)
                                                 {
                                                     Name = it.server_filename,
                                                     path = it.path,
                                                     Id = it.fs_id,
                                                     DateModified = new DateTimeOffset(EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(it.server_mtime)),
                                                 };
                                             }
                                             else
                                             {
                                                 array[i] = new NetdiskFile(this.Auth, null)
                                                 {
                                                     Name = it.server_filename,
                                                     path = it.path,
                                                     Md5 = it.md5,
                                                     Id = it.fs_id,
                                                     Size = it.size,
                                                     thumbs = it.thumbs,
                                                     Category = NetdiskFolder.GetCategory(it.category, it.server_filename),
                                                     DateModified = new DateTimeOffset(EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(it.server_mtime)),
                                                 };
                                             }
                                             i++;
                                         }
                                         
                                         return array;
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
        /// <param name="shareLink"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public XpanShareTool GetXpanShareTool(string shareLink, string password)
        {
            return new XpanShareTool(Auth, shareLink, password);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="shareUk"></param>
        /// <param name="shareId"></param>
        /// <returns></returns>
        public XpanShareTool GetXpanShareTool(string privateKey, string shareUk, string shareId)
        {
            return new XpanShareTool(Auth,shareUk, shareId);
        }
        private NetdiskFolder rootFolder;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="fs_id"></param>
        /// <param name="modifiedDate"></param>
        /// <returns></returns>
        public NetdiskFolder GetNetdiskFolder(string name, string path, ulong fs_id, long modifiedDate)
        {
            return new NetdiskFolder(Auth, null)
                {
                    Name = name,
                    DateModified = EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(modifiedDate),
                    path = path,
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
        public NetdiskFile GetNetdiskFile(string name, string path, ulong fs_id, long modifiedDate, ulong size, Category category, string icon, string md5)
        {
            return new NetdiskFile(Auth, null)
            {
                Name = name,
                DateModified = EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(modifiedDate),
                path = path,
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
    public abstract class NetdiskItem: INetdiskItem
    {
        internal OAuth Auth { get; }
        internal NetdiskFolder parent;
        internal NetdiskItem(OAuth Auth, NetdiskFolder parent)
        {
            this.Auth = Auth;
            this.parent = parent;
        }

        /// <summary>
        /// Gets the date and time when the current item was created.
        /// </summary>
        public DateTimeOffset DateModified { get; internal set; }
        /// <summary>
        /// Gets the name of the item including the file name extension if there is one.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the fs_id of the file.
        /// </summary>
        public ulong Id { get; internal set; }
        internal string path;
        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        public string Path
        {
            get
            {
                if (parent != null)
                    return parent.Path + (parent.Path.EndsWith('/') ? null : "/") + Name;
                else
                    return path;
            }
        }
        /// <summary>
        /// Creates a copy of the file in the specified folder.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the copy of the file is created.</param>
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
                                    (destinationFolder as NetdiskFolder).RefreshAsync().AsTask();
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
        /// Moves the current file to the specified folder。
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the file is moved.</param>
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
                                this.parent?.DeleteItem(this);
                                this.path = destinationFolder.Path + "/" + Name;
                                (destinationFolder as NetdiskFolder).AddItem(this);
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
        private static bool CheckNameIsOk(string name)
        {
            var bol = string.IsNullOrEmpty(name) || name.Contains('/')
                || name.Contains('\\') || name.Contains('|') || name.Contains(':')
                || name.Contains('*') || name.Contains('?') || name.Contains('"') || name.Contains('<')
                || name.Contains('>');
            return !bol;
        }
        /// <summary>
        /// Renames the file according to the desired name.
        /// </summary>
        /// <param name="desiredName">The desired name of the file after it is renamed.</param>
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
                                         this.path = Path.Substring(0, Path.LastIndexOf('/') + 1) + desiredName;
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
        /// Deletes the current file.
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
                                           this.parent?.DeleteItem(this);
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
        /// Determines whether the current INetdiskItem matches the specified NetdiskItemTypes
        /// </summary>
        /// <param name="type">The value to match against.</param>
        /// <returns></returns>
        public abstract bool IsOfType(NetdiskItemTypes type);
        /// <summary>
        /// Share the item.
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
        
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class NetdiskFile : NetdiskItem, INetdiskFile, INetdiskItem, INetdiskItem2
    {
        internal NetdiskBasic.Thumbs thumbs { get; set; }

        internal NetdiskFile(OAuth Auth, NetdiskFolder parent):base(Auth, parent)
        {
        }
        /// <summary>
        /// Get size of file.
        /// </summary>
        public ulong Size { get; internal set; }
        /// <summary>
        /// get the file Type
        /// </summary>
        public string FileType { get { if (Name != null && Name.Contains('.') && !Name.EndsWith('.')) return Name.Substring(Name.LastIndexOf('.')); return null; } }
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
        public IAsyncOperation<INetdiskFolder> GetParentAsync()
        {
            return AsyncInfo.Run(_ =>
               Task.Run<INetdiskFolder>(() =>
               {
                   if (parent == null)
                   {
                       string folderPath = "/", folderName = "All";
                       var index = Path.LastIndexOf('/');
                       if (index > 0)
                       {
                           folderPath = Path.Substring(0, index);
                           folderName = folderPath.Substring(folderPath.LastIndexOf('/') + 1);
                       }
                       return new NetdiskFolder(Auth, null) { Name = folderName, path = folderPath };
                   }
                   else
                       return parent;
               }
            ));
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool IsOfType(NetdiskItemTypes type) => type == NetdiskItemTypes.File;
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
                      
                      return NetdiskOpen.Request<byte[]>(await NetdiskOpen.GetLocationUriAsync(uri.AbsoluteUri));
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
                      var it = NetdiskVideo.VideoStream.GetAdToken(Path, NetdiskVideo.VideoStream.M3U8Type.M3U8_AUTO_480, Auth.AccessToken, adToken);
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode() & Name.GetHashCode() + Path.GetHashCode() | this.DateModified.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is INetdiskItem it)
                return this.Id == it.Id;
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class NetdiskFolder : NetdiskItem,INetdiskFolder, INetdiskItem, INetdiskItem2
    {
        
        internal NetdiskFolder(OAuth Auth, NetdiskFolder parent):base(Auth, parent)
        {
        }
        private List<NetdiskFile> files;
        private List<NetdiskFolder> folders;
        /// <summary>
        /// Get Parent
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<INetdiskFolder> GetParentAsync()
        {
            return AsyncInfo.Run(_ =>
               Task.Run<INetdiskFolder>(() =>
               {
                   if (parent == null)
                   {
                       string folderPath = "/", folderName = "All";
                       var index = Path.LastIndexOf('/');
                       if (index > 0)
                       {
                           folderPath = Path.Substring(0, index);
                           folderName = folderPath.Substring(folderPath.LastIndexOf('/') + 1);

                       }
                       return new NetdiskFolder(Auth, null) { Name = folderName, path = folderPath };
                   }
                   return parent;
               }
            ));
        }
        /// <summary>
        /// Create Folder
        /// </summary>
        /// <param name="desiredName"></param>
        /// <returns></returns>
        public IAsyncOperation<INetdiskFolder> CreateFolderAsync(string desiredName)
        {
            return AsyncInfo.Run(_ =>
                Task.Run<INetdiskFolder>(() =>
                {
                    var newPath = this.Path + "/" + desiredName;
                    var res = UploadFile.FileCreate.CreatNewItem(new UploadFile.FileCreate.FileCreateRequest(newPath, 1, Auth.AccessToken));
                    if (res != null)
                    {
                        switch (res.errno)
                        {
                            case 0:
                                {
                                    var name = res.path.Split('/').Last();
                                    var it = new NetdiskFolder(Auth, this)
                                    {
                                        Name = name,
                                        path = res.path,
                                        DateModified = EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(res.mtime),
                                        Id = res.fs_id
                                    };
                                    AddItem(it);
                                    return it;
                                }
                            case -6:
                                throw new InvalidAuthException(-6, res.error_msg??"Invalid Auth");
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
        public IAsyncOperation<INetdiskFile> GetFileAsync(string name)
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<INetdiskFile>(async () =>
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
        public IAsyncOperation<INetdiskFolder> GetFolderAsync(string name)
        {
            return AsyncInfo.Run(_ =>
                 Task.Run<INetdiskFolder>(async () =>
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
        public IAsyncOperation<IReadOnlyList<INetdiskFile>> GetFilesAsync()
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<IReadOnlyList<INetdiskFile>>(async () =>
                  {
                      var res = await GetItemsAsync();
                      if (res != null)
                      {
                          return files.AsReadOnly();
                      }
                      return null;
                  }
                  ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<IReadOnlyList<INetdiskFolder>> GetFoldersAsync()
        {
            return AsyncInfo.Run(_ =>
                 Task.Run<IReadOnlyList<INetdiskFolder>>(async() =>
                 {
                     var res = await GetItemsAsync();
                     if (res != null)
                     {
                         return folders.AsReadOnly();
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
                    var res = new List<INetdiskItem>();
                    res.AddRange(folders);
                    res.AddRange(files);
                    return res.AsReadOnly();

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
                                            _it.path = it.path;
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
                                            _it.path = it.path;
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
            return new NetdiskFile(Auth, this);
        }
        private NetdiskFolder HaveNetdiskFolder(List<NetdiskFolder> list, ulong fs_id)
        {
            if (list != null)
                foreach (var it in list)
                    if (it.Id == fs_id)
                        return it;
            return new NetdiskFolder(Auth, this);
        }

        internal void AddItem(INetdiskItem item)
        {
            if (item is NetdiskFile file)
            {
                this.files?.Add(file);
                file.parent = this;
            }
            else if (item is NetdiskFolder folder)
            {
                folders?.Add(folder);
                folder.parent = this;
            }
        }
        internal void DeleteItem(INetdiskItem item)
        {
            if (item is NetdiskFile file)
            {
                this.files?.Remove(file);
                file.parent = null;
            }
            else if (item is NetdiskFolder folder)
            {
                folders?.Remove(folder);
                folder.parent = null;
            }
        }
        

        /// <summary>
        /// is type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool IsOfType(NetdiskItemTypes type) => type == NetdiskItemTypes.Folder;


        
        internal static Category GetCategory(int category, string name)
        {
            if (category == 1)
            {
                return Category.Movie;

            }
            else if (category == 3|| IsPicFile(name))
            {
                return Category.Pic;
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode() & Name.GetHashCode() + Path.GetHashCode() | this.DateModified.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is INetdiskItem it)
                return this.Id == it.Id;
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

    /// <summary>
    /// 
    /// </summary>
    public class XpanShareTool
    {
        internal XpanShareClass xpanShareClass { get; }
        private OAuth Auth { get; }
        internal XpanShareTool(OAuth Auth, string privateKey, string uk, string shareid)
        {
            this.Auth = Auth;
            xpanShareClass = new XpanShareClass(privateKey, uk, shareid);
        }
        internal XpanShareTool(OAuth Auth, string shareLink, string pwd)
        {
            this.Auth = Auth;
            xpanShareClass = new XpanShareClass(shareLink, pwd);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<XpanShareFolder> GetRootFolderAsync()
        {
            return AsyncInfo.Run(_ =>
                Task.Run<XpanShareFolder>(async() =>
                {
                    if (await xpanShareClass.InlitializeAsync())
                    {
                        var folder = new XpanShareFolder(xpanShareClass, Auth) { Name = "All", Path = "/", fs_id = 0 };
                        await folder.RefreshAsync();
                        return folder;
                    }
                    throw new Exception("Failed");
                }));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class XpanShareFolder:INetdiskFolder, INetdiskItem, IXpanShareItem
    {
        internal XpanShareClass xpanShareClass { get; }
        private OAuth Auth { get; }
        internal XpanShareFolder(XpanShareClass xpanShareClass, OAuth Auth)
        {
            this.Auth = Auth;
            this.xpanShareClass = xpanShareClass;
        }
        /// <summary>
        /// Gets the date and time when the current item was created.
        /// </summary>
        public DateTimeOffset DateModified { get; internal set; }
        /// <summary>
        /// Gets the name of the item including the file name extension if there is one.
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        /// Gets the fs_id of the file.
        /// </summary>
        public ulong Id => fs_id;

        internal ulong fs_id;

        /// <summary>
        /// Creates a copy of the file in the specified folder.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the copy of the file is created.</param>
        /// <returns></returns>
        public IAsyncAction CopyAsync(INetdiskFolder destinationFolder)
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Moves the current file to the specified folder。
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the file is moved.</param>
        /// <returns></returns>
        public IAsyncAction MoveAsync(INetdiskFolder destinationFolder)
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Renames the file according to the desired name.
        /// </summary>
        /// <param name="desiredName">The desired name of the file after it is renamed.</param>
        /// <returns></returns>
        public IAsyncAction RenameAsync(string desiredName)
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Deletes the current file.
        /// </summary>
        /// <returns></returns>
        public IAsyncAction DeleteAsync()
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Determines whether the current INetdiskItem matches the specified NetdiskItemTypes
        /// </summary>
        /// <param name="type">The value to match against.</param>
        /// <returns></returns>
        public bool IsOfType(NetdiskItemTypes type) => type == NetdiskItemTypes.Folder;
        /// <summary>
        /// Share the item.
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<NetdiskItemShareInfo> ShareAsync()
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="desiredName"></param>
        /// <returns></returns>
        public IAsyncOperation<INetdiskFolder> CreateFolderAsync(string desiredName)
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IAsyncOperation<INetdiskFile> GetFileAsync(string name)
        {
            return AsyncInfo.Run(_ =>
             Task.Run<INetdiskFile>(async () =>
             {
                 var items = await GetFilesAsync();

                 foreach (var it in items)
                 {
                     if (it.Name == name)
                         return it;
                 }
                 return null;
             }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IAsyncOperation<INetdiskFolder> GetFolderAsync(string name)
        {
            return AsyncInfo.Run(_ =>
              Task.Run<INetdiskFolder>(async () =>
              {
                  var items = await GetFoldersAsync();

                  foreach (var it in items)
                  {
                      if (it.Name == name)
                          return it;
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
                   var items = await GetItemsAsync();

                   foreach (var it in items)
                   {
                       if (it.Name == name)
                           return it;
                   }
                   return null;
               }));
        }
        private  List<XpanShareFolder> folders;
        private List<XpanShareFile> files;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncAction RefreshAsync()
        {
            return AsyncInfo.Run(_ =>
                Task.Run(async () =>
                {
                    var it = await this.xpanShareClass.GetListAsync(fs_id, Path);
                    if (it != null)
                    {
                        switch(it.errno)
                        {
                            case -21:
                                throw new Exception("分享的文件已经被取消了或此链接分享内容可能因为涉及侵权、色情、反动、低俗等信息，无法访问!");
                            case 0:
                                if (it?.list != null)
                                {
                                    folders = new List<XpanShareFolder>();
                                    files = new List<XpanShareFile>();
                                    foreach (var _it in it.list)
                                    {
                                        if (_it.isdir > 0)
                                        {
                                            folders.Add(new XpanShareFolder(xpanShareClass, Auth)
                                            {
                                                Name = _it.server_filename,
                                                fs_id = _it.fs_id,
                                                Path = _it.path,
                                                DateModified = EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(_it.server_mtime),
                                            });
                                        }
                                        else
                                        {
                                            files.Add(new XpanShareFile(xpanShareClass, Auth)
                                            {
                                                Name = _it.server_filename,
                                                fs_id = _it.fs_id,
                                                Size = _it.size,
                                                Path = _it.path,
                                                Md5 = _it.md5,
                                                thumbs = _it.thumbs,
                                                DateModified = EveryThingSampleTools.UWP.Tools.TimeTool.ConvertToDateTime(_it.server_mtime),

                                            });
                                        }
                                    }
                                }
                                break;
                            default:
                                throw new Exception("Fail");
                        }
                    }
                    else
                        throw new Exception("Net Error");
                }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<IReadOnlyList<INetdiskFile>> GetFilesAsync()
        {
            return AsyncInfo.Run(_ =>
               Task.Run<IReadOnlyList<INetdiskFile>>(async () =>
               {
                   await GetItemsAsync();
                   var res = new INetdiskFile[files.Count];
                   int i = 0;
                   foreach (var it in files)
                   {
                       res[i++] = it;
                   }
                   return Array.AsReadOnly(res);
               }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<IReadOnlyList<INetdiskFolder>> GetFoldersAsync()
        {
            return AsyncInfo.Run(_ =>
                  Task.Run<IReadOnlyList<INetdiskFolder>>(async () =>
                  {
                      await GetItemsAsync();
                      var res = new INetdiskFolder[folders.Count];
                      int i = 0;
                      foreach (var it in folders)
                      {
                          res[i++] = it;
                      }
                      return Array.AsReadOnly(res);
                  }));

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
                       await RefreshAsync();
                   var res = new List<INetdiskItem>();
                   res.AddRange(folders);
                   res.AddRange(files);
                   return res.AsReadOnly();

               }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="desFolder"></param>
        /// <returns></returns>
        public IAsyncAction SaveItemAsync(INetdiskFolder desFolder)
        {
            return AsyncInfo.Run(_ =>
                Task.Run(async () =>
                {
                    if (await this.xpanShareClass.SaveFilesAsync(new ulong[] { this.Id }, desFolder.Path, Auth.AccessToken) != 0)
                        throw new Exception("fail");
                    var q = desFolder.RefreshAsync();
                }));
        }
    }

    public class XpanShareFile : INetdiskFile, INetdiskItem, IXpanShareItem
    {
        internal XpanShareClass xpanShareClass { get; }
        private OAuth Auth { get; }
        internal XpanShareFile(XpanShareClass xpanShareClass, OAuth Auth)
        {
            this.Auth = Auth;
            this.xpanShareClass = xpanShareClass;
        }
        internal ulong fs_id { get; set; }
        /// <summary>
        /// Gets the date and time when the current item was created.
        /// </summary>
        public DateTimeOffset DateModified { get; internal set; }
        /// <summary>
        /// Gets the name of the item including the file name extension if there is one.
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        /// Gets the fs_id of the file.
        /// </summary>
        public ulong Id => fs_id;

        /// <summary>
        /// Get type of file.
        /// </summary>
        public string FileType { get { if (Name != null && Name.Contains('.') && !Name.EndsWith('.')) return Name.Substring(Name.LastIndexOf('.')); return null; } }
        /// <summary>
        /// get md5 of file
        /// </summary>
        public string Md5 { get; internal set; }
        /// <summary>
        /// get category of file
        /// </summary>
        public Category Category { get; }

        /// <summary>
        /// Get size of file.
        /// </summary>
        public ulong Size { get; internal set; }

        internal NetdiskBasic.Thumbs thumbs { get; set; }
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
        /// <summary>
        /// Get music stream uri.
        /// </summary>
        /// <returns></returns>
        public Uri GetMusicStreamUri() { throw new Exception("Not support"); }
        /// <summary>
        /// Size can't more than 5MB
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<byte[]> GetFilebytesAsync() { throw new Exception("Not support"); }
        /// <summary>
        /// Get video stream uri, only the file is movie file.
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<VideoStreamInfo> GetVideoStreamInfoAsync() { throw new Exception("Not support"); }
        /// <summary>
        /// Get video stream uri, only the file is movie file.
        /// </summary>
        /// <param name="adToken"></param>
        /// <returns></returns>
        public IAsyncOperation<VideoStreamInfo> GetVideoStreamInfoAsync(string adToken) { throw new Exception("Not support"); }
        /// <summary>
        /// Get download uri of dile.
        /// </summary>
        /// <returns></returns>
        public Windows.Foundation.IAsyncOperation<Uri> GetDownloadUriAsync() { throw new Exception("Not support"); }
        /// <summary>
        /// Creates a copy of the file in the specified folder.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the copy of the file is created.</param>
        /// <returns></returns>
        public IAsyncAction CopyAsync(INetdiskFolder destinationFolder)
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Moves the current file to the specified folder。
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the file is moved.</param>
        /// <returns></returns>
        public IAsyncAction MoveAsync(INetdiskFolder destinationFolder)
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Renames the file according to the desired name.
        /// </summary>
        /// <param name="desiredName">The desired name of the file after it is renamed.</param>
        /// <returns></returns>
        public IAsyncAction RenameAsync(string desiredName)
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Deletes the current file.
        /// </summary>
        /// <returns></returns>
        public IAsyncAction DeleteAsync()
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// Determines whether the current INetdiskItem matches the specified NetdiskItemTypes
        /// </summary>
        /// <param name="type">The value to match against.</param>
        /// <returns></returns>
        public bool IsOfType(NetdiskItemTypes type) => type == NetdiskItemTypes.Folder;
        /// <summary>
        /// Share the item.
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<NetdiskItemShareInfo> ShareAsync()
        {
            throw new Exception("Not support");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="desFolder"></param>
        /// <returns></returns>
        public IAsyncAction SaveItemAsync(INetdiskFolder desFolder)
        {
            return AsyncInfo.Run(_ =>
                Task.Run(async () =>
                {
                    if (await this.xpanShareClass.SaveFilesAsync(new ulong[] { this.Id }, desFolder.Path, Auth.AccessToken) != 0)
                        throw new Exception("fail");
                    var q = desFolder.RefreshAsync();
                }));
        }

    }
}
