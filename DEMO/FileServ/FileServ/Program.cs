using FileServ.Client;
using System;
using System.IO;
using ZYNet.CloudSystem.Loggine;

namespace FileServ
{
    class Program
    {
        static  void Main(string[] args)
        {
            if(args.Length==0)
                RunCmd();
            else if(args[0].Equals("SERVER",StringComparison.OrdinalIgnoreCase))
            {
                LogFactory.AddConsole();
                FileServ.Server.FileServer.Server.Start();
                while (true)
                    Console.ReadLine();
            }
        }

        static  void RunCmd()
        {
            Top1:
            Console.WriteLine("Whether to open the file service ?\r\n1   Start(default)\r\n2   Skip");

            string select = Console.ReadLine();

            if (string.IsNullOrEmpty(select) || select.Equals("1", StringComparison.Ordinal))
            {
                FileServ.Server.FileServer.Server.Start();
                Console.WriteLine("Service Is Start...");
            }
            else if (!select.Equals("2", StringComparison.Ordinal))
                goto Top1;


            Top2:

            Console.WriteLine("Whether to open the Logging?\r\n 1   No(default)\r\n 2   Yes");
            select = Console.ReadLine();

            if (string.IsNullOrEmpty(select) || select.Equals("1", StringComparison.Ordinal))
            {

            }
            else if (select.Equals("2", StringComparison.Ordinal))
                LogFactory.AddConsole();
            else
                goto Top2;


            Top3:

            Console.WriteLine("CMD：\r\n connect [IP]\t(Connect to the computer )\r\n    Example : connect 192.168.0.1 \r\n\r\n exit\t\t(exit the program)");


            select = Console.ReadLine();

            if (select is null || select.Equals("exit", StringComparison.Ordinal))
                goto Exit;
            else if (select.ToLower().IndexOf("connect") == 0)
            {
                string[] sp = select.Split(new char[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (sp.Length == 2)
                {
                    FileClient fileClient = new FileClient();

                    if (fileClient.Connect(sp[1]))
                    {                        
                        Console.Clear();
                        Console.Clear();
                        Console.WriteLine($"Connect {sp[1]} OK");
                        fileClient.client.Sync.Get<IServer>().LogOn();
                        fileClient.PrintCmd();

                        while (true)
                        {
                            string cmd = Console.ReadLine().Trim();

                            if (fileClient.Cmd(cmd))
                            {
                                Console.Clear();
                                goto Top3;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"not connect IP:{sp[1]}");
                        goto Top3;
                    }
                }
                else
                    goto Top3;
            }
            else
                goto Top3;




            Exit:
            Console.WriteLine("Exit ...");
        }
    }
}
