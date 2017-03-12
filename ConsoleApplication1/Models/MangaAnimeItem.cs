using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalStyleImageUpdater.Models
{
    class MangaAnimeItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public MangaAnimeItem(int id, string name, string url)
        {
            this.id = id;
            this.name = name;
            this.url = url;
        }

        public Boolean EqualsImageUrl(MangaAnimeItem newer)
        {
            return this.url == newer.url;
        }
    }
}
