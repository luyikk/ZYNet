using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Server;
using System.IO;
using ZYNet.CloudSystem.Frame;
using System.Threading.Tasks;

namespace FileServ.Server
{
    public class ServerPackHandler
    {


        [TAG(1000)]
        public static bool LogOn(ASyncToken token)
        {
            UserInfo user = new UserInfo()
            {
                Async = token
            };
            token.UserToken = user;
            token.UserDisconnect += Token_UserDisconnect;

            return true;
        }

        private static void Token_UserDisconnect(ASyncToken arg1, string arg2)
        {
            if (arg1.UserToken != null)
            {
                var user = arg1.Token<UserInfo>();
                if (user != null && user.FilePushDictionary.Count > 0)
                {
                    foreach (var item in user.FilePushDictionary.Values)
                    {
                        item.Dispose();
                    }
                }

                if (user != null && user.FileGetDictionary.Count > 0)
                {
                    foreach (var item in user.FileGetDictionary.Values)
                    {
                        item.Dispose();
                    }
                }
            }
        }

        [TAG(1001)]
        public static string CombinePath(ASyncToken token, string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        [TAG(10001)]
        public static bool ExistsDir(ASyncToken token, string path)
        {
            return Directory.Exists(path);
        }

        [TAG(10002)]
        public static Task<ReturnResult> LsOrDir(AsyncCalls async, string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            if (directory.Exists)
            {
                var files = directory.GetFileSystemInfos();

                List<FileSysInfo> res = new List<FileSysInfo>();

                foreach (var item in files)
                {
                    FileSysInfo fileSysInfo = new FileSysInfo()
                    {
                        fileType = item is FileInfo ? FileType.File : FileType.Dir,
                        Name = item.Name,
                        FullName = item.FullName,
                        CreateTime = item.CreationTime,
                        LastAccessTime = item.LastAccessTime,
                        LastWriteTime = item.LastWriteTime,
                        Length = item is FileInfo wr ? wr.Length : 0
                    };

                    res.Add(fileSysInfo);
                }

                res.Sort((a, b) =>
                {
                    if (a.fileType == b.fileType)
                        return 0;
                    else if (a.fileType == FileType.Dir)
                        return -1;
                    else
                        return 1;

                });

                FileSysInfo current = new FileSysInfo()
                {
                    fileType = FileType.Dir,
                    Name = ".",
                    FullName = directory.FullName,
                    CreateTime = directory.CreationTime,
                    LastAccessTime = directory.LastAccessTime,
                    LastWriteTime = directory.LastWriteTime,
                    Length = 0
                };

                res.Insert(0, current);

                if (directory.Parent != null)
                {
                    FileSysInfo parent = new FileSysInfo()
                    {
                        fileType = FileType.Dir,
                        Name = "..",
                        FullName = directory.Parent.FullName,
                        CreateTime = directory.Parent.CreationTime,
                        LastAccessTime = directory.Parent.LastAccessTime,
                        LastWriteTime = directory.Parent.LastWriteTime,
                        Length = 0
                    };

                    res.Insert(1, parent);
                }

                return Task.FromResult<ReturnResult>(async.RET(res));
            }
            else
                return Task.FromResult<ReturnResult>(async.RET());
        }

        [TAG(10003)]
        public static Task<ReturnResult> CreateFile(AsyncCalls async, string path)
        {
            if (async.UserToken != null)
            {
                var user = async.Token<UserInfo>();


                var stream = File.Open(path, FileMode.Create, FileAccess.Write);
                var key = ++user.Key;
                if (user.FilePushDictionary.TryAdd(key, stream))
                {
                    Console.WriteLine($"UP File {path}");
                    return Task.FromResult<ReturnResult>(async.RET(key));
                }
                else
                {
                    stream.Dispose();
                    return Task.FromResult<ReturnResult>(async.RET());
                }


            }
            else
                return Task.FromResult<ReturnResult>(async.RET());
        }

        [TAG(10004)]
        public static bool WriteFile(ASyncToken token, int fileID, byte[] data, long offset, uint crc)
        {


            if (token.UserToken != null)
            {

                var user = token.Token<UserInfo>();

                if (CRC32.GetCRC32(data) != crc)
                    return false;

                var Stream = user.FilePushDictionary[fileID];
                Stream.Position = offset;
                Stream.Write(data, 0, data.Length);

                return true;
            }

            return false;
        }

        [TAG(10005)]
        public static void FileClose(ASyncToken token, int fileID)
        {
            if (token.UserToken != null)
            {
                var user = token.Token<UserInfo>();

                if (user.FilePushDictionary.ContainsKey(fileID))
                    user.FilePushDictionary[fileID].Dispose();

            }
        }

        [TAG(10006)]
        public static Task<ReturnResult> GetFile(AsyncCalls async, string path)
        {
            if (async.UserToken != null)
            {
                var user = async.Token<UserInfo>();


                if (!File.Exists(path))
                    throw new FileNotFoundException($"not Find file{path}");

                var stream = File.Open(path, FileMode.Open, FileAccess.Read);
                var key = ++user.Key;
                if (user.FileGetDictionary.TryAdd(key, stream))
                {
                    Console.WriteLine($"Get File {path}");
                    return Task.FromResult<ReturnResult>(async.RET(key,stream.Length));
                }
                else
                {
                    stream.Dispose();
                    return Task.FromResult<ReturnResult>(async.RET());
                }


            }

            return Task.FromResult<ReturnResult>(async.RET());
        }

        [TAG(10007)]
        public static void FileGetClose(ASyncToken token, int fileID)
        {
            if (token.UserToken != null)
            {
                var user = token.Token<UserInfo>();
                if (user.FileGetDictionary.ContainsKey(fileID))
                    user.FileGetDictionary[fileID].Dispose();

            }
        }


        [TAG(10008)]
        public static async Task<ReturnResult> GetFileData(AsyncCalls async, int fileId, long postion)
        {
            if (async.UserToken is UserInfo user)
            {
                if (user.FileGetDictionary.ContainsKey(fileId))
                {
                    var stream = user.FileGetDictionary[fileId];

                    stream.Position = postion;

                    byte[] data = new byte[4096];

                    int count = await stream.ReadAsync(data, 0, data.Length);

                    return async.RET(data, count,CRC32.GetCRC32(data));
                }
            }

            return async.RET();
        }
    }
}
