#define ThreadAndProcess
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection.PortableExecutable;
using System.Text;
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
#if DEBUG
        private static string Path1 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Debug\netcoreapp3.1\text1.json";
        private static string Path2 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Debug\netcoreapp3.1\text2.json";
        private static string Path3 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Debug\netcoreapp3.1\text3.json";
#endif
#if !DEBUG
        private static string Path1 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Release\netcoreapp3.1\text1.json";
        private static string Path2 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Release\netcoreapp3.1\text2.json";
        private static string Path3 = @"C:\Users\user\Desktop\OS\WorkWithThreads\WorkWithThreads\bin\Release\netcoreapp3.1\text3.json";
#endif
        static void ControllerThread()
        {
            Thread T1 = new Thread(WaitingThread)
            {
                Name = "T1"
            };
            Thread T2 = new Thread(WaitingThread)
            {
                Name = "T2"
            };
            Thread T3 = new Thread(WaitingThread)
            {
                Name = "T3"
            };
            T1.Start();
            T2.Start();
            T3.Start();
#if ThreadAndProcess
            Console.WriteLine("P2 T1 - Created");
            Console.WriteLine("P2 T2 - Created");
            Console.WriteLine("P2 T3 - Created");
#endif
        }

        static void Queue()
        {
            while (true)
            {
                if (_endReadFiles == 3)
                {
                    _resume = true;
                    _start = false;
                    return;
                }
            }
        }

        static void WaitingThread()
        {
            while (true)
            {
                if (_start)
                {
                    _endReadFiles = 0;
                    if (Thread.CurrentThread.Name == "T1")
                    {
#if ThreadAndProcess
                        Console.WriteLine("P2 T1 - Read File 1");
#endif
                        Debug.WriteLine("T1");
                        ReadFirstFile();
                        _endReadFiles++;
                        Queue();
                    }
                    else if (Thread.CurrentThread.Name == "T2")
                    {
#if ThreadAndProcess
                        Console.WriteLine("P2 T2 - Read File 2");
#endif
                        Debug.WriteLine("T2");
                        ReadSecondFile();
                        _endReadFiles++;
                        Queue();
                    }
                    else if (Thread.CurrentThread.Name == "T3")
                    {
#if ThreadAndProcess
                        Console.WriteLine("P2 T3 - Read File 3");
#endif
                        Debug.WriteLine("T3");
                        ReadEndFile();
                        _endReadFiles++;
                        Queue();
                    }
                }
            }
        }



        static void ReadFirstFile()
        {
            string tempData;

            using (var streamReader = new StreamReader(Path1))
            {
                tempData = streamReader.ReadToEnd();
                streamReader.Close();
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

        static void ReadSecondFile()
        {
            string tempData;

            using (var streamReader = new StreamReader(Path2))
            {
                tempData = streamReader.ReadToEnd();
                streamReader.Close();
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

        static void ReadEndFile()
        {
            string tempData;

            using (var streamReader = new StreamReader(Path3))
            {
                tempData = streamReader.ReadToEnd();
                streamReader.Close();
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

        static void StartWork()
        {
            while (true)
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("Trigger.txt", sizeof(bool)))
                {
                    using var trigger = mmf.CreateViewAccessor(0, sizeof(bool));
                    trigger.Read(0, out _start);

                    if (_start)
                    {
#if ThreadAndProcess
                        Console.WriteLine("P2 T0 - Check access");
#endif
                        Debug.WriteLine("T0");
                        Thread.Sleep(50);
                        _start = false;
                    }

                    if (_resume)
                    {
                        trigger.Write(0, _start);
                        _resume = false;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            ControllerThread();
#if ThreadAndProcess
            Console.WriteLine("P2 T0 - Start");
#endif
            while (true)
            {
                StartWork();
            }
        }
    }
}
