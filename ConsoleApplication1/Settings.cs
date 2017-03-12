using MalStyleImageUpdater.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalStyleImageUpdater
{

    class Settings
    {
        private static Settings instance;

        private string myDocumetsPath;
        private string configPath;
        private string configFilename;

        private SettingsModel settingsModel;
        public Settings()
        {
            this.myDocumetsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.configPath = myDocumetsPath + "\\MalStyleImageUpdater\\";
            this.configFilename = "config.json";

            loadJson();
        }

        public static Settings Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Settings();
                }
                return instance;
            }
        }

        private void loadJson()
        {
            if(Directory.Exists(this.configPath))
            {
                if (File.Exists(this.configPath + this.configFilename))
                {
                    using (StreamReader r = new StreamReader(this.configPath + this.configFilename))
                    {
                        string json = r.ReadToEnd();
                        this.settingsModel = JsonConvert.DeserializeObject<SettingsModel>(json);
                    }
                } else
                {
                    createSettings();
                }
            } else
            {
                Directory.CreateDirectory(this.configPath);
                createSettings();
            }

        }

        private void createSettings()
        {
            this.settingsModel = new SettingsModel();
            string username = "";
            string animePath = "";
            string mangaPath = "";
            string minFile = "";
            Boolean minFileBool = false;
            string animeMinPath = "";
            string mangaMinPath = "";

            ConsoleColor oldCC = Console.ForegroundColor;
            Console.ResetColor();

            while (String.IsNullOrEmpty(username) && String.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Enter your mal(MyAnimeList) username: ");
                username = Console.ReadLine();
            }

            while (String.IsNullOrEmpty(animePath) && String.IsNullOrWhiteSpace(animePath) && !File.Exists(animePath))
            {
                Console.WriteLine("Enter path to file mal_anime_images.css: ");
                animePath = Console.ReadLine();
            }

            while (String.IsNullOrEmpty(mangaPath) && String.IsNullOrWhiteSpace(mangaPath) && !File.Exists(mangaPath))
            {
                Console.WriteLine("Enter path to file mal_manga_images.css: ");
                mangaPath = Console.ReadLine();
            }

            while (String.IsNullOrEmpty(minFile) && String.IsNullOrWhiteSpace(minFile) && !(minFile.ToUpper().Equals("Y") | minFile.ToUpper().Equals("N")))
            {
                Console.WriteLine("You use min image files (y/N): ");
                minFile = Console.ReadLine();
            }

            if(minFile.ToUpper().Equals("Y"))
            {
                minFileBool = true;
            } else if (minFile.ToUpper().Equals("N"))
            {
                minFileBool = false;
            }

            while (String.IsNullOrEmpty(animeMinPath) && String.IsNullOrWhiteSpace(animeMinPath) && !File.Exists(animeMinPath))
            {
                Console.WriteLine("Enter path to file mal_anime_images.min.css: ");
                animeMinPath = Console.ReadLine();
            }

            while (String.IsNullOrEmpty(mangaMinPath) && String.IsNullOrWhiteSpace(mangaMinPath) && !File.Exists(mangaMinPath))
            {
                Console.WriteLine("Enter path to file mal_manga_images.min.css: ");
                mangaMinPath = Console.ReadLine();
            }


            this.setUsername(username);
            this.setAnimeFilePath(animePath);
            this.setMangaFilePath(mangaPath);
            this.setMinFileActivate(minFileBool);
            this.setAnimeMinFilePath(animeMinPath);
            this.setMangaMinFilePath(mangaMinPath);
            this.saveSettings();

            Console.ForegroundColor = oldCC;
        }

        public void setUsername(string username)
        {
            settingsModel.username = username;
        }

        public string getUsername()
        {
            return settingsModel.username;
        }

        public void setAnimeFilePath(string animePath)
        {
            settingsModel.animeImagePath = animePath;
        }

        public string getAnimeFilePath()
        {
            return settingsModel.animeImagePath;
        }

        public void setMangaFilePath(string mangaPath)
        {
            settingsModel.mangaImagePath = mangaPath;
        }

        public string getMangaFilePath()
        {
            return settingsModel.mangaImagePath;
        }

        public void setMinFileActivate(Boolean minFileActive)
        {
            settingsModel.minFileActivate = minFileActive;
        }

        public Boolean getMinFileActivate()
        {
            return settingsModel.minFileActivate;
        }

        public void setAnimeMinFilePath(string animeMinPath)
        {
            settingsModel.animeImageMinPath = animeMinPath;
        }

        public string getAnimeMinFilePath()
        {
            return settingsModel.animeImageMinPath;
        }

        public void setMangaMinFilePath(string mangaMinPath)
        {
            settingsModel.mangaImageMinPath = mangaMinPath;
        }

        public string getMangaMinFilePath()
        {
            return settingsModel.mangaImageMinPath;
        }

        public void saveSettings()
        {
            if (!Directory.Exists(this.configPath))
            {
                Directory.CreateDirectory(this.configPath);
            }
            {
                if (settingsModel != null)
                {
                    string output = JsonConvert.SerializeObject(settingsModel);

                    using (StreamWriter writetext = new StreamWriter(this.configPath + this.configFilename))
                    {
                        writetext.Write(output);
                    }
                }
            }
        }
    }
}
