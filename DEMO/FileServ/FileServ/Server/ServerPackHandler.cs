using System;
using System.Collections.Generic;
using System.Text;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Server;
using System.IO;
using ZYNet.CloudSystem.Frame;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Interfaces;
using Autofac;
using Microsoft.Extensions.Logging;

namespace FileServ.Server
{
    public class ServerPackHandler : ControllerBase
    {
        public ServerPackHandler(IContainer container, ILoggerFactory loggerFactory) : base(container, loggerFactory)
        {
        }

        [TAG(1000)]
        public bool LogOn(IASync token)
        {
            UserInfo user = new UserInfo()
            {
                Async = token.GetAsyncToken()
            };
            token.UserToken = user;
            token.UserDisconnect += Token_UserDisconnect;

            return true;
        }

        private void Token_UserDisconnect(ASyncToken arg1, string arg2)
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
                user.FilePushDictionary.Clear();

                if (user != null && user.FileGetDictionary.Count > 0)
                {
                    foreach (var item in user.FileGetDictionary.Values)
                    {
                        item.Dispose();
                    }
                }

                user.FileGetDictionary.Clear();
            }
        }

        [TAG(1001)]
        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        [TAG(10001)]
        public bool ExistsDir(string path)
        {
            return Directory.Exists(path);
        }

        [TAG(10002)]
        public Task<List<FileSysInfo>> LsOrDir(string path)
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

                return Task.FromResult(res);
            }
            else
                return null;
        }

        [TAG(10003)]
        public Task<int> CreateFile(IASync async, string path)
        {
            if (async.UserToken != null)
            {
                var user = async.Token<UserInfo>();


                var stream = File.Open(path, FileMode.Create, FileAccess.Write);
                var key = ++user.Key;
                if (user.FilePushDictionary.TryAdd(key, stream))
                {
                    Console.WriteLine($"UP File {path}");
                    return Task.FromResult(key);
                }
                else
                {
                    stream.Dispose();
                    return null;
                }


            }
            else
                return null;
        }

        [TAG(10004)]
        public async Task<bool> WriteFile(IASync async, int fileID, byte[] data, int count, long offset, uint crc)
        {


            if (async.UserToken != null)
            {

                var user = async.Token<UserInfo>();

                if (CRC32.GetCRC32(data) != crc)
                    return false;
                if (user.FilePushDictionary.ContainsKey(fileID))
                {
                    var Stream = user.FilePushDictionary[fileID];
                    Stream.Position = offset;

                    await Stream.WriteAsync(data, 0, count);

                    return true;
                }

                return false;
            }

            return false;
        }

        [TAG(10005)]
        public void FileClose(IASync token, int fileID)
        {
            if (token.UserToken != null)
            {
                var user = token.Token<UserInfo>();

                if (user.FilePushDictionary.ContainsKey(fileID))
                {
                    user.FilePushDictionary.TryRemove(fileID, out FileStream cc);
                    cc.Dispose();
                }
            }
        }

        [TAG(10006)]
        public Task<Result> GetFile(IASync async, string path)
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
                    return Task.FromResult<Result>(async.Res(key, stream.Length));
                }
                else
                {
                    stream.Dispose();
                    return Task.FromResult<Result>(async.Res());
                }


            }

            return Task.FromResult<Result>(async.Res());
        }

        [TAG(10007)]
        public void FileGetClose(IASync token, int fileID)
        {
            if (token.UserToken != null)
            {
                var user = token.Token<UserInfo>();


                if (user.FileGetDictionary.ContainsKey(fileID))
                {
                    user.FileGetDictionary.TryRemove(fileID, out FileStream cc);
                    cc.Dispose();
                }

            }
        }


        [TAG(10008)]
        public async Task<Result> GetFileData(IASync async, int fileId, long postion)
        {
            if (async.UserToken is UserInfo user)
            {
                if (user.FileGetDictionary.ContainsKey(fileId))
                {
                    var stream = user.FileGetDictionary[fileId];

                    stream.Position = postion;

                    byte[] data = new byte[4096];

                    int count = await stream.ReadAsync(data, 0, data.Length);

                    return async.Res(data, count, CRC32.GetCRC32(data));
                }
            }

            return async.Res();
        }
        [TAG(10009)]
        public bool CreateDirectory(IASync async, string path)
        {
            if (async.UserToken is UserInfo user)
            {
                if (Directory.Exists(path))
                    return true;
                else
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
            }

            return false;
        }

        [TAG(10010)]
        public Task<bool> MvFile(IASync async, string source, string target)
        {
            if (File.Exists(source))
            {
                File.Move(source, target);
                return Task.FromResult(true);
            }
            else if (Directory.Exists(source))
            {
                Directory.Move(source, target);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        [TAG(10011)]
        public Task<bool> MkDir(IASync async, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return Task.FromResult((true));
            }

            return Task.FromResult((false));
        }

        [TAG(10012)]
        public Task<bool> Rm(IASync async, string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                return Task.FromResult(true);
            }
            else if (Directory.Exists(file))
            {
                Directory.Delete(file, true);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }



        [TAG(10013)]
        public Task<List<Drive_Info>> GetDriveInfo()
        {
            var driveinfos = DriveInfo.GetDrives();
            var drivelist = new List<Drive_Info>();
            foreach (var item in driveinfos)
            {
                Drive_Info tmp = new Drive_Info();

                try
                {
                    tmp.AvailableFreeSpace = item.AvailableFreeSpace;
                }
                catch { }
                try { tmp.DriveFormat = item.DriveFormat; } catch { }
                try { tmp.DriveType = item.DriveType; } catch { }
                try { tmp.IsReady = item.IsReady; } catch { }
                try { tmp.Name = item.Name; } catch { }
                try { tmp.TotalFreeSpace = item.TotalFreeSpace; } catch { }
                try { tmp.TotalSize = item.TotalSize; } catch { }
                try { tmp.VolumeLabel = item.VolumeLabel; } catch { }
                tmp.RootDirectory = item.RootDirectory == null ? null : new FileSysInfo();

                if (tmp.RootDirectory != null)
                {

                    try { tmp.RootDirectory.CreateTime = item.RootDirectory.CreationTime; } catch { }
                    tmp.RootDirectory.fileType = FileType.Dir;
                    tmp.RootDirectory.FullName = item.RootDirectory.FullName;
                    try { tmp.RootDirectory.LastAccessTime = item.RootDirectory.LastAccessTime; } catch { }
                    try { tmp.RootDirectory.LastWriteTime = item.RootDirectory.LastWriteTime; } catch { }

                    tmp.RootDirectory.Name = item.RootDirectory.Name;

                }

                drivelist.Add(tmp);
            }


            return Task.FromResult(drivelist);
        }

        [TAG(10014)]
        public Task<bool> Copy(IASync async, string source, string target)
        {
            if (File.Exists(source))
            {
                File.Copy(source, target, true);
                return Task.FromResult(true);
            }
            else if (Directory.Exists(source))
            {

                return Task.Run(() =>
                {
                    CopyDirectory(async as AsyncCalls, new DirectoryInfo(source), new DirectoryInfo(target));
                    return Task.FromResult(true);
                });

            }

            return Task.FromResult(false);
        }

        public bool CopyDirectory(AsyncCalls async, DirectoryInfo dir, DirectoryInfo target)
        {
            if (target.FullName.IndexOf(dir.FullName) == 0)
                return false;

            DirectoryInfo targetdir = null;

            string targetpath = Path.Combine(target.FullName, dir.Name).Replace("\\", "/");

            if (!Directory.Exists(path: targetpath))
                targetdir = Directory.CreateDirectory(path: targetpath);
            else
                targetdir = new DirectoryInfo(targetpath);


            foreach (var item in dir.GetFileSystemInfos())
            {
                if (item is DirectoryInfo p)
                {
                    CopyDirectory(async, p, new DirectoryInfo(targetdir.FullName));
                }
                else if (item is FileInfo x)
                {
                    var file = Path.Combine(targetdir.FullName, x.Name).Replace("\\", "/");
                    x.CopyTo(file, true);
                    async.AsyncUser.Action(20000, file + " Copy OK!");
                }
            }

            return true;
        }
    }
}
