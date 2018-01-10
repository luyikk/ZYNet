﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.SocketClient;

namespace FileServ.Client
{
    public class FileClient
    {
        public CloudClient client { get; set; }

        public string Current { get; set; } = null;

        public FileClient()
        {
            client = new CloudClient(new SocketClient(), 10000, 1024 * 1024); //最大数据包能够接收 1M
            ClientPackHander tmp = new ClientPackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;          
        }

        public bool Connect(string IP)
        {
            return client.Connect(IP, 9557);
        }
     

        private void Client_Disconnect(string obj)
        {
            Console.WriteLine(obj);
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public void PrintCmd()
        {
            Console.WriteLine("CMD:\r\n");
            Console.WriteLine(" cd [path]\t (Display the current directory name or change the current directory. )\r\n   Example: cd\r\n   Example: cd /bin");
            Console.WriteLine(" ls [path]\tor\tdir [path]\t(show path files)\r\n   Example: ls /\r\n   Example: dir c:\\\r\n   Example: ls (please use CD set current path)\r\n   Example: dir (please use CD set current path)");
            Console.WriteLine(" push [source] [target] \t (push file to path)\r\n   Example: push d:/a.txt /home/a.txt \r\n   Example: push d:/a.txt (please use CD set current path)");
            Console.WriteLine(" get [target] [source]\t (get file to path)\r\n   Example: get /home/a.txt d:/a.txt \r\n   Example: get a.txt d:/a.txt (please use CD set current path)");
            Console.WriteLine(" img [source] [target]\t (img dir to path)\r\n   Example: img c:/abc /home/ \r\n   Example: img c:/abc (please use CD set current path)");
            Console.WriteLine(" mv [source] [target]\t (move file or rename file)\r\n   Example: mv /home/a.txt /home/b.txt \r\n   Example: mv a.txt b.txt (please use CD set current path)");
            Console.WriteLine(" mkdir [path]\t (create directory to path)\r\n   Example: mkdir /home/ccc \r\n   Example: mkdir xxx(please use CD set current path)");
            Console.WriteLine(" rm [file]\t (delete file)\r\n   Example: rm /home/ccc \r\n   Example: rm xxx.txt(please use CD set current path)");
            Console.WriteLine(" close (close current connect)");
        }

        public  bool  Cmd(string cmdarg)
        {
            if (string.IsNullOrEmpty(cmdarg))
                return false ;

            string[] cmd = null;
          
            var t= cmdarg.IndexOf(' ');

            if (t == -1)
                cmd = new string[1] { cmdarg };
            else
            {
                var tag = cmdarg.Substring(0, t + 1).Trim();
                var arg = cmdarg.Substring(t + 1, cmdarg.Length - (t+1)).Trim();

                cmd = new string[2] { tag, arg };
            }
                       

            switch (cmd[0].ToLower())
            {
                case "cd":
                    {
                        if (cmd.Length == 1)
                            Console.WriteLine(Current ?? "not set path");
                        else if(cmd.Length==2)
                            CurrentDir(cmd[1]);
                        else
                        {
                            string array = cmd[1];
                            for (int i=2;i<cmd.Length;i++ )
                            {
                                array += cmd[i];
                            }

                            CurrentDir(array);
                        }

                    }
                    break;
                case "ls":
                case "dir":
                    {
                        if (cmd.Length == 1)
                            LsOrDir();
                        else
                            LsOrDir(cmd[1]);
                    }
                    break;                
                case "push":
                    {
                        if (cmd.Length == 2)
                        {
                            string[] uparg = cmd[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            if (uparg.Length == 1)
                                 UpFile(uparg[0], Current + "/");
                            else if (uparg.Length == 2)
                                 UpFile(uparg[0], uparg[1]);


                        }
                    }
                    break;
                case "get":
                    {
                        if (cmd.Length == 2)
                        {
                            string[] uparg = cmd[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (uparg.Length == 2)
                                GetFile(uparg[0], uparg[1]);
                        }

                    }
                    break;
                case "img":
                    {
                        if (cmd.Length == 2)
                        {
                            string[] uparg = cmd[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (uparg.Length == 1)
                                ImageDirectory(uparg[0], "");
                            else if (uparg.Length == 2)
                                ImageDirectory(uparg[0], uparg[1]);

                        }
                    }
                    break;
                case "mv":
                    {
                        if (cmd.Length == 2)
                        {
                            string[] mvarg = cmd[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            if (mvarg.Length == 2)
                                MvFile(mvarg[0], mvarg[1]);
                        }
                    }
                    break;
                case "mkdir":
                    {
                        if (cmd.Length == 2)
                        {
                            string[] mkdirarg = cmd[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (mkdirarg.Length == 1)
                                Mkdir(mkdirarg[0]);

                        }
                    }
                    break;
                case "rm":
                    {
                        if (cmd.Length == 2)
                        {
                            string[] rmarg = cmd[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (rmarg.Length == 1)
                                Rm(rmarg[0]);

                        }
                    }
                    break;
                case "close":
                    {
                        client.Close();
                        return true;
                    }


            }

            return false;

        }

     

        protected void CurrentDir(string path=null)
        {
            if (path.Equals("..", StringComparison.Ordinal))
            {
                var tmpPath = Path.GetDirectoryName(Current)?? Current;

                tmpPath = tmpPath.Replace("\\", "/");

                if (client.Sync.Get<IServer>().ExistsDir(tmpPath))
                {
                    Current = tmpPath;
                    Console.WriteLine(Current);
                }
                else
                {
                    Console.WriteLine($"not find {tmpPath}");
                    Console.WriteLine(Current);
                }

            }
            else if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine(Current);
            }
            else
            {
                if (!string.IsNullOrEmpty(Current))
                {
                    var tmpPath = client.Sync.Get<IServer>().CombinePath(Current, path);

                    if (tmpPath != null)
                    {
                        tmpPath = tmpPath.Replace("\\", "/");

                        if (client.Sync.Get<IServer>().ExistsDir(tmpPath))
                        {
                            Current = tmpPath;
                            Console.WriteLine(Current);
                        }
                        else
                        {
                            Console.WriteLine($"not find {tmpPath}");
                            Console.WriteLine(Current);
                        }
                    }
                }
                else
                {
                    if (client.Sync.Get<IServer>().ExistsDir(path))
                    {
                        path = path.Replace("\\", "/");
                        Current = path;
                        Console.WriteLine(Current);
                    }
                    else
                    {
                        Console.WriteLine($"not find {path}");
                        Console.WriteLine(Current);
                    }

                }
            }

        }


        protected  void LsOrDir(string path = null)
        {
            if (path != null)            
                 PrintDir(path);            
            else
            {
                if (Current is null)
                    Console.WriteLine("please use CD set current path");
                else
                     PrintDir(Current);
            }

        }

        protected async void PrintDir(string path)
        {

            var res = await client.NewAsync().Get<IServer>().LsOrDir(path);
            if (res != null && res.Length > 0)
            {
                var filesystem = res.As<List<FileSysInfo>>(0);

                if (filesystem != null && filesystem.Count > 0)
                    foreach (var item in filesystem)
                    {
                        Console.WriteLine(item);
                    }
            }
        }

        protected async Task UpFile(string source, string target)
        {
            var sourceFileName = Path.GetFileName(source);
            var targetFileName = Path.GetFileName(target);

            if (string.IsNullOrEmpty(sourceFileName))
            {
                Console.WriteLine($"1 Not Find {sourceFileName}");
                return;
            }
            if (string.IsNullOrEmpty(targetFileName))
            {
                if(string.IsNullOrEmpty(Current))
                {
                    Console.WriteLine("please use CD set current path");
                    return;
                }
                target += sourceFileName;
            }
            if (!File.Exists(source))
            {
                Console.WriteLine($"2 not find {sourceFileName}");
                return;
            }

            var Async = client.NewAsync();
            var Sync = client.Sync.Get<IServer>();

            var Serv = Async.Get<IServer>();

            var res = await Serv.CreateFile(target);

            if (res is null || res.IsError)
            {
                Console.WriteLine($"create {target} Error:{res.ErrorMsg}");
                return;
            }

            if (res is null || !res.IsHaveValue)
            {
                Console.WriteLine($"create {target} faill");
                return;
            }
            int fileId = res.As<int>(0);

            using (FileStream stream = File.OpenRead(source))
            {
                int count;
                int check = 10;

                double lengt = stream.Length;
                double current = 0;
                byte t = 0;

                do
                {
                    byte[] data = new byte[8192];
                    long offset = stream.Position;
                    count = await stream.ReadAsync(data, 0, data.Length);

                    if (count == 0)
                        break;


                    Re:
                    try
                    {
                        bool isOk = Sync.WriteFile(fileId, data, count, offset,  CRC32.GetCRC32(data));

                        while (!isOk)
                        {
                            isOk = Sync.WriteFile(fileId, data, count, offset,  CRC32.GetCRC32(data));
                        }

                    }
                    catch (Exception er)
                    {
                        if (er.InnerException != null && er.InnerException is TimeoutException)
                        {
                            Console.WriteLine("write time out  retry write");
                            check--;

                            if (check <= 0)
                            {
                                Console.WriteLine($"send file time Out {source}");
                                break;
                            }
                            goto Re;
                        }
                        else
                        {
                            Console.WriteLine($"send file Error {source} : {er.ToString()}");
                            break;
                        }
                    }

                    check = 10;
                    current += count;

                    t++;
                    if (t >= 255)
                    {
                        Console.CursorLeft = 0;
                        Console.Write($"{source} Send: {current} { Math.Round(current / lengt * 100.0, 0)}%");
                    }

                } while (count > 0);
                Console.CursorLeft = 0;
                Console.Write($"{source} Send: {current} 100%");

                Sync.CloseFile(fileId);

                Console.WriteLine($"\r\n{source} push close");
            }
        }

        protected async void GetFile(string target, string source)
        {
            if (string.IsNullOrEmpty(target))
            {
                Console.WriteLine($"1 Not Find {target}");
                PrintCmd();
                return;
            }

            if (string.IsNullOrEmpty(source))
            {
                Console.WriteLine($"1 Not Find {source}");
                PrintCmd();
                return;
            }
            target = target.Replace("\\", "/");
            var targetdir = Path.GetDirectoryName(target);

            if (string.IsNullOrEmpty(targetdir))
                if (!string.IsNullOrEmpty(Current))
                {
                    target =Path.Combine( Current , target);
                    target = target.Replace("\\", "/");
                }
                else
                {
                    Console.WriteLine("please use CD set current path");
                    return;
                }

            var sourcedir = Path.GetDirectoryName(source);

            if (!string.IsNullOrEmpty(sourcedir))
            {
                if (!Directory.Exists(sourcedir))
                {
                    try { Directory.CreateDirectory(sourcedir); }
                    catch
                    {
                        Console.WriteLine($"not create dir:{sourcedir}");
                        return;
                    }
                }
            }


            var Async = client.NewAsync();
            var Sync = client.Sync.Get<IServer>();

            var Serv = Async.Get<IServer>();

            var res = await Serv.GetFile(target);

            if (res is null || res.IsError)
            {
                Console.WriteLine($"Get {target} Error:{res.ErrorMsg}");
                return;
            }

            if (res is null || !res.IsHaveValue)
            {
                Console.WriteLine($"Get {target} faill");
                return;
            }

            int fileId = res.As<int>(0);
            double lengt = res.As<long>(1);         
            double current = 0;
            byte t = 0;

            try
            {
                using (FileStream stream = File.Open(source, FileMode.Create, FileAccess.Write))
                {
                    while (true)
                    {
                        int check = 10;
                        Re:
                        var resvalue = await Serv.GetFileData(fileId, stream.Position);

                        if (resvalue is null)
                        {
                            Console.WriteLine($"Get {target} faill");
                            break;
                        }

                        if (resvalue.IsError)
                        {
                            Console.WriteLine($"Get {target} Error:{res.ErrorMsg}");
                            break;
                        }

                        if (!resvalue.IsHaveValue||resvalue.Length != 3)
                        {
                            Console.WriteLine($"Get {target} faill");
                            break;
                        }

                        var data = resvalue[0].Value<byte[]>();
                        var len = resvalue[1].Value<int>();
                        var crc= resvalue[2].Value<uint>();

                        if (len == 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine($"Get {target} OK");
                            break;
                        }


                        if(crc!=CRC32.GetCRC32(data))
                        {
                            check--;
                            if (check <= 0)
                            {
                                Console.WriteLine($"Get {target} faill");
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"Get file {source} Data CRC32 Error");
                                goto Re;
                            }
                        }

                        
                        await stream.WriteAsync(data, 0, len);

                        current += resvalue[1].Value<int>();
                        t++;
                        if (t >= 255)
                        {
                            Console.CursorLeft = 0;
                            Console.Write($"{source} Get: {current} { Math.Round(current / lengt * 100.0, 0)}%");
                        }

                    }

                    Sync.CloseGetFile(fileId);
                }
            }
            catch (Exception er)
            {
                Console.WriteLine($"Get {source} faill:{er}");
                Sync.CloseGetFile(fileId);
                return;
            }


        }

        protected  async void ImageDirectory(string source,string target)
        {
            if(string.IsNullOrEmpty(target))
            {
                if(!string.IsNullOrEmpty(Current))                
                    target = Current;
                else
                {
                    Console.WriteLine("please use CD set current path");
                    return;
                }

            }
            if(!Directory.Exists(source))
            {
                Console.WriteLine($"not find directory {source}");
                return;
            }
            

            await Img(new DirectoryInfo(source), target);
            Console.WriteLine($"img {source} to {target} close");
        }

        protected async Task Img(DirectoryInfo dir,string target)
        {
            var Serv = client.NewAsync().Get<IServer>();
            var Sync = client.Sync.Get<IServer>();

            var dirname = dir.Name;
            target = Path.Combine(target, dirname);
            target = target.Replace("\\", "/");
            var dirres = Sync.CreateDirectory(target);

            if (dirres)
            {

                foreach (var item in dir.GetFileSystemInfos())
                {
                    if (item is DirectoryInfo diritem)
                    {
                        await Img(diritem, target);
                    }
                    else if (item is FileInfo fileitem)
                    {
                        var filepath = Path.Combine(target, item.Name);
                        filepath = filepath.Replace("\\", "/");
                        await UpFile(fileitem.FullName, filepath);
                    }
                }
            }
          
        }

        protected async void MvFile(string source, string target)
        {
            var a = Path.GetDirectoryName(source);
            var b= Path.GetDirectoryName(target);
            if (string.IsNullOrEmpty(a))            
                source = Path.Combine(Current, source).Replace("\\","/");

            if (string.IsNullOrEmpty(b))
                target = Path.Combine(Current, target).Replace("\\", "/");


            var Async = client.NewAsync().Get<IServer>();
            var res = await Async.MvFile(source, target);

            
            if (res is null)
            {
                Console.WriteLine($"mv {source} {target} faill");
                return;
            }

            if(res.IsError)
            {
                Console.WriteLine($"mv {source} {target} Is Error: {res.ErrorMsg}");
                return;
            }

            if(!res.IsHaveValue)
            {
                Console.WriteLine($"mv {source} {target} faill");
                return;
            }

            Console.WriteLine($"mv {source} {target} Close");


        }

        private async void Mkdir(string file)
        {
            var a = Path.GetDirectoryName(file);
           
            if (string.IsNullOrEmpty(a))
                file = Path.Combine(Current, file).Replace("\\", "/");

            var Async = client.NewAsync().Get<IServer>();
            var res = await Async.MkDir(file);


            if (res is null)
            {
                Console.WriteLine($"mkdir {file} faill");
                return;
            }

            if (res.IsError)
            {
                Console.WriteLine($"mkdir {file} Is Error: {res.ErrorMsg}");
                return;
            }

            if (!res.IsHaveValue)
            {
                Console.WriteLine($"mkdir {file} faill");
                return;
            }

            Console.WriteLine($"mkdir {file} Close");
        }


        private async void Rm(string file)
        {
            var a = Path.GetDirectoryName(file);

            if (string.IsNullOrEmpty(a))
                file = Path.Combine(Current, file).Replace("\\", "/");

            var Async = client.NewAsync().Get<IServer>();
            var res = await Async.Rm(file);


            if (res is null)
            {
                Console.WriteLine($"rm {file} faill");
                return;
            }

            if (res.IsError)
            {
                Console.WriteLine($"rm {file} Is Error: {res.ErrorMsg}");
                return;
            }

            if (!res.IsHaveValue)
            {
                Console.WriteLine($"rm {file} faill");
                return;
            }

            Console.WriteLine($"rm {file} Close");
        }
    }
}