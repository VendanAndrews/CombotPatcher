function main()
{
	LavishScript:RegisterEvent[Combot_Patched]
	Event[Combot_Patched]:AttachAtom[Combot_Patched]
	dotnet ComBot GithubPatcher.exe Tehtsuo Combot experimental Scripts/ComBot Combot_Patched
}

atom(globalkeep) Combot_Patched()
{
	echo Launching ComBot
	run combot/combot
}