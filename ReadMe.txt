====================================================
P2Acars - © roland_lfor 2023
====================================================

Prerequisites
-------------
- You need a Hoppie account, see here: https://www.hoppie.nl/acars/system/register.html
	You can use the same account for both your aircraft and P2Acars (you don't need 2)
- Obviously you need to have Pilot2ATC program and a Hoppie compatible client
	(On MSFS you have the Fenix A320, on X-Plane the Toliss for example)
- An Internet connection to push messages through Hoppie server ;)

	
Installation
-------------
- Put the P2Acars.exe in a folder of your choice
- Pilot2ATC setting: In Config / Speech tab / "Conversation Text File Path" group, 
	tick both "Enabled" and "New on startup" options.
	For the text file choice, Set "P2Aconversation.txt" in your %LOCALAPPDATA% system folder
	(which points usually to "C:\Users\<user>\AppData\Local")

How it works
-------------
- P2Acars needs 2 input from you at startup:
	1/ Your SimBrief ID (only to grant a unique ATC name within Hoppie network)
	2/ Your flight CALLSIGN, on which P2Acars messages will be send
- They will be requested upon start from the console
	=> to avoid the need to enter each time your SimBrief ID, 
	you can pass it as an argument to PAcars.exe. 
	You can do that in a BAT file, or using FSUIPC auto start capabilities,
	or maybe also in the EXE.xml of MSFS
	
Usage
-------------
1/ Start you sim, start P2Acars (and enter info) and Pilot2ATC
2/ Once in your aircraft (any Hoppie compatible client), you have to:
	a/ Enter your correct CALLSIGN in the FMS/INIT page (the one you set in P2Acars)
	b/ Request connection to the ATC station (ATC MSG page, Connection->Notification in the A320)
	using the name shown in GREEN in P2Acars console
	=> You won't receive any messages until your request is received and approved
	via confirmation message "LOGON ACCEPTED" that should be displayed on ARCDU
3/ Run your flight as usual with Pilot2ATC discussion

 - If you request disconnection, P2Acars message diffusion is stopped