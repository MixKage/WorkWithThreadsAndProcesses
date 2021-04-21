#define ThreadAndProcess
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using WorkWithThreads.Objects;

namespace WorkWithThreads
{
    internal class Program
    {
        private static object _flagFile1 = new object();
        private static object _flagFile2 = new object();
        private static object _flagFile3 = new object();

        private static object locker = new object();

        private static bool _triggerProcess;

        public static void Main()
        {
            var arrayIdPost = new List<ulong?>();
            var arrayObjectIdTexts = new List<ObjectIdText>();
            var arrayObjectIdPhotos = new List<ObjectIdPhoto>();
            var arrayObjectIdUrIs = new List<ObjectIdUri>();

            Console.WriteLine("P1 T0 - Start");

            ThreadsControl(arrayObjectIdPhotos, arrayObjectIdTexts, arrayObjectIdUrIs);
            Console.WriteLine("P1 T0 - download posts");
            var api = Authorize();
            GetInfo(api, arrayIdPost, arrayObjectIdTexts, arrayObjectIdPhotos, arrayObjectIdUrIs);
            ControllProccess1(arrayObjectIdTexts, arrayObjectIdPhotos, arrayObjectIdUrIs);
#if DEBUG
            //Process.Start("C:\\Users\\user\\Desktop\\OS\\WorkWithThreadsProc2\\WorkWithThreadsProc2\\bin\\Debug\\netcoreapp3.1\\WorkWithThreadsProc2.exe");
#endif
#if !DEBUG
            //Process.Start("C:\\Users\\user\\Desktop\\OS\\WorkWithThreadsProc2\\WorkWithThreadsProc2\\bin\\Release\\netcoreapp3.1\\WorkWithThreadsProc2.exe");
#endif
            while (true)
            {
                Thread.Sleep(10000);
                Console.WriteLine("P1 T0 - download posts");
                GetInfo(api, arrayIdPost, arrayObjectIdTexts, arrayObjectIdPhotos, arrayObjectIdUrIs);
            }
        }

        private static void ControllProccess1(List<ObjectIdText> arrayObjectIdTexts, List<ObjectIdPhoto> arrayObjectIdPhotos, List<ObjectIdUri> arrayObjectIdUrIs)
        {
            //using MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("Trigger", sizeof(bool));
            //using var access = mmf.CreateViewAccessor(0, sizeof(bool));

            //while (!access.CanRead) { }

            //_triggerProcess = access.ReadBoolean(0);
            while (true)
            {
                for (var mode = 1; mode <= 3; mode++)
                {
                    var threads = StartThreadWork(mode, arrayObjectIdTexts, arrayObjectIdPhotos, arrayObjectIdUrIs);
                    while ((threads[0].IsAlive || threads[1].IsAlive || threads[2].IsAlive)) { }
                }
            }

            //while (!access.CanWrite) { }
            //access.Write(0, true);
        }

        private static List<Thread> StartThreadWork(object mode, List<ObjectIdText> arrayObjectIdTexts, List<ObjectIdPhoto> arrayObjectIdPhotos, List<ObjectIdUri> arrayObjectIdUrIs)
        {
            var threads = new List<Thread> { new Thread(SetDataInTextFile), new Thread(SetDataInTextFile), new Thread(ReadDataFile) };
            //for (int i = 0; i < 2; i++)
            //    threads[i] = new Thread(SetDataInTextFile);
            //threads[2] = new Thread(ReadDataFile);

            switch (mode)
            {
                case 1:

                    threads[0].Name = "T2 (Photo)";
                    threads[1].Name = "T3 (Uri)";
                    threads[2].Name = "T4 (Read)";

                    threads[0].Start(arrayObjectIdPhotos);
                    threads[1].Start(arrayObjectIdUrIs);
                    threads[2].Start(mode);
                    PrintInfo(1);
                    break;
                case 2:
                    threads[0].Name = "T1 (Text)";
                    threads[1].Name = "T3 (Uri)";
                    threads[2].Name = "T4 (Read)";

                    threads[0].Start(arrayObjectIdTexts);
                    threads[1].Start(arrayObjectIdUrIs);
                    threads[2].Start(mode);
                    PrintInfo(2);
                    break;
                default:
                    threads[0].Name = "T1 (Text)";
                    threads[1].Name = "T2 (Photo)";
                    threads[2].Name = "T4 (Read)";

                    threads[0].Start(arrayObjectIdTexts);
                    threads[1].Start(arrayObjectIdPhotos);
                    threads[2].Start(mode);
                    PrintInfo(3);
                    break;
            }

            return threads;
        }

