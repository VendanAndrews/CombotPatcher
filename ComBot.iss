objectdef package
{
	variable string Name
	variable string Path
	variable string User
	variable string Ref
	method Initialize(string argName, string argPath, string argUser, string argRef)
	{
		Name:Set[${argName.Escape}]
		Path:Set[${argPath.Escape}]
		User:Set[${argUser.Escape}]
		Ref:Set[${argRef.Escape}]
	}
}

variable(globalkeep) queue:string Packages

function main()
{
	Packages:Clear
	Packages:Queue["GithubPatcher", ".NET Programs", "VendanAndrews", "master/bin/Release"]
	Packages:Queue["CombotPatcher", "Scripts", "VendanAndrews", "master/ComBot.iss"]
	Packages:Queue["LSMIPC", "LavishScript Modules", "VendanAndrews", "master/bin/Release"]
	Packages:Queue["GithubPatcher", "Scripts/Combot", "Tehtsuo", "experimental"]
	LavishScript:RegisterEvent[Patched]
	Event[Patched]:AttachAtom[Patched]
	dotnet ${Packages.Peek.Name} GithubPatcher.exe ${Packages.Peek.User} ${Packages.Peek.Name} "${Packages.Peek.Ref.Escape}" "${Packages.Peek.Path.Escape}" Patched
}

atom(globalkeep) Patched(bool updated)
{
	if ${Packages.Used} > 1
	{
		if ${updated}
		{
			Event[Patched]:DetachAtom[Patched]
			run combot
		}
		else
		{
			Packages:Dequeue
			dotnet ${Packages.Peek.Name} GithubPatcher.exe ${Packages.Peek.User} ${Packages.Peek.Name} "${Packages.Peek.Ref.Escape}" "${Packages.Peek.Path.Escape}" Patched
		}
	}
	else
	{
		echo Launching ComBot
		Event[Patched]:DetachAtom[Patched]
		run combot/combot
	}
}