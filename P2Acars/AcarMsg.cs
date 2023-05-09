using System;


namespace P2Acars
{
    //enum eAcarType
    //{
    //    poll,
    //    peek,
    //    cpdlc,
    //    telex,
    //    progress,
    //    ping,
    //    posreq,
    //    position,
    //    datareq,
    //}

    public class CAcarMsg
    {
 //       static readonly string sAcarHeader = "http://www.hoppie.nl/acars/system/connect.html?logon=4QtuMHquHMaVL";
        static readonly string sAcarHeader = "http://www.hoppie.nl/acars/system/connect.html?logon=";
        //readonly Type acarType = typeof(eAcarType);
        //acarType etype;
        public string sPacket, sFrom, sTo, sType, sFull, sLogon;
        
        public CAcarMsg(string sId, string Raw) 
        {
            sLogon = sId;
            DecodeIn(Raw);  
        }
        public CAcarMsg(string sId, string type, string from, string to, string paquet)
        {
            sLogon = sId;
            sType   = type;
            sFrom   = from;
            sTo     = to;
            sPacket = paquet;
        }

        bool DecodeIn(string sRaw)
        {
            string[] subs = sRaw.Split('&');
            string use;
            Int32 pos;

            foreach (string sub in subs)
            {
                pos = sub.IndexOf('=');
                
                if (pos > -1)
                { 
                    use = sub.Substring(pos);

                    if (sub.Contains("from")) sFrom=use;
                    else if (sub.Contains("to")) sTo=use;
                    else if (sub.Contains("type")) sType=use;
                    else if (sub.Contains("paquet")) sPacket = use;
                    //Console.WriteLine($"Substring: {sub}");
                }
            }
            return true;
        }

        public string PrepareOut()
        {
            bool bRet = true;
            if (sFrom != "" && sTo == "" && sType=="")
                bRet = false;
            else
            {
                sFull = sAcarHeader+sLogon+"&from="+sFrom+"&to="+sTo+"&type="+sType+"&packet="+ sPacket;
            }
            return sFull;
        }

        //void Type2string()
        //{
        //    switch (etype)
        //    {
        //        case eAcarType.ePoll:
        //            sType = "poll";
        //            break;
        //        case eAcarType.ePeek:
        //            sType = "peek";
        //            break;
        //        case eAcarType.eCpdlc:
        //            sType = "cpdlc";
        //            break;
        //        case eAcarType.eTelex:
        //            sType = "telex";
        //            break;
        //        case eAcarType.eProgress:
        //            sType = "progress";
        //            break;
        //        case eAcarType.ePing:
        //            sType = "ping";
        //            break;
        //        case eAcarType.ePosreq:
        //            sType = "posreq";
        //            break;
        //        case eAcarType.ePosition:
        //            sType = "position";
        //            break;
        //        case eAcarType.eDatareq:
        //            sType = "datareq";
        //            break;
        //    }
        //}

        //void String2type()
        //{
        //         if (sType == "poll")   etype=eAcarType.ePoll;
        //    else if (sType == "peek")   etype=eAcarType.ePeek;
        //}
    }
}
