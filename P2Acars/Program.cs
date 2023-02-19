using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Timers;
using System.Reflection;

namespace P2Acars
{
    class Program
    {
        static readonly HttpClient hoppie = new HttpClient();
        private static Timer pollTimer;
        static readonly bool bTrace = true;
        static DateTime lastTime = DateTime.Now;
        static string sAtcPos = "P2A";
        static Int32 acrftID, sendMsgID = 1;        // compteur de messages envoyés (autre que Poll)
        static Int32 nP2AlastRead = -1;             // Dernière ligne lue dans le log P2A
        static bool bAcftConnected = false;
        // %LAPPDATA% equivalent :
        static readonly string sP2Afolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        static readonly string sP2Afile = "P2AConversation.txt";
        static readonly string sP2Alog = sP2Afolder + "\\" + sP2Afile;
        static readonly CIcaoDic icaoDic = new CIcaoDic();
        static string sCallsign = "AFR278";         // affectation en DEBUG only
        static readonly char[] sep = { ' ', ',', '.' };
        static CSplitter sp = new CSplitter(icaoDic);

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.Write("        P2Acars version {0} © roland_lfor - 2023\n", Assembly.GetEntryAssembly().GetName().Version);

            if (args.Length > 0)
            {
                // Simbrief ID passée
                if (args[0].Length > 3)
                    sAtcPos += args[0];
            }
            else
            {
                while (sAtcPos.Length == 3)
                {
                    Console.Write("Enter SIMBRIEF ID:\n");
                    sAtcPos = "P2A" + Console.ReadLine();
                }
            }
#if !DEBUG
            sCallsign = "";
            while (sCallsign.Length < 4)
            {
                Console.Write("Enter aircraft CALLSIGN:\n");
                sCallsign = Console.ReadLine().ToUpper();
            }
#endif
            Console.WriteLine();
            Console.Write("Your personal ATC: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("{0}\n", sAtcPos);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Target Callsign: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("{0}\n", sCallsign);
            Console.ForegroundColor = ConsoleColor.Gray;

            // Timer 2 Poll message from Hoppie (to accept connection mostly)
            PollHoppie(); // first Poll
            pollTimer = new System.Timers.Timer(55000);
            pollTimer.Elapsed += OnTimerEvent;
            pollTimer.Enabled = true;

            // Surveillance log P2A
            FileSystemWatcher watcher = new FileSystemWatcher(sP2Afolder);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnP2AMsg;
            watcher.Filter = sP2Afile;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
        private static void OnTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            PollHoppie();
        }

        static async void PollHoppie()
        {
            CAcarMsg msg = new CAcarMsg("poll", sAtcPos, sAtcPos, "");

            try
            {
                HttpResponseMessage response = await hoppie.GetAsync(msg.PrepareOut());
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                if (bTrace && responseBody.Length > 5) Console.WriteLine("Hoopie Poll <== {0}", responseBody);
                // Envoyer l'acceptation de connexion au client
                if (responseBody.Contains("REQUEST LOGON"))
                {
                    OnLogonReq(ref responseBody);
                }
                else if (responseBody.Contains("LOGOFF"))
                { 
                    bAcftConnected = false;
                    Console.WriteLine("* Disconnected by Aircraft *");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught! {0} ", e.Message);
            }
        }

        private static async Task PostHoppie(CAcarMsg msg)
        {
            try
            {
                if (bTrace) Console.WriteLine("Hoppie ->: " + msg.sPacket);
                HttpResponseMessage response = await hoppie.GetAsync(msg.PrepareOut());
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                if (bTrace)
                {
                    //Console.WriteLine("Hoppie ->: " + msg.sPacket);
                    Console.WriteLine("Hoppie <-: " + responseBody);
                }
                sendMsgID++;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught! {0} ", e.Message);
            }
        }

        public static void OnLogonReq(ref string logMsg)
        {
            string paquet, saID, sTarget, choice = "ACCEPTED";
            int pos1, pos2;

            pos1 = logMsg.IndexOf("{/data2/");
            pos2 = logMsg.IndexOf('/', pos1 + 8);
            saID = logMsg.Substring(pos1 + 8, pos2 - pos1 - 8);

            if (logMsg.Contains(sCallsign))
            {
                bAcftConnected = true;
                sTarget = sCallsign;
            }
            else
            {
                choice = "REJECTED";
                sTarget = sp.Word(ref logMsg, new string[] { "{" }, 1);    // récupère le nom de l'intrus pour répondre
                Console.WriteLine("Connection refused, unkown CALLSIGN: {0}", sTarget);
            }

            if (saID != "")
            {
                acrftID = int.Parse(saID);  // mémorise pour tous les envois suivants
                paquet = String.Format(@"/data2/{0}/{1}/NE/LOGON {2}", sendMsgID, saID, choice);
                CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sTarget, paquet);
                PostHoppie(msg);
            }
        }

