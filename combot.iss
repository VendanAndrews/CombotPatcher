variable queue:string states
variable bool Updated = FALSE

objectdef treeref
{
	variable string path
	variable string url
	
	method Initialize(string newPath, string newURL)
	{
		path:Set[${newPath.Escape}]
		url:Set[${newURL.Escape}]
	}
}

objectdef obj_ShaTree
{
	variable filepath CONFIG_PATH = "${Script.CurrentDirectory}/combot/config"
	variable string CONFIG_FILE = "ShaTree.xml"
	variable settingsetref BaseRef

	method Initialize()
	{
		LavishSettings[ShaTree]:Clear
		LavishSettings:AddSet[ShaTree]


		if !${CONFIG_PATH.FileExists["${CONFIG_PATH}/${CONFIG_FILE}"]}
		{
			UI:Update["obj_Configuration", "Configuration file is ${CONFIG_FILE}", "g"]
			LavishSettings[ShaTree]:Import["${CONFIG_PATH}/${CONFIG_FILE}"]
		}

		BaseRef:Set[${LavishSettings[ShaTree]}]
	}

	method Shutdown()
	{
		This:Save[]
		LavishSettings[ShaTree]:Clear
	}

	method Save()
	{
		LavishSettings[ShaTree]:Export["${CONFIG_PATH}/${CONFIG_FILE}"]
	}
	
}

variable queue:treeref Trees
variable queue:treeref Files
variable obj_ShaTree ShaTree

function main()
{
	module -require LSMHttp
	echo Loaded
	Event["LSMHttp_ReceiveJSON"]:AttachAtom[GetRef]
	states:Queue["GetRef"]
	states:Queue["GetCommit"]
	states:Queue["GetTree"]
	HTTPSGetJSON https://api.github.com/repos/Tehtsuo/Combot/git/refs/heads/experimental
	while !${Updated}
	{
		wait 10
	}
	ShaTree:Save
	run combot/combot
}

atom(script) GetRef(JSONNode ref)
{
	if ${ref[object].Node[sha].Value.Equal[${ShaTree.BaseRef.FindSetting[CommitSha]}]}
	{
		Updated:Set[TRUE]
	}
	else
	{
		ShaTree.BaseRef:AddSetting[CommitSha, "${ref[object].Node[sha].Value}"]
		NextState
		HTTPSGetJSON ${ref[object].Node[url].Value}
	}
}

atom(script) GetCommit(JSONNode commit)
{
	NextState
	Trees:Queue["combot", "${commit[tree].Node[url].Value.Escape}"]
	mkdir ${Trees.Peek.path.Escape}
	HTTPSGetJSON ${commit[tree].Node[url].Value}
}

atom(script) GetTree(JSONNode tree)
{
	variable string location = ${Trees.Peek.path.Escape}
	variable int treepos = 1
	echo "${body.Escape}"
	variable int maxtreepos = ${tree[tree].Length}
	echo ${tree[tree].Length}
	for(${treepos} <= ${maxtreepos}; treepos:Inc)
	{
		if ${tree[tree].Node[${treepos}].Node[type].Value.Equal[blob]}
		{
			Files:Queue[${String["${location}/${tree[tree].Node[${treepos}].Node[path].Value}"].Escape}, "${tree[tree].Node[${treepos}].Node[url].Value.Escape}"]
			echo "${location}/${tree[tree].Node[${treepos}].Node[path].Value}" Queued
		}
		if ${tree[tree].Node[${treepos}].Node[type].Value.Equal[tree]}
		{
			mkdir ${String["${location}/${tree[tree].Node[${treepos}].Node[path].Value}"].Escape}
			Trees:Queue[${String["${location}/${tree[tree].Node[${treepos}].Node[path].Value}"].Escape}, "${tree[tree].Node[${treepos}].Node[url].Value.Escape}"]
		}
	}
	Trees:Dequeue
	if ${Trees.Used} == 0
	{
		Event[LSMHttp_ReceiveJSON]:DetachAtom[${states.Peek}]
		Event[LSMHttp_Download]:AttachAtom[GetFile]
		HTTPSGetToFile ${Files.Peek.url} "${Files.Peek.path.Escape}"
	}
	else
	{
		HTTPSGetJSON ${Trees.Peek.url}
	}
}

atom(script) GetFile(string filename)
{
	echo ${filename.Escape} Downloaded
	Files:Dequeue
	if ${Files.Used}==0
	{
		Event[LSMHttp_Download]:DetachAtom[GetFile]
		Updated:Set[TRUE]
	}
	else
	{
		HTTPSGetToFile ${Files.Peek.url} "${Files.Peek.path.Escape}"
	}
}

atom(script) NextState()
{
	Event[LSMHttp_ReceiveJSON]:DetachAtom[${states.Peek}]
	states:Dequeue
	Event[LSMHttp_ReceiveJSON]:AttachAtom[${states.Peek}]
}