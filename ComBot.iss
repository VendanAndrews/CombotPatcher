function main()
{
	LavishScript:RegisterEvent[Github_Patched]
	Event[Github_Patched]:AttachAtom[Github_Patched]
	LavishScript:RegisterEvent[ComBotPatcher_Patched]
	Event[ComBotPatcher_Patched]:AttachAtom[ComBotPatcher_Patched]
	LavishScript:RegisterEvent[LSMIPC_Patched]
	Event[LSMIPC_Patched]:AttachAtom[LSMIPC_Patched]
	LavishScript:RegisterEvent[ComBot_Patched]
	Event[ComBot_Patched]:AttachAtom[ComBot_Patched]
	dotnet GithubPatcher GithubPatcher.exe VendanAndrews GithubPatcher master/bin/Release ".NET Programs" Github_Patched
}

atom(globalkeep) Github_Patched(bool updated)
{
	dotnet ComBotPatcher GithubPatcher.exe VendanAndrews CombotPatcher master/ComBot.iss Scripts ComBotPatcher_Patched
	Event[Github_Patched]:DetachAtom[Github_Patched]
}

atom(globalkeep) ComBotPatcher_Patched(bool updated)
{
	Event[ComBotPatcher_Patched]:DetachAtom[ComBotPatcher_Patched]
	if ${updated}
	{
		Event[ComBot_Patched]:DetachAtom[Combot_Patched]
		run combot
	}
	else
	{
		dotnet LSMIPC GithubPatcher.exe VendanAndrews LSMIPC master/Release/LSMIPC.dll "LavishScript Modules" LSMIPC_Patched
	}
}

atom(globalkeep) LSMIPC_Patched(bool updated)
{
	dotnet ComBot GithubPatcher.exe Tehtsuo Combot experimental Scripts/Combot ComBot_Patched
	Event[Github_Patched]:DetachAtom[Github_Patched]
}

atom(globalkeep) Combot_Patched()
{
	echo Launching ComBot
	run combot/combot
	Event[ComBot_Patched]:DetachAtom[ComBot_Patched]
}