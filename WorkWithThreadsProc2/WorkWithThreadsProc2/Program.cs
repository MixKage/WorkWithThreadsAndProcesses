#define ThreadAndProcess
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Newtonsoft.Json;
using WorkWithThreads.Objects;

namespace WorkWithThreadsProc2
{
    class Program
    {
        private static bool _processAccess;
#if DEBUG
        private const string Path1 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Debug\netcoreapp3.1\text1.json";
        private const string Path2 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Debug\netcoreapp3.1\text2.json";
        private const string Path3 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Debug\netcoreapp3.1\text3.json";
#endif
#if !DEBUG
        private const string Path1 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Release\netcoreapp3.1\text1.json";
        private const string Path2 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Release\netcoreapp3.1\text2.json";
        private const string Path3 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Release\netcoreapp3.1\text3.json";
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
            Console.WriteLine("P2 - T1, T2, T3 - START WORK");
            var threads = new List<Thread> { new Thread(ReadFirstFile), new Thread(ReadSecondFile), new Thread(ReadEndFile) };
            //ОБЪЕДИНИТЬ ФУНКЦИИ
            for (int i = 0; i < 3; i++)
            {
                threads[i].Start();
            }
            while (threads[0].IsAlive||threads[1].IsAlive||threads[2].IsAlive) { }
            Console.WriteLine("P2 - T1, T2, T3 - END WORK");
        }

        private static void Main()
        {
            Console.WriteLine("P2 - T0 - Create");
            using MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("Trigger", sizeof(bool));
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
    }
}