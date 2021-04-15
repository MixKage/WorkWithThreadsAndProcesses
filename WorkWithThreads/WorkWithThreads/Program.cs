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
        private static bool _flagFile1;
        private static bool _flagFile2;
        private static bool _flagFile3;
        private static bool _triggerProcess;

        public static void Main()
        {
            var threadMain = Thread.CurrentThread;
            threadMain.Name = "T0 (Main)";

            var arrayIdPost = new List<ulong?>();
            var arrayObjectIdTexts = new List<ObjectIdText>();
            var arrayObjectIdPhotos = new List<ObjectIdPhoto>();
            var arrayObjectIdUrIs = new List<ObjectIdUri>();
#if ThreadAndProcess
            Console.WriteLine("P1 T0 - Start");
            Console.WriteLine("P1 T0 - download posts");
#endif
            var api = Authorize();
            Debug.WriteLine("T0 - download posts");
            GetInfo(api, arrayIdPost, arrayObjectIdTexts, arrayObjectIdPhotos, arrayObjectIdUrIs);

            ThreadsControl(arrayObjectIdPhotos, arrayObjectIdTexts, arrayObjectIdUrIs);
#if DEBUG
            Process.Start("C:\\Users\\user\\Desktop\\OS\\WorkWithThreadsProc2\\WorkWithThreadsProc2\\bin\\Debug\\netcoreapp3.1\\WorkWithThreadsProc2.exe");
#endif

#if !DEBUG
            Process.Start("C:\\Users\\user\\Desktop\\OS\\WorkWithThreadsProc2\\WorkWithThreadsProc2\\bin\\Release\\netcoreapp3.1\\WorkWithThreadsProc2.exe");
#endif
            while (true)
            {
                Thread.Sleep(10000);
#if ThreadAndProcess
                Console.WriteLine("P1 T0 - download posts");
#endif
                Debug.WriteLine("T0 - download posts");
                GetInfo(api, arrayIdPost, arrayObjectIdTexts, arrayObjectIdPhotos, arrayObjectIdUrIs);
            }
        }

        private static void WaitingThread(object arrInput)
        {
            while (true)
            {
                if (_triggerProcess) continue;
                Thread.Sleep(301);
                if (Thread.CurrentThread.Name == "T1 (Text)")
                    SetData1InTextFile(arrInput);
                else if (Thread.CurrentThread.Name == "T2 (Photo)")
                    SetData2InTextFile(arrInput);
                else if (Thread.CurrentThread.Name == "T3 (Uri)")
                    SetData3InTextFile(arrInput);
            }
        }

        private static void ThreadsControl(List<ObjectIdPhoto> arrayObjectIdPhotos,
            List<ObjectIdText> arrayObjectIdTexts, List<ObjectIdUri> arrayObjectIdUrIs)
        {
            Thread threadT1 = new Thread(SetData1InTextFile)
            {
                Name = "T1 (Text)"
            };
            var threadT2 = new Thread(SetData2InTextFile)
            {
                Name = "T2 (Photo)"
            };
            var threadT3 = new Thread(SetData3InTextFile)
            {
                Name = "T3 (Uri)"
            };
            var threadT4 = new Thread(ReadDataStaticFile)
            {
                Name = "T4 (Read)"
            };
            threadT1.Start(arrayObjectIdTexts);
            threadT2.Start(arrayObjectIdPhotos);
            threadT3.Start(arrayObjectIdUrIs);
            threadT4.Start();
#if ThreadAndProcess
            Console.WriteLine("P1 T1 - Start");
            Console.WriteLine("P1 T2 - Start");
            Console.WriteLine("P1 T3 - Start");
            Console.WriteLine("P1 T4 - Start");
#endif
        }

        private static void ReadDataStaticFile()
        {
            while (true)
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("Trigger.txt", sizeof(bool)))
                {
                    using var acces = mmf.CreateViewAccessor(0, sizeof(bool));
                    
                    while (!acces.CanRead)
                    {

                    }
                    _triggerProcess = acces.ReadBoolean(0);

                    if (!_triggerProcess)
                    {
                        ReadDataFirstFile();
                        ReadDataSecondFile();
                        ReadDataEndFile();
                        _flagFile1 = false;
                        _flagFile2 = false;
                        _flagFile3 = false;
                        Thread.Sleep(100);
                        while (!acces.CanWrite)
                        {

                        }
                        acces.Write(0, true);
                    }
                }
            }

        }

        private static void ReadDataFirstFile()
        {
            while (!_flagFile1)
            {

            }
#if ThreadAndProcess
            Console.WriteLine("P1 T4 - reading T1 File");
#endif
            Debug.WriteLine("T4 - reading T1 File");
            string tempData;
           
            using (var streamReader = new StreamReader("text1.json"))
            {
                tempData = streamReader.ReadToEnd();
            }

            if (tempData.Contains("TextPost"))
            {
                var restoredPosts = JsonConvert.DeserializeObject<List<ObjectIdText>>(tempData);
#if !ThreadAndProcess
                foreach (var restoredPost in restoredPosts)
                {
                    Console.WriteLine("ID Поста: " + restoredPost.IdPost + " Текст поста:" +
                                      restoredPost.TextPost);
                }
#endif
#if ThreadAndProcess
                Console.WriteLine("P1 T4 - END reading T1 File");
#endif
                Debug.WriteLine("T4 - END Read T1 File");
            }
            _flagFile1 = false;
        }

        private static void ReadDataSecondFile()
        {

            while (!_flagFile2)
            {

            }
#if ThreadAndProcess
            Console.WriteLine("P1 T4 - reading T2 File");
#endif
            Debug.WriteLine("T4 - reading T2 File");
            string tempData;
            using (var streamReader = new StreamReader("text2.json"))
            {
                tempData = streamReader.ReadToEnd();
            }
            if (tempData.Contains("PhotoPost"))
            {
                var restoredPosts = JsonConvert.DeserializeObject<List<ObjectIdPhoto>>(tempData);
#if !ThreadAndProcess
                foreach (var restoredPost in restoredPosts)
                {
                    Console.WriteLine("ID Поста: " + restoredPost.IdPost + " Фото поста:" +
                                      restoredPost.PhotoPost);
                }
#endif
#if ThreadAndProcess
                Console.WriteLine("P1 T4 - END Read T2 Fil");
#endif
                Debug.WriteLine("T4 - END Read T2 File");
            }
            _flagFile2 = false;
        }

        private static void ReadDataEndFile()
        {
            while (!_flagFile3)
            {

            }
#if ThreadAndProcess
            Console.WriteLine("P1 T4 - reading T3 File");
#endif
            string tempData;
            using (var streamReader = new StreamReader("text3.json"))
            {
                tempData = streamReader.ReadToEnd();
            }
            if (tempData.Contains("UriPost"))
            {
                var restoredPosts = JsonConvert.DeserializeObject<List<ObjectIdUri>>(tempData);
#if !ThreadAndProcess
                foreach (var restoredPost in restoredPosts)
                {
                    Console.WriteLine("ID Поста: " + restoredPost.IdPost + " Ссылка поста:" +
                                      restoredPost.UriPost);
                }
#endif
#if ThreadAndProcess
                Console.WriteLine("P1 T4 - END reading T3 File");
#endif
                Debug.WriteLine("T4 - END Read T3 File");
            }
            _flagFile3 = false;
        }

        private static void SetData1InTextFile(object arrInput)
        {
            while (_flagFile1)
            {

            }

            var arr = (List<ObjectIdText>)arrInput;
            var jsonString = JsonConvert.SerializeObject(arr);
#if ThreadAndProcess
            Console.WriteLine("P1 T1 - writing T1 File");
#endif
            Debug.WriteLine("T1 - writing T1 File");
            using (var streamWriter = new StreamWriter("text1.json"))
            {
                streamWriter.WriteLine(jsonString);
            }
            _flagFile1 = true;
#if ThreadAndProcess
            Console.WriteLine("P1 T1 - END writing T1 File");
#endif
            Debug.WriteLine("T1 - END writing T1 File");
            WaitingThread(arrInput);
        }

        private static void SetData2InTextFile(object arrInput)
        {
            while (_flagFile2)
            {

            }
            var arr = (List<ObjectIdPhoto>)arrInput;
            var jsonString = JsonConvert.SerializeObject(arr);
#if ThreadAndProcess
            Console.WriteLine("P1 T2 - writing T2 File");
#endif
            Debug.WriteLine("T2 - writing T2 File");
            using (var streamWriter = new StreamWriter("text2.json"))
            {
                streamWriter.WriteLine(jsonString);
            }
            _flagFile2 = true;
#if ThreadAndProcess
            Console.WriteLine("P1 T2 - END writing T2 File");
#endif
            Debug.WriteLine("T2 - END writing T2 File");
            WaitingThread(arrInput);
        }

        private static void SetData3InTextFile(object arrInput)
        {
            while (_flagFile3)
            {

            }

            var arr = (List<ObjectIdUri>)arrInput;
            var jsonString = JsonConvert.SerializeObject(arr);
#if ThreadAndProcess
            Console.WriteLine("P1 T3 - writing T3 File");
#endif
            Debug.WriteLine("T3 - writing T3 File");
            using (var streamWriter = new StreamWriter("text3.json"))
            {
                streamWriter.WriteLine(jsonString);
            }
            _flagFile3 = true;
#if ThreadAndProcess
            Console.WriteLine("P1 T3 - END writing T3 File");
#endif
            Debug.WriteLine("T3 - END writing T3 File");
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
                Debug.WriteLine("!!wi-fi disconnected or wrong token!!");
                Environment.Exit(0);
            }

            foreach (var item in res.Items)
            {
                var isContentId = false;
                foreach (var item2 in res.Items)
                {
                    if (arrayIdPost.Contains(item2.PostId))
                    {
                        isContentId = true;
                    }
                }
                if (isContentId)
                    continue;
                if (item.PostId != null)
                {
                    if (item.Text != "")
                    {
                        arrayObjectIdTexts.Add(new ObjectIdText { IdPost = item.PostId, TextPost = item.Text });
                    }

                    if (item.Attachments != null)
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
                                { IdPost = item.PostId, UriPost = ((Link)attachment.Instance).Uri.ToString() });
                        }
                }
            }
        }
    }
}