        private static void PrintInfo(int mode)
        {
            switch (mode)
            {
                case 1:
                    Console.WriteLine("P1 T2 - Start");
                    Console.WriteLine("P1 T3 - Start");
                    Console.WriteLine("P1 T4 - Start");
                    break;
                case 2:
                    Console.WriteLine("P1 T1 - Start");
                    Console.WriteLine("P1 T3 - Start");
                    Console.WriteLine("P1 T4 - Start");
                    break;
                case 3:
                    Console.WriteLine("P1 T1 - Start");
                    Console.WriteLine("P1 T2 - Start");
                    Console.WriteLine("P1 T4 - Start");
                    break;
                default:
                    Console.WriteLine("Defaulte");
                    break;
            }
        }


        private static void WaitingThread(object arrInput)
        {
            while (true)
            {
                if (_triggerProcess) continue;
                return;
            }
        }

        private static void ThreadsControl(List<ObjectIdPhoto> arrayObjectIdPhotos,
            List<ObjectIdText> arrayObjectIdTexts, List<ObjectIdUri> arrayObjectIdUrIs)
        {
            var threadT1 = new Thread(FirstReadFile)
            {
                Name = "T1 (Text)"
            };
            var threadT2 = new Thread(FirstReadFile)
            {
                Name = "T2 (Photo)"
            };
            var threadT3 = new Thread(FirstReadFile)
            {
                Name = "T3 (Uri)"
            };
            threadT1.Start(arrayObjectIdTexts);
            threadT2.Start(arrayObjectIdPhotos);
            threadT3.Start(arrayObjectIdUrIs);
            Console.WriteLine("P1 T1 - Start");
            Console.WriteLine("P1 T2 - Start");
            Console.WriteLine("P1 T3 - Start");
        }

        private static void ReadDataStaticFile()
        {
            var iteratorLap = 0;




            for (var i = 1; i <= 3; i++)
                ReadDataFile(i);

            iteratorLap++;

            Console.WriteLine("The lap №" + iteratorLap);
            Thread.Sleep(200 + iteratorLap / 100);


        }


        private static void ReadDataFile(object mode)
        {
            List<ObjectIdText> restoredPostsIdText = default;
            List<ObjectIdPhoto> restoredPostsIdPhoto = default;
            List<ObjectIdUri> restoredPostsIdUri = default;

            string tempData;
            string path;
            string content;

            switch (mode)
            {
                case 1:
                    Console.WriteLine("P1 T4 - reading T1 File");
                    path = "text1.json";
                    content = " Текст поста: ";
                    break;

                case 2:
                    Console.WriteLine("P1 T4 - reading T2 File");
                    path = "text2.json";
                    content = " Фото поста: ";
                    break;

                default:
                    Console.WriteLine("P1 T4 - reading T3 File");
                    path = "text3.json";
                    content = " Ссылка поста: ";
                    break;
            }
            using (var streamReader = new StreamReader(path))
            {
                tempData = streamReader.ReadToEnd();
            }

            if (tempData.Contains("TextPost"))
            {
                restoredPostsIdText = JsonConvert.DeserializeObject<List<ObjectIdText>>(tempData);
                Console.WriteLine("P1 T4 - END reading T1 File");
            }
            else if (tempData.Contains("PhotoPost"))
            {
                restoredPostsIdPhoto = JsonConvert.DeserializeObject<List<ObjectIdPhoto>>(tempData);
                Console.WriteLine("P1 T4 - END reading T2 File");
            }
            else if (tempData.Contains("UriPost"))
            {
                restoredPostsIdUri = JsonConvert.DeserializeObject<List<ObjectIdUri>>(tempData);
                Console.WriteLine("P1 T4 - END reading T3 File");
            }
#if !ThreadAndProcess
            switch (mode)
            {
                case 1:
                    if (restoredPostsIdText != null)
                        foreach (var restoredPost in restoredPostsIdText)
                        {
                            Console.WriteLine("ID Поста: " + restoredPost.IdPost + content +
                                              restoredPost.TextPost);
                        }

                    break;
                case 2:
                    if (restoredPostsIdPhoto != null)
                        foreach (var restoredPost in restoredPostsIdPhoto)
                        {
                            Console.WriteLine("ID Поста: " + restoredPost.IdPost + content +
                                              restoredPost.PhotoPost);
                        }

                    break;
                default:
                    if (restoredPostsIdUri != null)
                        foreach (var restoredPost in restoredPostsIdUri)
                        {
                            Console.WriteLine("ID Поста: " + restoredPost.IdPost + content +
                                              restoredPost.UriPost);
                        }

                    break;
            }
#endif
        }

