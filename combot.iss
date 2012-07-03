function main()
{
	LavishScript:RegisterEvent[Combot_Patched]
	Event[Combot_Patched]:AttachAtom[Combot_Patched]
	dotnet ComBot GithubPatcher.exe Tehtsuo Combot experimental Scripts\\combot Combot_Patched
}

atom(globalkeep) Combot_Patched()
{
	echo Launching Combot
	run combot\\combot
}