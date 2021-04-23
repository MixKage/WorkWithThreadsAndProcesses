#define ThreadAndProcess

using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Newtonsoft.Json;
using WorkWithThreads.Objects;

namespace Deamon
{
    public partial class Service1 : ServiceBase
    {
        private Thread _main;

        public Service1()
        {
            InitializeComponent();
        }

        private static bool _processAccess;
#if DEBUG
        private const string Path1 = @"C:\Users\user\Desktop\OS\WorkWithThreads\ProccesOne\bin\Debug\text1.json";
        private const string Path2 = @"C:\Users\user\Desktop\OS\WorkWithThreads\ProccesOne\bin\Debug\text2.json";
        private const string Path3 = @"C:\Users\user\Desktop\OS\WorkWithThreads\ProccesOne\bin\Debug\text3.json";
#endif
#if !DEBUG
        private const string Path1 = @"C:\Users\user\Desktop\OS\WorkWithThreads\ProccesOne\bin\Release\text2.json";
        private const string Path2 = @"C:\Users\user\Desktop\OS\WorkWithThreads\ProccesOne\bin\Release\text2.json";
        private const string Path3 = @"C:\Users\user\Desktop\OS\WorkWithThreads\ProccesOne\bin\Release\text2.json";
#endif
        private static void ReadFirstFile()
        {
            string tempData;

            using (var streamReader = new StreamReader(Path1))
            {
                tempData = streamReader.ReadToEnd();
            }

            var restoredPosts = JsonConvert.DeserializeObject<List<ObjectIdText>>(tempData);
#if !ThreadAndProcess
                foreach (var restoredPost in restoredPosts)
                {
                    Console.WriteLine(Thread.CurrentThread.Name + " ID Поста: " + restoredPost.IdPost +
                                      " Текст поста: " +
                                      restoredPost.TextPost);
                }
#endif
        }

        private static void ReadSecondFile()
        {
            string tempData;

            using (var streamReader = new StreamReader(Path2))
            {
                tempData = streamReader.ReadToEnd();
            }

            var restoredPosts = JsonConvert.DeserializeObject<List<ObjectIdPhoto>>(tempData);
#if !ThreadAndProcess
                foreach (var restoredPost in restoredPosts)
                {
                    Console.WriteLine(Thread.CurrentThread.Name + " ID Поста: " + restoredPost.IdPost +
                                      " Фото поста: " +
                                      restoredPost.PhotoPost);
                }
#endif
        }

        private static void ReadEndFile()
        {
            string tempData;

            using (var streamReader = new StreamReader(Path3))
            {
                tempData = streamReader.ReadToEnd();
            }

            var restoredPosts = JsonConvert.DeserializeObject<List<ObjectIdUri>>(tempData);

#if !ThreadAndProcess
                foreach (var restoredPost in restoredPosts)
                {
                    Console.WriteLine(Thread.CurrentThread.Name + " ID Поста: " + restoredPost.IdPost +
                                      " Фото поста: " +
                                      restoredPost.UriPost);
                }
#endif
        }

        private static void WorkProcess2()
        {
            var threads = new List<Thread> { new Thread(ReadFirstFile), new Thread(ReadSecondFile), new Thread(ReadEndFile) };
            for (int i = 0; i < 3; i++)
            {
                threads[i].Start();
            }
            while (threads[0].IsAlive || threads[1].IsAlive || threads[2].IsAlive) { }
        }



        protected override void OnStart(string[] args)
        {

            _main = new Thread(Process2Work);
            _main.Start();
        }

        private static void Process2Work()
        {
            var security = new MemoryMappedFileSecurity();
            security.AddAccessRule(new AccessRule<MemoryMappedFileRights>(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null), MemoryMappedFileRights.FullControl,
                AccessControlType.Allow));
            using var mmf = MemoryMappedFile.CreateOrOpen(@"Global\trigger", sizeof(bool),
                MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, security,
                HandleInheritability.Inheritable);
            using var access = mmf.CreateViewAccessor(0, sizeof(bool));

            while (true)
            {
                while (!access.CanRead) { }
                access.Read(0, out _processAccess);

                if (_processAccess)
                {
                    WorkProcess2();

                    while (!access.CanWrite) { }
                    access.Write(0, false);
                }
            }
        }


        protected override void OnStop()
        {
            _main.Abort();
        }
    }
}