        private static async void OnP2AMsg(object sender, FileSystemEventArgs e)
        {
            // transmet seulement si on a accepté une demande de connexion
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                DateTime newTime = DateTime.Now;
                TimeSpan elapsed = new TimeSpan(newTime.Ticks - lastTime.Ticks);
                if (elapsed.TotalSeconds > 1 )
                {
                    lastTime = newTime;
                    await Task.Delay(500);
                    InterpretP2A();
                }
            }
        }

        static void InterpretP2A()
        {
            if (File.Exists(sP2Alog))
            {
                string readAtc;
                try
                {
                    StreamReader sr = File.OpenText(sP2Alog);
                    string s;
                    int i = 0;
                    while ((s = sr.ReadLine()) != null)
                    {
#if !DEBUG
                        if (bAcftConnected)
                        {
#endif                        
                            if (i > nP2AlastRead)
                            {
                                if (s.Contains("ATC:"))
                                {
                                    readAtc = s.Substring(11).ToLower();

                                    // Par ordre de priorité décroissante :
                                         if(readAtc.Contains("is cleared to "))         OnClearance(ref readAtc);
                                    else if(readAtc.Contains("taxi to runway "))        OnTaxi(ref readAtc);
                                    else if(readAtc.Contains("expect "))                OnExpect(ref readAtc);
                                    else if(readAtc.Contains("climb "))                 OnClimbFL(ref readAtc);
                                    else if(readAtc.Contains("descend to cross"))       OnStartDescent(ref readAtc);
                                    else if(readAtc.Contains("descend and maintain "))  OnDescend(ref readAtc);
                                    else if(readAtc.Contains("cleared for "))           OnClearedApp(ref readAtc);
                                    else if(readAtc.Contains("taxi to "))               OnTaxi2Gate(ref readAtc);
                                    else if(readAtc.Contains("contact"))                OnContactCenter(ref readAtc);
                                    else if(readAtc.Contains("turn "))                  OnTurn(ref readAtc);
                                }
                            }
#if !DEBUG
                        }
#endif
                        i++;
                    }
                    sr.Close();
                    nP2AlastRead = i - 1;
                }
                catch (IOException e)
                {
                    Console.WriteLine("P2A log read error: {0}", e.Message);
                }
            }
 //           return bRet;
        }

