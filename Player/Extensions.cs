using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer2
{
    public static class Extensions
    {
        public static string ForSearch(this string texto)
        {
            if (!string.IsNullOrEmpty(texto))
            {
                string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
                string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

                for (int i = 0; i < comAcentos.Length; i++)
                {
                    texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
                }
                return texto.ToLower().TrimEnd();
            }
            return texto;
        }
    }
}
