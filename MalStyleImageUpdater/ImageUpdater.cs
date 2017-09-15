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
    using System.Xml.Serialization;
    using MangaAnimeItems = SortedDictionary<int, MangaAnimeItem>;

    public class ImageUpdater
    {
        //Anime
        static int animeAdded = 0;
        static int animeUpdated = 0;
        static MangaAnimeItems animeLocal = new MangaAnimeItems();
        static MangaAnimeItems animeRemote = new MangaAnimeItems();

        //Manga
        static int mangaAdded = 0;
        static int mangaUpdated = 0;
        static MangaAnimeItems mangaLocal = new MangaAnimeItems();
        static MangaAnimeItems mangaRemote = new MangaAnimeItems();

        public ImageUpdater()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            Task anime = new Task(() =>
            {
                Task local = new Task(getLocalAnimeData);
                Task remote = new Task(getRemoteAnimeData);
                local.Start();
                remote.Start();
                local.Wait();
                remote.Wait();

                //update anime
                updateAnimeDataInLocal();
                saveAnimeToCss();
            });
            Task manga = new Task(() =>
            {
                Task local = new Task(getLocalMangaData);
                Task remote = new Task(getRemoteMangaData);
                local.Start();
                remote.Start();
                local.Wait();
                remote.Wait();

                //update manga
                updateMangaDataInLocal();
                saveMangaToCss();
            });

            anime.Start();
            manga.Start();
            anime.Wait();
            manga.Wait();

            //statistic count
            //Console.WriteLine("Anime remote count: " + animeRemote.Count());
            //Console.WriteLine("Anime local count: " + animeLocal.Count());
            //Console.WriteLine("Manga remote count: " + mangaRemote.Count());
            //Console.WriteLine("Mnime local count: " + mangaLocal.Count());


            //statistic added/removed
            //Console.WriteLine("Anime added : {0}", animeAdded);
            //Console.WriteLine("Anime updated : {0}", animeUpdated);
            //Console.WriteLine("Manga added : {0}", mangaAdded);
            //Console.WriteLine("Manga updated : {0}", mangaUpdated);
            sw.Stop();
            Console.WriteLine("App work: {0}ms.", sw.Elapsed.TotalMilliseconds);
            animeLocal = animeRemote = mangaLocal = mangaRemote = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Thread.Sleep(7 * 1000);
        }

        private MangaAnimeItems parseCssFile(StreamReader reader)
        {
            MangaAnimeItems list = new MangaAnimeItems();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                Regex regexId = new Regex(@"#more([0-9]+)");
                Regex regexUrl = new Regex(@"url\((.*)\);");
                Regex regexName = new Regex(@"(\/\*.*\*\/)");
                Match matchId = regexId.Match(line);
                Match matchUrl = regexUrl.Match(line);
                Match matchName = regexName.Match(line);
                if (!matchId.Success || !matchUrl.Success || !matchName.Success)
                {
                    continue;
                }

                int id = Int32.Parse(matchId.Groups[1].Value);
                MangaAnimeItem item = null;
                if (list.ContainsKey(id))
                {
                    item = list[id];

                }
                else
                {
                    item = new MangaAnimeItem();
                    list.Add(id, item);
                }

                item.id = id;
                item.url = matchUrl.Groups[1].Value;
                item.name = matchName.Groups[1].Value.Substring(3).TrimEnd('/').TrimEnd('*').TrimEnd(' ');
            }
            return list;
        }

        private void getLocalAnimeData()
        {
            //Console.WriteLine("Load anime local data.");
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

        [Serializable()]
        public class XmlAnimeEntry
        {
            [XmlElement("series_title")]
            public string Title;
            [XmlElement("series_image")]
            public string Image;
            [XmlElement("series_animedb_id")]
            public int id;
        }

        [XmlRoot("myanimelist")]
        public class XmlAnime
        {
            [XmlElement("anime")]
            public XmlAnimeEntry[] animes;
        }
        private void getRemoteAnimeData()
        {
            //Console.WriteLine("Reading anime remote data.");
            Stopwatch sw = Stopwatch.StartNew();
            using (var client = new HttpClient())
            {
                var responseString = client.GetStringAsync("http://myanimelist.net/malappinfo.php?u=" + Settings.Instance.getUsername() + "&status=all&type=anime");

                /*XmlDocument xmlData = new XmlDocument();
                xmlData.LoadXml(responseString.Result);

                XmlNodeList xnList = xmlData.SelectNodes("/myanimelist/anime");

                foreach (XmlNode xn in xnList)
                {
                    string title = xn["series_title"].InnerText;
                    string image = xn["series_image"].InnerText;
                    int id = Int32.Parse(xn["series_animedb_id"].InnerText);
                    animeRemote.Add(id, new MangaAnimeItem(id, title, image));
                }*/

                XmlSerializer serializer = new XmlSerializer(typeof(XmlAnime));
                TextReader reader = new StringReader(responseString.Result);
                XmlAnime anime = (XmlAnime)serializer.Deserialize(reader);

                foreach (XmlAnimeEntry entry in anime.animes)
                {
                    animeRemote.Add(entry.id, new MangaAnimeItem(entry.id, entry.Title, entry.Image));
                }
            }
            sw.Stop();
            Console.WriteLine("Anime remote data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void getLocalMangaData()
        {
            //Console.WriteLine("Load manga local data.");
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

        [Serializable()]
        public class XmlMangaEntry
        {
            [XmlElement("series_title")]
            public string Title;
            [XmlElement("series_image")]
            public string Image;
            [XmlElement("series_mangadb_id")]
            public int id;
        }

        [XmlRoot("myanimelist")]
        public class XmlManga
        {
            [XmlElement("manga")]
            public XmlMangaEntry[] mangas;
        }

        private void getRemoteMangaData()
        {
            Stopwatch sw = Stopwatch.StartNew();
            using (var client = new HttpClient())
            {
                var responseString = client.GetStringAsync("http://myanimelist.net/malappinfo.php?u=" + Settings.Instance.getUsername() + "&status=all&type=manga");

                /*XmlDocument xmlData = new XmlDocument();
                xmlData.LoadXml(responseString.Result);

                XmlNodeList xnList = xmlData.SelectNodes("/myanimelist/manga");

                foreach (XmlNode xn in xnList)
                {
                    string title = xn["series_title"].InnerText;
                    string image = xn["series_image"].InnerText;
                    int id = Int32.Parse(xn["series_mangadb_id"].InnerText);
                    mangaRemote.Add(id, new MangaAnimeItem(id, title, image));
                }*/

                XmlSerializer serializer = new XmlSerializer(typeof(XmlManga));
                TextReader reader = new StringReader(responseString.Result);
                XmlManga manga = (XmlManga)serializer.Deserialize(reader);

                foreach (XmlMangaEntry entry in manga.mangas)
                {
                    mangaRemote.Add(entry.id, new MangaAnimeItem(entry.id, entry.Title, entry.Image));
                }
            }
            sw.Stop();
            Console.WriteLine("Manga remote data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void updateLocalData(MangaAnimeItems locals, MangaAnimeItems remote, ref int added, ref int updated)
        {
            foreach (MangaAnimeItem remoteItem in remote.Values)
            {
                if (!locals.ContainsKey(remoteItem.id))
                {
                    locals.Add(remoteItem.id, remoteItem);
                    added++;
                    continue;
                }

                bool changed = false;
                MangaAnimeItem localItem = locals[remoteItem.id];
                if (!localItem.EqualsImageUrl(remoteItem))
                {
                    localItem.url = remoteItem.url;
                    changed = true;
                }

                if (localItem.name != remoteItem.name)
                {
                    localItem.name = remoteItem.name;
                    changed = true;
                }

                if (changed) updated++;
            }
        }

        private void updateAnimeDataInLocal()
        {
            Stopwatch sw = Stopwatch.StartNew();
            updateLocalData(animeLocal, animeRemote, ref animeAdded, ref animeUpdated);
            sw.Stop();
            Console.WriteLine("Anime local data updated: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void updateMangaDataInLocal()
        {
            Stopwatch sw = Stopwatch.StartNew();
            updateLocalData(mangaLocal, mangaRemote, ref mangaAdded, ref mangaUpdated);
            sw.Stop();
            Console.WriteLine("Manga local data updated: {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void saveAnimeToCss()
        {
            //Console.WriteLine("Write new anime css file.");
            Stopwatch sw = Stopwatch.StartNew();

            StringBuilder sb = new StringBuilder(animeLocal.Count * 256);

            foreach (MangaAnimeItem item in animeLocal.Values)
            {
                sb.Append("#more" + item.id.ToString() + " {background-image: url(" + item.url + ");} /* " + item.name + " */\r\n");
                //css += "#more" + item.id.ToString() + " {background-image: url(" + item.url + ");} /* " + item.name + " */\r\n";
            }

            sw.Stop();

            Console.WriteLine("String generate: {0}ms.", sw.Elapsed.TotalMilliseconds);

            sw.Restart();

            using (StreamWriter writetext = new StreamWriter(Settings.Instance.getAnimeFilePath()))
            {
                writetext.Write(sb.ToString());
            }

            sw.Stop();
            Console.WriteLine("Saved do file {0}ms.", sw.Elapsed.TotalMilliseconds);

            sw.Restart();

            if (Settings.Instance.getMinFileActivate())
            {
                string cssMin = Utils.RemoveWhiteSpaceFromStyles(sb.ToString());

                using (StreamWriter writetext = new StreamWriter(Settings.Instance.getAnimeMinFilePath()))
                {
                    writetext.Write(cssMin);
                }
            }

            sw.Stop();
            Console.WriteLine("Min css {0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        private void saveMangaToCss()
        {
            //Console.WriteLine("Write new manga css file.");
            Stopwatch sw = Stopwatch.StartNew();

            StringBuilder sb = new StringBuilder(mangaLocal.Count * 256);

            foreach (MangaAnimeItem item in mangaLocal.Values)
            {
                sb.Append("#more" + item.id.ToString() + " {background-image: url(" + item.url + ");} /* " + item.name + " */\r\n");
            }

            using (StreamWriter writetext = new StreamWriter(Settings.Instance.getMangaFilePath()))
            {
                writetext.Write(sb.ToString());
            }

            if (Settings.Instance.getMinFileActivate())
            {
                string cssMin = Utils.RemoveWhiteSpaceFromStyles(sb.ToString());

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
