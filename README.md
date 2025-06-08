# NLBE-Bot


## Installation

### On Windows

01. Copy the binaries to `C:\ProgramData\NLBEBot\`
02. Install the latest .NET 9.0 Runtime from <https://dotnet.microsoft.com/en-us/download/dotnet/9.0>.	
03. Run PowerShell as an Administrator.
04. Register a Windows Service by executing the following commands:

	```pwsh
	sc create "NLBEBot" binpath="C:\ProgramData\NLBEBot\NLBE-Bot.exe"
	sc config "NLBEBot" start=auto
	```