        static void OnClearance(ref string s)
        {
            
            string sTmp, sAirpt, sSid, sTrans, sRwy, sAlt, sFreq, sSquawk, sP2Amsg;
            
            string[] key1 = { "is cleared to " };                   // airport ICAO
            string[] key2 = { "climb via the ", "fly the "};        // SID
            string[] key3 = { "with the " };                        // Transition (Option)
            string[] key4 = { "expect departure runway " };         // rwy
            string[] key5 = { "climb to ", "climb and maintain " }; // initial ALT
            string[] key6 = { "approach on ", "departure on "}; // next freq: 3 mots "decimal" (ou "point") 1 ou plusieurs mots et "."
            string[] key7 = {"squawk "};

            // Destination
            sAirpt = sp.Word2Dic(ref s, key1, 4);
            // SID (Opt)
            sSid = sp.Word(ref s, key2, 1);
            // TRANS (Opt)
            sTrans = "";  sTmp = sp.Word(ref s, key3, 1);
            if (sTmp != "")
                sTrans = " via " + sTmp;
            // RWY
            sRwy = sp.Word2DicUntil(ref s, ". ", key4);
            // ALT
            sAlt = FixAltitude( sp.Word2DicUntil(ref s, ". ", key5) );
            // FREQ
            sFreq = sp.Word2DicUntil(ref s, key7[0], key6);
            // SQUAWK
            sSquawk = sp.Word2Dic(ref s, key7, 4);

            sP2Amsg = $"/data2/{sendMsgID}//N/CLEARED to @{sAirpt}@ @{sSid}@{sTrans} RWY@{sRwy}@ ALT {sAlt} DEP @{sFreq}@ SQUAWK @{sSquawk}@";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);
        }

        static void OnTaxi(ref string s)
        {
            string sRwy, sTaxiwys, sHold, sP2Amsg;
            string[] key1 = { "taxi to runway " };
            string[] key2 = { "via taxiways " };
            string[] key3 = { "hold short " };

            // Rwy
            sRwy = sp.Word2DicUntil(ref s, "via", key1);
            // Taxiways
            sTaxiwys = sp.Word2DicUntil(ref s, "hold", key2);
            spaceTaxiways(ref sTaxiwys);
            // Hold short
            sHold = GetAllOf(ref s, key3);

            sP2Amsg = $"/data2/{sendMsgID}//WU/TAXI RWY @{sRwy}@ via @{sTaxiwys}@ HOLD {sHold}@";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);
        }

        static string GetAllOf(ref string s, string[] first)
        {
            string[] then = { ", and " };
            string sTmp = "";
            
            sTmp = "@" + sp.Word2DicUntil(ref s, then[0], first);

            while(s.IndexOf(then[0]) > 0)
            {
                sTmp += "@ and @" + sp.Word2DicUntil(ref s, then[0], then);
            }

            return sTmp;
        }

        static void spaceTaxiways(ref string sTaxiwys)
        {
            char last=' ';
            int i = 0;
            string sNew="";
            foreach (char c in sTaxiwys)
            {
                i++;
                if (i == 1)
                {
                    last = c;
                    sNew += c;
                    continue;
                }
                if ( (Char.IsDigit(last) && Char.IsLetter(c)) || (Char.IsLetter(last) && Char.IsLetter(c)) )
                    sNew += "@ @";
                sNew += c;
                last = c;
            }
            sTaxiwys = sNew;
        }

        static void OnExpect(ref string s) // N flag
        {
            string sStar, sApp, sTrans1, sTrans2, sRwy, sP2Amsg, sTmp;
            string[] key1 = { "expect the ","expect direct " };          // STAR
            string[] key2 = { "with the " };            // Trans
            string[] key3 = { "for the " };             // App
            string[] key4 = { "approach to runway " };  // RWY

            // STAR : on doit faire en 2 passes !
            sStar = sp.Word(ref s, key1, 1); // récupère le corps
            if (sStar.Length > 2)
            {
                sStar = sStar.ToLower();
                sStar += sp.Word2DicUntil(ref s, "arrival", new string[] {sStar});
                sStar = sStar.ToUpper();
            }
            // FROM
            sTrans1 = sp.Word(ref s, key2, 1);
            // APP
            sApp = sp.Word2DicUntil(ref s, "approach", key3);
            // RWY
            sRwy = sp.Word2DicUntil(ref s, "with", key4);
            // TRANS
            sTrans2 = sp.Word(ref s, key2, 1);

            sP2Amsg = $"/data2/{sendMsgID}//N/EXPECT @{sStar}@ via @{sTrans1}@ to @{sApp}@ RWY@{sRwy}@ trans @{sTrans2}@";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);

            if (s.Contains("contact "))
                OnContactCenter(ref s);
        }

        static void OnClimbFL(ref string s)     // WU flag
        {
            string sAlt, sP2Amsg;
            string[] key1 = { "climb to ", "climb and maintain " };
            bool bContact = s.Contains("contact ");

            if(bContact)
                sAlt = FixAltitude( sp.Word2DicUntil(ref s, "contact", key1));
            else
                sAlt = FixAltitude( sp.Word2DicAll(ref s, key1));

            sP2Amsg = $"/data2/{sendMsgID}//WU/CLIMB TO {sAlt}";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);

            if (bContact)
                OnContactCenter(ref s);
        }
        static void OnTurn(ref string s)
        {
            if (s.Contains("to return to"))
                return;

            string sHdg, sP2Amsg, sThen = "";
            string[] key1 = { "turn " };
            string[] key2 = { "then " };

            bool bContact = s.Contains("contact ");
            bool bThen    = s.Contains(key2[0]);

            if (bThen)
            { 
                sHdg = sp.Word2DicUntil(ref s, "then ", key1);
            }
            else if(bContact && ! bThen)
                sHdg = sp.Word2DicUntil(ref s, "contact", key1);
            else
                sHdg = sp.Word2DicAll(ref s, key1);

            if (bContact && bThen)
                sThen = " then " + sp.Word2DicUntil(ref s, "contact", key2);
            else if(bThen)
                sThen = " then " + sp.Word2DicAll(ref s, key2);

            sP2Amsg = $"/data2/{sendMsgID}//N/TURN {sHdg}@{sThen}";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);

            if (bContact)
                OnContactCenter(ref s);
        }
        static void OnDescend(ref string s)     // WU flag
        {
            string sAlt, sP2Amsg;
            string[] key1 = { "descend and maintain ", "descend to " };
            // ALT
            sAlt = FixAltitude(sp.Word2DicAll(ref s, key1));

            sP2Amsg = $"/data2/{sendMsgID}//WU/CLIMB TO {sAlt}";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);

            if (s.Contains("contact "))
                OnContactCenter(ref s);
        }

        static void OnStartDescent(ref string s) // WU flag
        {
            string[] sCross = { "", "" };
            string sTmp, sStar, sTrans, sQnh, sP2Amsg, sFirstCross ="";
            string[] key1 = { "descend to cross ", "descend to " };
            string[] key2 = { "at or ", "at " };
            string[] key3 = { "then  descend via the " };
            string[] key4 = { "with the " };
            string[] key5 = { "to cross " };
            string[] key6 = { "qnh is " };
 
            // Cross 1
            sCross[0] = sp.Word(ref s, key1, 1);
            if (s.Contains(key1[0]))
            {
                if (sCross[0].Length > 0)
                {
                    sFirstCross = sCross[0];
                    sCross[0] = "@" + sFirstCross + "@ AT ";
                    sCross[0] += FixAltitude(sp.Word2DicUntil(ref s, key3[0], key2));
                }
            }
            // STAR
            sStar = sp.Word(ref s, key3, 1); // récupère le corps
            if (sStar.Length > 0)
            {
                sStar = sStar.ToLower();
                sStar += sp.Word2DicUntil(ref s, "arrival", new string[] {sStar});
                sStar = sStar.ToUpper();
            }
            // Trans
            sTrans = sp.Word(ref s, key4, 1);
            // Cross 2 (Opt)
            sTmp = sp.Word(ref s, key5, 1);
            if(sTmp != sFirstCross && sTmp.Length > 0)
            {
                sCross[1] = "cross @"+ sTmp +"@ ";
                sCross[1] += FixAltitude(sp.Word2DicUntil(ref s, "qnh", key2));
            }
            // QNH
            sQnh = sp.Word2DicUntil(ref s, "at ", key6);

            sP2Amsg = $"/data2/{sendMsgID}//WU/DESCEND TO {sCross[0]} @{sStar}@ VIA {sTrans} {sCross[1]} QNH @{sQnh}@";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);

        }
        static void OnTaxi2Gate(ref string s) // WU flag
        {
            string sGate, sTaxiwys, sHold, sP2Amsg;
            string[] key1 = { "taxi to  ramp ", "taxi to  gate ", "taxi to  parking " };
            string[] key2 = { "via taxiways " };
            string[] key3 = { "hold short" };
            bool bHold = s.Contains(key3[0]);

            // ALT
            sGate = sp.Word2DicUntil(ref s, "via", key1);
            // Taxiways
            if (bHold)
            {
                sTaxiwys = sp.Word2DicUntil(ref s, "hold", key2);
                spaceTaxiways(ref sTaxiwys);
                // Hold short
                sHold = GetAllOf(ref s, key3);
            }
            else
            {
                sTaxiwys = sp.Word2DicAll(ref s, key2);
                sHold = "";
            }

            spaceTaxiways(ref sTaxiwys);

            if (bHold)
                sP2Amsg = $"/data2/{sendMsgID}//WU/TAXI TO GATE @{sGate}@ VIA @{sTaxiwys}@ HOLD {sHold}@";
            else
                 sP2Amsg = $"/data2/{sendMsgID}//WU/TAXI TO GATE @{sGate}@ VIA @{sTaxiwys}@";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);
        }

        static void OnContactCenter(ref string s)
        {
            string sCenter, sFreq, sP2Amsg;
            string[] key1 = { "contact " };
            string[] key2 = { " on " };

            sCenter = sp.WordUntil(ref s, "on ", key1);
            sFreq = sp.Word2DicAll(ref s, key2);
            if (sFreq != "")
            { 
                sP2Amsg = $"/data2/{sendMsgID}//N/CONTACT {sCenter}on @{sFreq}@";
                CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
                PostHoppie(msg);
            }
        }

        static void OnClearedApp(ref string s)
        {
            string sApp, sRwy, sP2Amsg;
            string[] key1 = { "cleared for the ", "cleared for " };
            string[] key2 = { "runway " };
            bool bContact = s.Contains("contact ");

            sApp    = sp.WordUntil(ref s, "to runway", key1);
            if(bContact)
                sRwy = sp.Word2DicUntil(ref s, "contact", key2);
            else 
                sRwy = sp.Word2DicUntil(ref s, "at ", key2);

            sP2Amsg = $"/data2/{sendMsgID}//N/CLEARED for {sApp} to RWY@{sRwy}@";
            CAcarMsg msg = new CAcarMsg("cpdlc", sAtcPos, sCallsign, sP2Amsg);
            PostHoppie(msg);

            if (bContact)
                OnContactCenter(ref s);
        }

        static string FixAltitude(string source)
        {
            string sfix="";
            int i, j;
            bool bThousand = false;
            bool bHundred = false;

            if (source.Contains("FL"))
            {
                sfix = "@"+source+"@";
                return sfix;
            }

            if(source.Length > 0)
            {
                string[] sWordList = source.Split();
                for(j=0; j < sWordList.Length; j++)
                {
                    if (sWordList[j] == "ABOVE" || sWordList[j] == "BELOW")
                    {
                        sfix = sWordList[j] + " ";
                    }
                    else
                    {
                        if ( ! bThousand)
                        { 
                            i = sWordList[j].IndexOf("000");
                            if (i > 0)
                            {
                                sfix += "@";
                                sfix += sWordList[j].Substring(0, i);
                                bThousand = true;
                            } 
                        }
                        else if ( ! bHundred)
                        {
                            i = sWordList[j].IndexOf("00");
                            if (i > 0)
                            {
                                sfix += sWordList[j].Substring(0, i);
                                sfix += "00@FT";
                                bHundred = true;
                            }
                        }
                    }
                }
                if( ! bHundred)
                    sfix += "000@FT";
            }
            return sfix;
        }
    }
}
