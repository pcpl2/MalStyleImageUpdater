using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ConsoleApplication1
{

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Item(int id, string name, string url)
        {
            Id = id;
            Name = name;
            Url = url;
        }

        public Boolean EqualsImageUrl(Item newer)
        {
            return Url == newer.Url;
        }
    }
    class Program
    {
        static int added = 0;
        static int updated = 0;
        static SortedDictionary<String, Item> local = new SortedDictionary<string, Item>();
        static SortedDictionary<String, Item> remote = new SortedDictionary<string, Item>();

        static void Main(string[] args)
        {

            if (args.Length == 6)
            {
                if(args[0] == "-u" && args[2] == "-t" && args[4] == "-f")
                {
                    if (!(args[3] != "anime" || args[3] != "manga"))
                    {
                        Console.WriteLine("updateMalImageCss : wrong type, type is must anime or manga :)");
                    }

                    Console.WriteLine("Load local data.");
                    Stopwatch sw = Stopwatch.StartNew();
                    try
                    {
                        using (StreamReader reader = new StreamReader(args[5]))
                        {
                            parseCssFile(reader);
                        }
                    } catch(IOException ioex)
                    {
                        Console.WriteLine("test " + ioex.ToString());
                    }
                    sw.Stop();
                    Console.WriteLine("Local data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);

                    Console.WriteLine("Reading remote data.");
                    sw = Stopwatch.StartNew();
                    using (var client = new HttpClient())
                    {
                        var responseString = client.GetStringAsync("http://myanimelist.net/malappinfo.php?u=" + args[1] + "&status=all&type=" + args[3]);

                        XmlDocument xmlData = new XmlDocument();
                        xmlData.LoadXml(responseString.Result);

                        XmlNodeList xnList = xmlData.SelectNodes("/myanimelist/anime");

                        foreach (XmlNode xn in xnList)
                        {
                            string title = xn["series_title"].InnerText;
                            string image = xn["series_image"].InnerText;
                            string id = xn["series_animedb_id"].InnerText;
                            //Console.WriteLine("{0} - {1}: {2}", id, title, image);
                            remote.Add(id.ToString(), new Item(Int32.Parse(id), title, image));
                        }
                    }
                    sw.Stop();
                    Console.WriteLine("Remote data loaded: {0}ms.", sw.Elapsed.TotalMilliseconds);

                    Console.WriteLine("remote count: " + remote.Count());
                    Console.WriteLine("local count: " + local.Count());
                    Console.WriteLine("Updating...");
                    sw = Stopwatch.StartNew();
                    updateDataInLocal();
                    sw.Stop();
                    Console.WriteLine("Updated local data: {0}ms.", sw.Elapsed.TotalMilliseconds);
                    Console.WriteLine("Added : {0}", added);
                    Console.WriteLine("Updated : {0}", updated);

                    Console.WriteLine("Write new css file.");
                    sw = Stopwatch.StartNew();
                    saveToCss(args[5]);
                    sw.Stop();
                    Console.WriteLine("Write to file is done {0}ms.", sw.Elapsed.TotalMilliseconds);

                    Console.Read();
                }

            } else if(args.Length == 1)
            {
                if(args[0] == "-help")
                {

                }

            } else
            {
                Console.WriteLine("updateMalImageCss : missing username and type ");
                Console.WriteLine("Usage : updateMalImageCss -u [username] -t [anime/manga] -f [file Path] ");
                Console.WriteLine("Usage : ");
                Console.WriteLine("Try `updateMalImageCss -help' for more options.");

            }

            Console.Read();


        }

        static void parseCssFile(StreamReader reader)
        {
            String line;
            while((line = reader.ReadLine()) != null)
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
                } else
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

                if (!local.ContainsKey(Id.ToString()))
                    local.Add(Id.ToString(), new Item(Int32.Parse(Id), Name, Url));
                else
                    local[Id.ToString()] = new Item(Int32.Parse(Id), Name, Url);
            }
        }

        static void saveToCss(String path)
        {
            String css = "";
            foreach(Item item in local.Values)
            {
                css += "#more" + item.Id.ToString() + " {background-image: url(" + item.Url + ");} /* " + item.Name + " */\r\n";
            }

            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.Write(css);
            }
        }

        static void updateDataInLocal()
        {
            foreach(Item item in remote.Values)
            {
                Item localItem;
                if(local.ContainsKey(item.Id.ToString()))
                {
                    localItem = local[item.Id.ToString()];
                    if(!localItem.EqualsImageUrl(item))
                    {
                        Console.WriteLine("Local and remote image is not same {0} - {1}", item.Id.ToString(), item.Name);
                        Console.WriteLine("Old : {0}", localItem.Url);
                        Console.WriteLine("New : {0}", item.Url);
                        localItem = item;
                        local[item.Id.ToString()] = localItem;
                        updated++;
                    } else
                    {
                        if (localItem.Name.Length <= 0)
                        {
                            Console.WriteLine("Fixing title for {0}", localItem.Id.ToString());
                            localItem.Name = item.Name;
                            local[item.Id.ToString()] = localItem;
                        }
                        Console.WriteLine("Local and remote image is same {0} - {1} : {2}", localItem.Id.ToString(), localItem.Name, localItem.Url);
                    }
                } else
                {
                    Console.WriteLine("Local is not exist. Adding");
                    Console.WriteLine("Id : {0}", item.Id.ToString());
                    Console.WriteLine("Name : {0}", item.Name);
                    Console.WriteLine("ImageUrl : {0}", item.Url);
                    localItem = item;
                    local.Add(localItem.Id.ToString(), localItem);
                    added++;
                }
                Console.WriteLine("________________________________________\n");
            }
        }
    }
}
