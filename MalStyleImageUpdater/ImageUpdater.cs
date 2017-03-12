using MalStyleImageUpdater.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MalStyleImageUpdater
{
    class ImageUpdater
    {
        //Anime
        ConsoleColor animeColor = ConsoleColor.Blue;
        static int animeAdded = 0;
        static int animeUpdated = 0;
        static SortedDictionary<string, MangaAnimeItem> animeLocal = new SortedDictionary<string, MangaAnimeItem>();
        static SortedDictionary<string, MangaAnimeItem> animeRemote = new SortedDictionary<string, MangaAnimeItem>();

        //Manga
        ConsoleColor mangaColor = ConsoleColor.Green;
        static int mangaAdded = 0;
        static int mangaUpdated = 0;
        static SortedDictionary<string, MangaAnimeItem> mangaLocal = new SortedDictionary<string, MangaAnimeItem>();
        static SortedDictionary<string, MangaAnimeItem> mangaRemote = new SortedDictionary<string, MangaAnimeItem>();

        public ImageUpdater()
        {
            //get anime local file data
            Console.ForegroundColor = animeColor;
            getLocalAnimeData();
            Console.WriteLine("");

            //get manga local file data
            Console.ForegroundColor = mangaColor;
            getLocalMangaData();
            Console.WriteLine("");

            //get anime remote data
            Console.ForegroundColor = animeColor;
            getRemoteAnimeData();
            Console.WriteLine("");

            //get manga remote data
            Console.ForegroundColor = mangaColor;
            getRemoteMangaData();
            Console.WriteLine("");

            //statistic count
            Console.ForegroundColor = animeColor;
            Console.WriteLine("Anime remote count: " + animeRemote.Count());
            Console.WriteLine("Anime local count: " + animeLocal.Count());
            Console.WriteLine("");
            Console.ForegroundColor = mangaColor;
            Console.WriteLine("Manga remote count: " + mangaRemote.Count());
            Console.WriteLine("Mnime local count: " + mangaLocal.Count());

            //update anime
            Console.ForegroundColor = animeColor;
            updateAnimeDataInLocal();
            Console.WriteLine("");
            saveAnimeToCss();
            Console.WriteLine("");

            //update manga
            Console.ForegroundColor = mangaColor;
            updateMangaDataInLocal();
            Console.WriteLine("");
            saveMangaToCss();
            Console.WriteLine("");

            //statistic added/removed
            Console.ForegroundColor = animeColor;
            Console.WriteLine("Anime added : {0}", animeAdded);
            Console.WriteLine("Anime updated : {0}", animeUpdated);
            Console.WriteLine("");
            Console.ForegroundColor = mangaColor;
            Console.WriteLine("Manga added : {0}", mangaAdded);
            Console.WriteLine("Manga updated : {0}", mangaUpdated);

            Thread.Sleep(7 * 1000);
        }

        private SortedDictionary<string, MangaAnimeItem> parseCssFile(StreamReader reader)
        {
            SortedDictionary<string, MangaAnimeItem> list = new SortedDictionary<string, MangaAnimeItem>();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                String Id = "";
                String Url = "";
                String Name = "";
                Regex regexId = new Regex(@"(#more[0-9]*)");
                Regex regexUrl = new Regex(@"(url(\S)*[^);} ])");
                Regex regexName = new Regex(@"(\/\*.*\*\/)");
                Match matchId = regexId.Match(line);
                Match matchUrl = regexUrl.Match(line);
                Match matchName = regexName.Match(line);
                if (matchId.Success)
                {
                    Id = matchId.Groups[1].Value.Substring(5);
                }
                else
                {
                    continue;
                }
                if (matchUrl.Success)
                {
                    Url = matchUrl.Groups[1].Value.Substring(4);
                }
                if (matchName.Success)
                {
                    Name = matchName.Groups[1].Value.Substring(3).TrimEnd('/').TrimEnd('*').TrimEnd(' ');
                }
                //Console.WriteLine("{0} - {1}: {2}", Id, Name, Url);

                if (!list.ContainsKey(Id.ToString()))
                    list.Add(Id.ToString(), new MangaAnimeItem(Int32.Parse(Id), Name, Url));
                else
                    list[Id.ToString()] = new MangaAnimeItem(Int32.Parse(Id), Name, Url);
            }
            return list;
        }

        private void getLocalAnimeData()
        {
            Console.WriteLine("Load anime local data.");
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (StreamReader reader = new StreamReader(Settings.Instance.getAnimeFilePath()))
                {
                    animeLocal = parseCssFile(reader);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine("test " + ioex.ToString());
            }
            sw.Stop();
            Console.WriteLine("Anime local data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void getRemoteAnimeData()
        {
            Console.WriteLine("Reading anime remote data.");
            Stopwatch sw = Stopwatch.StartNew();
            using (var client = new HttpClient())
            {
                var responseString = client.GetStringAsync("http://myanimelist.net/malappinfo.php?u=" + Settings.Instance.getUsername() + "&status=all&type=anime");

                XmlDocument xmlData = new XmlDocument();
                xmlData.LoadXml(responseString.Result);

                XmlNodeList xnList = xmlData.SelectNodes("/myanimelist/anime");

                foreach (XmlNode xn in xnList)
                {
                    string title = xn["series_title"].InnerText;
                    string image = xn["series_image"].InnerText;
                    string id = xn["series_animedb_id"].InnerText;
                    animeRemote.Add(id.ToString(), new MangaAnimeItem(Int32.Parse(id), title, image));
                }
            }
            sw.Stop();
            Console.WriteLine("Anime remote data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void getLocalMangaData()
        {
            Console.WriteLine("Load manga local data.");
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (StreamReader reader = new StreamReader(Settings.Instance.getMangaFilePath()))
                {
                    mangaLocal = parseCssFile(reader);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine("test " + ioex.ToString());
            }
            sw.Stop();
            Console.WriteLine("Manga local data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void getRemoteMangaData()
        {
            Console.WriteLine("Reading manga remote data.");
            Stopwatch sw = Stopwatch.StartNew();
            using (var client = new HttpClient())
            {
                var responseString = client.GetStringAsync("http://myanimelist.net/malappinfo.php?u=" + Settings.Instance.getUsername() + "&status=all&type=manga");

                XmlDocument xmlData = new XmlDocument();
                xmlData.LoadXml(responseString.Result);

                XmlNodeList xnList = xmlData.SelectNodes("/myanimelist/manga");

                foreach (XmlNode xn in xnList)
                {
                    string title = xn["series_title"].InnerText;
                    string image = xn["series_image"].InnerText;
                    string id = xn["series_mangadb_id"].InnerText;
                    mangaRemote.Add(id.ToString(), new MangaAnimeItem(Int32.Parse(id), title, image));
                }
            }
            sw.Stop();
            Console.WriteLine("Manga remote data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void updateAnimeDataInLocal()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Console.WriteLine("Updating anime...");
            foreach (MangaAnimeItem item in animeRemote.Values)
            {
                MangaAnimeItem localItem;
                if (animeLocal.ContainsKey(item.id.ToString()))
                {
                    localItem = animeLocal[item.id.ToString()];
                    if (!localItem.EqualsImageUrl(item))
                    {
                        Console.WriteLine("Local and remote image is not same {0} - {1}", item.id.ToString(), item.name);
                        Console.WriteLine("Old : {0}", localItem.url);
                        Console.WriteLine("New : {0}", item.url);
                        localItem = item;
                        animeLocal[item.id.ToString()] = localItem;
                        animeUpdated++;
                    }
                    else
                    {
                        if (localItem.name.Length <= 0)
                        {
                            Console.WriteLine("Fixing title for {0}", localItem.id.ToString());
                            localItem.name = item.name;
                            animeLocal[item.id.ToString()] = localItem;
                        }
                        Console.WriteLine("Local and remote image is same {0} - {1} : {2}", localItem.id.ToString(), localItem.name, localItem.url);
                    }
                }
                else
                {
                    Console.WriteLine("Local is not exist. Adding");
                    Console.WriteLine("Id : {0}", item.id.ToString());
                    Console.WriteLine("Name : {0}", item.name);
                    Console.WriteLine("ImageUrl : {0}", item.url);
                    localItem = item;
                    animeLocal.Add(localItem.id.ToString(), localItem);
                    animeAdded++;
                }
                Console.WriteLine("________________________________________\n");
            }
            sw.Stop();
            Console.WriteLine("Anime local data updated: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void updateMangaDataInLocal()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Console.WriteLine("Updating manga...");
            foreach (MangaAnimeItem item in mangaRemote.Values)
            {
                MangaAnimeItem localItem;
                if (mangaLocal.ContainsKey(item.id.ToString()))
                {
                    localItem = mangaLocal[item.id.ToString()];
                    if (!localItem.EqualsImageUrl(item))
                    {
                        Console.WriteLine("Local and remote image is not same {0} - {1}", item.id.ToString(), item.name);
                        Console.WriteLine("Old : {0}", localItem.url);
                        Console.WriteLine("New : {0}", item.url);
                        localItem = item;
                        mangaLocal[item.id.ToString()] = localItem;
                        mangaUpdated++;
                    }
                    else
                    {
                        if (localItem.name.Length <= 0)
                        {
                            Console.WriteLine("Fixing title for {0}", localItem.id.ToString());
                            localItem.name = item.name;
                            mangaLocal[item.id.ToString()] = localItem;
                        }
                        Console.WriteLine("Local and remote image is same {0} - {1} : {2}", localItem.id.ToString(), localItem.name, localItem.url);
                    }
                }
                else
                {
                    Console.WriteLine("Local is not exist. Adding");
                    Console.WriteLine("Id : {0}", item.id.ToString());
                    Console.WriteLine("Name : {0}", item.name);
                    Console.WriteLine("ImageUrl : {0}", item.url);
                    localItem = item;
                    mangaLocal.Add(localItem.id.ToString(), localItem);
                    mangaAdded++;
                }
                Console.WriteLine("________________________________________\n");
            }
            sw.Stop();
            Console.WriteLine("Manga local data updated: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void saveAnimeToCss()
        {
            Console.WriteLine("Write new anime css file.");
            Stopwatch sw = Stopwatch.StartNew();

            string css = "";

            foreach (MangaAnimeItem item in animeLocal.Values)
            {
                css += "#more" + item.id.ToString() + " {background-image: url(" + item.url + ");} /* " + item.name + " */\r\n";
            }

            using (StreamWriter writetext = new StreamWriter(Settings.Instance.getAnimeFilePath()))
            {
                writetext.Write(css);
            }

            if(Settings.Instance.getMinFileActivate())
            {
                string cssMin = Utils.RemoveWhiteSpaceFromStyles(css);

                using (StreamWriter writetext = new StreamWriter(Settings.Instance.getAnimeMinFilePath()))
                {
                    writetext.Write(cssMin);
                }
            }

            sw.Stop();
            Console.WriteLine("Write to anime file is done {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void saveMangaToCss()
        {
            Console.WriteLine("Write new manga css file.");
            Stopwatch sw = Stopwatch.StartNew();

            string css = "";

            foreach (MangaAnimeItem item in mangaLocal.Values)
            {
                css += "#more" + item.id.ToString() + " {background-image: url(" + item.url + ");} /* " + item.name + " */\r\n";
            }

            using (StreamWriter writetext = new StreamWriter(Settings.Instance.getMangaFilePath()))
            {
                writetext.Write(css);
            }

            if (Settings.Instance.getMinFileActivate())
            {
                string cssMin = Utils.RemoveWhiteSpaceFromStyles(css);

                using (StreamWriter writetext = new StreamWriter(Settings.Instance.getMangaMinFilePath()))
                {
                    writetext.Write(cssMin);
                }
            }

            sw.Stop();
            Console.WriteLine("Write to manga file is done {0}ms.", sw.Elapsed.TotalMilliseconds);
        }


    }
}
