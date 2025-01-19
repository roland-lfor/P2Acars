====================================================
		P2Acars - roland_lfor 2023
====================================================

 Prerequisites
-------------------------------
- You need a Hoppie account, see here: https://www.hoppie.nl/acars/system/register.html
	You can use the same account for both your aircraft and P2Acars (you don't need 2)
- Obviously you need to own and use Pilot2ATC program, and a Hoppie compatible client
	For example on MSFS you have the Fenix A320 and FBW A32X, on X-Plane the Toliss
- An Internet connection to push messages through Hoppie server ;)
	
 Installation and configuration
-------------------------------
- Put the P2Acars.exe in a folder of your choice
- Pilot2ATC setting: In Config / Speech tab / "Conversation Text File Path" group, 
	tick both "Enabled" and "New on startup" options.
	For the text file choice, Set "P2Aconversation.txt" in your %LOCALAPPDATA% system folder
	(which points usually to "C:\Users\<user>\AppData\Local")

 Startup
-------------------------------
- P2Acars needs two inputs from you at startup:
	1/ your Hoppie Logon
	2/ Your flight CALLSIGN
  They will be requested upon start from the P2Acars console,
  OR you can pass them optionnaly as arguments to P2Acars.exe:
	1st arg must be Hoppie Logon and 2nd the Callsign.
  You can do that in a BAT file, or using FSUIPC auto start capabilities,
	 or maybe also in the EXE.xml of MSFS.
  Cmd line sample with both args: "P2Acars 75dOZ7125w BAW4578T"
	
 Usage Overview
-------------------------------
1/ Start you sim, start P2Acars (enter your Hoppie Logon & Callsign), Start Pilot2ATC
2/ Once in your aircraft (any Hoppie compatible client), you have to:
	a/ Enter your correct CALLSIGN in the FMS/INIT page (the one you set in P2Acars)
	b/ Request connection to the ATC station (ATC MSG page, Connection->Notification in the A320)
	using the 4 digits identification number shown in GREEN in P2Acars console
	=> You won't receive any messages until your request is received and approved
	via confirmation message "LOGON ACCEPTED" that should be displayed on ARCDU
3/ Run your flight as usual with Pilot2ATC discussion, and important messages
	will be send to your ARCDU. Some of them will require an answer (Stdbye / Wilco / Unable)
4/ If you request disconnection, P2Acars message broadcast is stopped
