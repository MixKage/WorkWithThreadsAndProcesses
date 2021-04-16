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
        private static int _endReadFiles;
        private static bool _resume;
        private static bool _start;
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
        private static void ControllerThread()
        {
            var t1 = new Thread(WaitingThread)
            {
                Name = "T1"
            };
            var t2 = new Thread(WaitingThread)
            {
                Name = "T2"
            };
            var t3 = new Thread(WaitingThread)
            {
                Name = "T3"
            };
            t1.Start();
            t2.Start();
            t3.Start();

            Console.WriteLine("P2 T1 - Created");
            Console.WriteLine("P2 T2 - Created");
            Console.WriteLine("P2 T3 - Created");
        }

        private static void Queue()
        {
            while (true)
            {
                if (_endReadFiles == 3)
                {
                    _start = false;
                    _resume = true;
                    return;
                }
            }
        }

        private static void WaitingThread()
        {
            while (true)
            {
                if (_start)
                {
                    _endReadFiles = 0;
                    switch (Thread.CurrentThread.Name)
                    {
                        case "T1":
                            Console.WriteLine("P2 T1 - Read File 1");
                            ReadFirstFile();
                            Console.WriteLine("P2 T1 - END read File 1");
                            _endReadFiles++;
                            Queue();
                            break;
                        case "T2":
                            Console.WriteLine("P2 T2 - Read File 2");
                            ReadSecondFile();
                            Console.WriteLine("P2 T2 - END read File 2");
                            _endReadFiles++;
                            Queue();
                            break;
                        case "T3":
                            Console.WriteLine("P2 T3 - Read File 3");
                            ReadEndFile();
                            Console.WriteLine("P2 T3 - END read File 3");
                            _endReadFiles++;
                            Queue();
                            break;
                    }
                }
            }
        }


        private static void ReadFirstFile()
        {
            string tempData = default;
            bool allGood = false;
            while (!allGood)
            {
                try
                {
                    using (var streamReader = new StreamReader(Path1))
                    {
                        tempData = streamReader.ReadToEnd();
                    }

                    allGood = true;
                }
                catch
                {
                    Thread.Sleep(100);
                }
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
            string tempData = default;
            bool allGood = false;
            while (!allGood)
            {
                try
                {
                    using (var streamReader = new StreamReader(Path2))
                    {
                        tempData = streamReader.ReadToEnd();
                    }

                    allGood = true;
                }
                catch
                {
                    Thread.Sleep(100);
                }
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
            string tempData = default;
            bool allGood = false;
            while (!allGood)
            {
                try
                {
                    using (var streamReader = new StreamReader(Path2))
                    {
                        tempData = streamReader.ReadToEnd();
                    }

                    allGood = true;
                }
                catch
                {
                    Thread.Sleep(100);
                }
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

        private static void StartWork()
        {
            while (true)
            {
                using MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("Trigger", sizeof(bool));
                using var access = mmf.CreateViewAccessor(0, sizeof(bool));

                while (!access.CanRead) { }
                access.Read(0, out _processAccess);

                if (_processAccess)
                {
                    Console.WriteLine("P2 T0 - Give access");
                    _start = true;
                    _processAccess = false;
                    while (!_resume) { }

                    while (!access.CanWrite) { }
                    access.Write(0, _processAccess);
                    _resume = false;
                    Thread.Sleep(300);
                    break;

                }
            }
        }

        private static void Main()
        {
            Console.WriteLine("P2 T0 - Start");
            ControllerThread();
            while (true)
            {
                StartWork();
            }
        }
    }
}