        private static void FirstReadFile(object arrInput)
        {
            string path;
            string tempData;
            switch (Thread.CurrentThread.Name)
            {
                case "T1 (Text)":
                    path = "text1.json";
                    if (!File.Exists(path))
                    {
                        Console.WriteLine("The file \"text1.json\" does not exist.");
                        SetDataInTextFile(arrInput);
                    }

                    Console.WriteLine("P1 T1 - reading old T1 File");
                    break;
                case "T2 (Photo)":
                    path = "text2.json";
                    if (!File.Exists(path))
                    {
                        Console.WriteLine("The file \"text2.json\" does not exist.");
                        SetDataInTextFile(arrInput);
                    }
                    Console.WriteLine("P1 T2 - reading old T2 File");
                    break;
                default:
                    var arr3 = (List<ObjectIdUri>)arrInput;
                    path = "text3.json";
                    if (!File.Exists(path))
                    {
                        Console.WriteLine("The file \"text3.json\" does not exist.");
                        SetDataInTextFile(arrInput);
                    }
                    Console.WriteLine("P1 T3 - reading old T3 File");
                    break;
            }
            using (var streamReader = new StreamReader(path))
            {
                tempData = streamReader.ReadToEnd();
            }

            if (tempData.Contains("TextPost"))
            {
                arrInput = JsonConvert.DeserializeObject<List<ObjectIdText>>(tempData);
                Console.WriteLine("P1 T4 - END reading T1 File");
                _flagFile1 = false;
            }
            else if (tempData.Contains("PhotoPost"))
            {
                arrInput = JsonConvert.DeserializeObject<List<ObjectIdPhoto>>(tempData);
                Console.WriteLine("P1 T4 - END reading T2 File");
                _flagFile2 = false;
            }
            else if (tempData.Contains("UriPost"))
            {
                arrInput = JsonConvert.DeserializeObject<List<ObjectIdUri>>(tempData);
                Console.WriteLine("P1 T4 - END reading T3 File");
                _flagFile3 = false;
            }

            SetDataInTextFile(arrInput);
        }

        private static void SetDataInTextFile(object arrInput)
        {
            string jsonString;
            string path;
            switch (Thread.CurrentThread.Name)
            {
                case "T1 (Text)":
                    var arr1 = (List<ObjectIdText>)arrInput;
                    jsonString = JsonConvert.SerializeObject(arr1);
                    path = "text1.json";
                    Console.WriteLine("P1 T1 - writing T1 File");
                    break;
                case "T2 (Photo)":
                    var arr2 = (List<ObjectIdPhoto>)arrInput;
                    jsonString = JsonConvert.SerializeObject(arr2);
                    path = "text2.json";
                    Console.WriteLine("P1 T2 - writing T2 File");
                    break;
                default:
                    var arr3 = (List<ObjectIdUri>)arrInput;
                    jsonString = JsonConvert.SerializeObject(arr3);
                    path = "text3.json";
                    Console.WriteLine("P1 T3 - writing T3 File");
                    break;
            }


            using (var streamWriter = new StreamWriter(path))
            {
                streamWriter.WriteLine(jsonString);
            }

            //using (var streamWriter = new StreamWriter(path))
            //{
            //    streamWriter.WriteLine(jsonString);
            //}

            switch (Thread.CurrentThread.Name)
            {
                case "T1 (Text)":
                    Console.WriteLine("P1 T1 - END writing T1 File");
                    break;
                case "T2 (Photo)":
                    Console.WriteLine("P1 T2 - END writing T2 File");
                    break;
                default:
                    Console.WriteLine("P1 T3 - END writing T3 File");
                    break;
            }

            WaitingThread(arrInput);
        }

        private static VkApi Authorize()
        {
            var api = new VkApi();

            api.Authorize(new ApiAuthParams
            {
                AccessToken = "d07d682a93f0ba39ed5ee4716020d670c4c979fc091621ca09b5138515b6194b005a8b42b67191e002efa"
            });
            return api;
        }

        private static void GetInfo(VkApi api, List<ulong?> arrayIdPost, List<ObjectIdText> arrayObjectIdTexts, List<ObjectIdPhoto> arrayObjectIdPhotos, List<ObjectIdUri> arrayObjectIdUrIs)
        {
            NewsFeed res = default;
            try
            {
                res = api.NewsFeed.Get(new NewsFeedGetParams
                { Filters = NewsTypes.Post, Count = 100 });
            }
            catch
            {
                Console.WriteLine("!!wi-fi disconnected or wrong token!!");
                Environment.Exit(0);
            }

            foreach (var item in res.Items)
            {
                if (item.PostId == null) continue;

                if (arrayIdPost.Contains(item.PostId)) continue;

                arrayIdPost.Add(item.PostId);

                if (item.Text != "") arrayObjectIdTexts.Add(new ObjectIdText { IdPost = item.PostId, TextPost = item.Text });

                if (item.Attachments == null) continue;

                foreach (var attachment in item?.Attachments)
                {
                    if (attachment.Type == typeof(Photo))
                        arrayObjectIdPhotos.Add(new ObjectIdPhoto
                        {
                            IdPost = item.PostId,
                            PhotoPost = ((Photo)attachment.Instance).Sizes.Last().Url.ToString()
                        });

                    if (attachment.Type == typeof(Link))
                        arrayObjectIdUrIs.Add(new ObjectIdUri
                        {
                            IdPost = item.PostId,
                            UriPost = ((Link)attachment.Instance).Uri.ToString()
                        });
                }
            }
        }
    }
}