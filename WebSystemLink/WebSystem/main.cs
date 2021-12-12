using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WebSystem
{
    class main
    {
        public static void Main(string[] args)
        {
            CWebCotentManager Content = new CWebCotentManager();
            Content.LoadConfig("WebInfo");

        }
    }
}
