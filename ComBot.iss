function main()
{
	LavishScript:RegisterEvent[Github_Patched]
	Event[Github_Patched]:AttachAtom[Github_Patched]
	LavishScript:RegisterEvent[ComBotPatcher_Patched]
	Event[ComBotPatcher_Patched]:AttachAtom[ComBotPatcher_Patched]
	LavishScript:RegisterEvent[ComBot_Patched]
	Event[ComBot_Patched]:AttachAtom[ComBot_Patched]
	dotnet GithubPatcher GithubPatcher.exe VendanAndrews GithubPatcher master/bin/Release ".NET Programs" Github_Patched
}

atom(globalkeep) Github_Patched()
{
	dotnet ComBotPatcher GithubPatcher.exe VendanAndrews CombotPatcher master/ComBot.iss Scripts ComBotPatcher_Patched
}

atom(globalkeep) ComBotPatcher_Patched()
{
	dotnet ComBot GithubPatcher.exe Tehtsuo Combot experimental Scripts/Combot ComBot_Patched
}

atom(globalkeep) Combot_Patched()
{
	echo Launching ComBot
	run combot/combot
}