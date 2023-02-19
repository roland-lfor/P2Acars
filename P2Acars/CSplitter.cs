using System;
using System.Collections.Generic;

namespace P2Acars
{// ATTENTION : la chaine source est systématiquement raccourcie de la partie en amont de la clé trouvée
    class CSplitter 
    {
        static readonly char[] sep = { ' ', ',', '.' };
        string dest;
        string[] sWordList;
        int i, j ,k , iEnd, endkeypos, keylen;
        CIcaoDic icaoDic;

        public CSplitter(CIcaoDic dic)
        {
            endkeypos = -1;
            icaoDic = dic;
        }

        public string Word2DicUntil(ref string source, string sEnd, string[] key)
        {
            //if(source.Contains(sEnd) == false)
            //{
            //    return Word2DicAll(ref source, key);
            //}
            dest = "";
            keylen = key.Length;
            // Recherche de la clé parmi le tableau de chaine key1
            for (k = 0; k < keylen; k++)
            {
                i = source.IndexOf(key[k]);
                if (i >= 0)
                    break;
            }

            if (i >= 0) // remplissage du tableau de mots après la position de la clé trouvée
            {
                // *** retire de la chaine source la partie en amont de la clé trouvée **
                source = source.Substring(i);
                endkeypos = key[k].Length;
                iEnd = source.IndexOf(sEnd, 1); // commence à 1 en cas d'égalité entre key et sEnd
                if(iEnd > 0)
                    sWordList = source.Substring(endkeypos, iEnd-endkeypos).Split(sep, StringSplitOptions.RemoveEmptyEntries);
                else
                    sWordList = source.Substring(endkeypos).Split(sep, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    for (j = 0; j < sWordList.Length; j++)
                    {
                        if (sWordList[j] == sEnd)   // on sort dès qu'on a trouvé la clé de fin
                            break;
                        else
                            dest += icaoDic[sWordList[j]];
                    }
                }
                catch (KeyNotFoundException)
                {
                    Console.Write("*** Exception: Key {0} missing in Dictionnary! ***\n", sWordList[j]);
                }
            }
            return dest;
        }

        public string Word2Dic(ref string source, string[] key, int nbr)
        {
            dest = "";
            keylen = key.Length;

            for (k = 0; k < keylen; k++)
            {
                i = source.IndexOf(key[k]);
                if (i >= 0)
                    break;
            }

            if (i >= 0)
            {
                // *** retire de la chaine source la partie en amont de la clé trouvée **
                source = source.Substring(i);
                endkeypos = key[k].Length; 
                sWordList = source.Substring(endkeypos).Split(sep, nbr + 1, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    for (j = 0; j < nbr; j++)
                    {
                        dest += icaoDic[sWordList[j]];
                    }
                }
                catch (KeyNotFoundException)
                {
                    Console.Write("*** Exception: Key {0} missing in Dictionnary! ***\n", sWordList[j]);
                }
            }
            return dest;
        }

        public string Word(ref string source, string[] key, int nbr = 1)
        {
            dest = "";
            keylen = key.Length;

            for (k = 0; k < keylen; k++)
            {
                i = source.IndexOf(key[k]);
                if (i >= 0)
                    break;
            }

            if (i >= 0)
            {
                // *** retire de la chaine source la partie en amont de la clé trouvée **
                source = source.Substring(i);
                endkeypos = key[k].Length;
                sWordList = source.Substring(endkeypos).Split(sep, nbr + 1, StringSplitOptions.RemoveEmptyEntries);  // il en faut 1 de plus car tout le reste est mis dedans !
                for (int j = 0; j < nbr; j++)
                {
                    dest += sWordList[j];
                }
            }
            return dest.ToUpper();
        }

        public string WordUntil(ref string source, string sEnd, string[] key)
        {
            if (source.Contains(sEnd) == false)
            {
                return Word(ref source, key);
            }
            dest = "";
            keylen = key.Length;

            for (k = 0; k < keylen; k++)
            {
                i = source.IndexOf(key[k]);
                if (i >= 0)
                    break;
            }

            if (i >= 0)
            {
                // *** retire de la chaine source la partie en amont de la clé trouvée **
                source = source.Substring(i);

                endkeypos = key[k].Length;
                iEnd = source.IndexOf(sEnd, 1);
                sWordList = source.Substring(endkeypos, iEnd - endkeypos).Split(sep, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < sWordList.Length; j++)
                {
                    if (sWordList[j] == sEnd)   // on sort dès qu'on a trouvé la clé de fin
                        break;
                    else
                        dest += sWordList[j]+" ";
                }
            }
            return dest.ToUpper();
        }


        public string Word2DicAll(ref string source, string[] key)
        {
            dest = "";
            keylen = key.Length;

            for (k = 0; k < keylen; k++)
            {
                i = source.IndexOf(key[k]);
                if (i >= 0)
                    break;
            }
            if (i >= 0)
            {
                source = source.Substring(i); // retire de la chaine source la partie amont **
                endkeypos = key[k].Length;

                sWordList = source.Substring(endkeypos).Split(sep, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    for (j = 0; j < sWordList.Length; j++)
                    {
                        dest += icaoDic[sWordList[j]];
                    }
                }
                catch (KeyNotFoundException)
                {
                    Console.Write("*** Exception: Key {0} missing in Dictionnary! ***\n", sWordList[j]);
                }
            }
            return dest;
        }
    }
}
