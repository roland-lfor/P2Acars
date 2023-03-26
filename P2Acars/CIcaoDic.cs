using System.Collections.Generic;

namespace P2Acars
{
    class CIcaoDic : Dictionary<string, string>
    {
        public CIcaoDic()
        {
            // Digits
            Add("zero"   , "0");
            Add("one"    , "1");
            Add("two"    , "2");
            Add("three"  , "3");
            Add("four"   , "4");
            Add("five"   , "5");
            Add("six"    , "6");
            Add("seven"  , "7");
            Add("eight"  , "8");
            Add("nine"   , "9");
            Add("niner"  , "9");
            // Alphabet
            Add("alpha"      , "A");
            Add("bravo"      , "B");
            Add("charlie"    , "C");
            Add("delta"      , "D");
            Add("echo"       , "E");
            Add("foxtrot"    , "F");
            Add("golf"       , "G");
            Add("hotel"      , "H");
            Add("india"      , "I");
            Add("juliet"     , "J");
            Add("kilo"       , "K");
            Add("lima"       , "L");
            Add("mike"       , "M");
            Add("november"   , "N");
            Add("oscar"      , "O");
            Add("papa"       , "P");
            Add("quebec"     , "Q");
            Add("romeo"      , "R");
            Add("sierra"     , "S");
            Add("tango"      , "T");
            Add("uniform"    , "U");
            Add("victor"     , "V");
            Add("whiskey"    , "W");
            Add("x-ray"      , "X");
            Add("ex-ray"     , "X");
            Add("yankee"     , "Y");
            Add("zulu"       , "Z");
            // Freq
            Add("hundred", "00 FT");
            Add("thousand", "000 ");
            Add("decimal", ".");
            Add("point", ".");
            // Runway
            Add("runway", "RWY");
            Add("left", "L ");
            Add("right", "R ");
            Add("center", "C");
            // Altitude
            Add("flight", "F");
            Add("level", "L");
            Add("feet", "");
            Add("above", "ABOVE ");
            Add("below", "BELOW ");
            // Approches
            Add("ils", "ILS");
            Add("igs", "IGS");
            Add("vor", "VOR");
            Add("vortac", "VORTAC");
            Add("rnav", "RNAV");
            Add("ndb", "NDB");
            Add("visual", "VISUAL");
            Add("approach", "APP");
            // Divers
            Add("or", "");
            Add("at", "");
            Add("via", "");
            Add("the", "");
            Add("then", "");
            Add("direct", "DIRECT ");
            Add("descend", "");
            Add("heading", "HEADING @");
            Add("enjoy", "");
            Add("have", "");
            Add("nice", "");
            Add("good", "");
            Add("goodday", "");
            Add("a", "");
            Add("day", "");
            Add("your", "");
            Add("morning", "");
            Add("afternoon", "");
            Add("evening", "");
            Add("stay", "");
            Add("departure", "");
        }
    }
}
