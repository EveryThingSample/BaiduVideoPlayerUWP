using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace EveryThingSampleTools.UWP.NetdiskOpenAPI
{
    public enum NetdiskItemTypes
    {
        None,
        Folder,
        File,
    }
    /// <summary>
    /// 
    /// </summary>
    public interface INetdiskItem
    {
        /// <summary>
        /// Creates a copy of the file in the specified folder.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the copy of the file is created.</param>
        /// <returns></returns>
        IAsyncAction CopyAsync(INetdiskFolder destinationFolder);
        /// <summary>
        /// Moves the current file to the specified folder。
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the file is moved.</param>
        /// <returns></returns>
        IAsyncAction MoveAsync(INetdiskFolder destinationFolder);
        /// <summary>
        /// Renames the file according to the desired name.
        /// </summary>
        /// <param name="desiredName">The desired name of the file after it is renamed.</param>
        /// <returns></returns>
        IAsyncAction RenameAsync(string desiredName);
        /// <summary>
        /// Deletes the current file.
        /// </summary>
        /// <returns></returns>
        IAsyncAction DeleteAsync();
        /// <summary>
        /// Determines whether the current INetdiskItem matches the specified NetdiskItemTypes
        /// </summary>
        /// <param name="type">The value to match against.</param>
        /// <returns></returns>
        bool IsOfType(NetdiskItemTypes type);
        /// <summary>
        /// Share the item.
        /// </summary>
        /// <returns></returns>
        IAsyncOperation<NetdiskItemShareInfo> ShareAsync();
        /// <summary>
        /// Gets the date and time when the current item was created.
        /// </summary>
        DateTimeOffset DateModified { get; }
        /// <summary>
        /// Gets the name of the item including the file name extension if there is one.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        string Path { get; }
        /// <summary>
        /// Gets the fs_id of the file.
        /// </summary>
        ulong Id { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public interface INetdiskItem2 : INetdiskItem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IAsyncOperation<INetdiskFolder> GetParentAsync();

    }
    /// <summary>
    /// 
    /// </summary>
    public interface INetdiskFolder : INetdiskItem
    {
        IAsyncOperation<INetdiskFolder> CreateFolderAsync(string desiredName);
        IAsyncOperation<INetdiskFile> GetFileAsync(string name);
        IAsyncOperation<INetdiskFolder> GetFolderAsync(string name);
        // IAsyncOperation<NetdiskFolder> CreateFolderAsync(string desiredName, Windows.Storage.CreationCollisionOption options);
        IAsyncOperation<INetdiskItem> GetItemAsync(string name);

        IAsyncAction RefreshAsync();
        IAsyncOperation<IReadOnlyList<INetdiskFile>> GetFilesAsync();

        IAsyncOperation<IReadOnlyList<INetdiskFolder>> GetFoldersAsync();

        IAsyncOperation<IReadOnlyList<INetdiskItem>> GetItemsAsync();
    }
    /// <summary>
    /// 
    /// </summary>
    public interface INetdiskFile : INetdiskItem
    {
        /// <summary>
        /// Get music stream uri.
        /// </summary>
        /// <returns></returns>
        Uri GetMusicStreamUri();
        /// <summary>
        /// Size can't more than 5MB
        /// </summary>
        /// <returns></returns>
        IAsyncOperation<byte[]> GetFilebytesAsync();
        /// <summary>
        /// Get video stream uri, only the file is movie file.
        /// </summary>
        /// <returns></returns>
        IAsyncOperation<VideoStreamInfo> GetVideoStreamInfoAsync();
        /// <summary>
        /// Get video stream uri, only the file is movie file.
        /// </summary>
        /// <param name="adToken"></param>
        /// <returns></returns>
        IAsyncOperation<VideoStreamInfo> GetVideoStreamInfoAsync(string adToken);
        /// <summary>
        /// Get download uri of dile.
        /// </summary>
        /// <returns></returns>
        Windows.Foundation.IAsyncOperation<Uri> GetDownloadUriAsync();
        /// <summary>
        /// Get type of file.
        /// </summary>
        string FileType { get; }
        /// <summary>
        /// get md5 of file
        /// </summary>
        string Md5 { get; }
        /// <summary>
        /// get category of file
        /// </summary>
        Category Category { get; }
        /// <summary>
        /// Get url of thumb.
        /// </summary>
        /// <returns></returns>
        string GetThumbUrl();
        /// <summary>
        /// Get size of file.
        /// </summary>
        ulong Size { get; }

    }
    /// <summary>
    /// 
    /// </summary>
    public enum Category
    {
        /// <summary>
        /// 其他
        /// </summary>
        Other = 0,
        /// <summary>
        /// 视频
        /// </summary>
        Movie = 1,
        /// <summary>
        /// 音频
        /// </summary>
        Music = 2,
        /// <summary>
        /// 图片
        /// </summary>
        Pic= 3,
        /// <summary>
        /// 文档
        /// </summary>
        Doc = 4,
        /// <summary>
        /// 表格
        /// </summary>
        Xls = 5,
        /// <summary>
        /// 放映
        /// </summary>
        Ppt = 6,
        /// <summary>
        /// 文本
        /// </summary>
        Txt = 7,
        /// <summary>
        /// 压缩
        /// </summary>
        Zip = 8,
        /// <summary>
        /// PDF
        /// </summary>
        Pdf = 9,
    }
    /// <summary>
    /// 
    /// </summary>
    public interface IXpanShareItem
    {
        IAsyncAction SaveItemAsync(INetdiskFolder desFolder);
    }
}
