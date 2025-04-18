using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer2
{
    public class Music
    {
        public int Nro { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string NameForSearch { get; set; }
        public bool IsInFilter { get; set; }
    }
}
