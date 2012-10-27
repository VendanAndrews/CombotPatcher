using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.IO;
using InnerSpaceAPI;
using LavishScriptAPI;
using LavishSettingsAPI;
using LavishVMAPI;
using NDesk.Options;

namespace CombotPatcher
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (InnerSpaceAPI.InnerSpace.BuildNumber == 0)
            {
                return;
            }

            bool showhelp = false;
            bool updateonly = false;
            bool skipupdate = false;
            bool delete = false;
            bool fulldelete = false;
            bool listrepos = false;
            bool deleterepos = false;

            OptionSet p = new OptionSet() {
                { "b|branch=", "Change the active Combot branch",
                    v => {
                    Properties.Settings.Default.CombotBranch = v;
                    Properties.Settings.Default.Save(); } 
                },
                { "u|update-only", "Only perform update",
                    v => { updateonly = v != null;}
                },
                { "s|skip-update", "Skip update",
                    v => { skipupdate = v != null;}
                },
                { "h|help", "Show this message and exit", 
                    v => { showhelp = v != null; } 
                },
                { "d|delete", "Delete all but config files and 3rd party files, and download fresh version",
                    v => { delete = v != null; }
                },
                { "full-delete", "Delete all but config files, and download fresh version",
                    v => { fulldelete = v != null; }
                },
                { "list-repos", "List all active 3rd party repos and exit",
                    v => { listrepos = v != null; }
                },
                { "add-behavior=", "Add 3rd Party Behavior and exit",
                    v => {
                        if (Properties.Settings.Default.BehaviorRepos == null)
                        {
                            Properties.Settings.Default.BehaviorRepos = new System.Collections.Specialized.StringCollection();
                        }
                        Properties.Settings.Default.BehaviorRepos.Add(string.Join("|", v.Split(' ')));
                        Properties.Settings.Default.Save();
                        listrepos = true;
                    }
                },
                { "add-minimode=", "Add 3rd Party MiniMode and exit",
                    v => {
                        if (Properties.Settings.Default.MiniModeRepos == null)
                        {
                            Properties.Settings.Default.MiniModeRepos = new System.Collections.Specialized.StringCollection();
                        }
                        Properties.Settings.Default.MiniModeRepos.Add(string.Join("|", v.Split(' ')));
                        Properties.Settings.Default.Save();
                        listrepos = true;
                    }
                },
                { "rem-behavior=", "Remove 3rd Party Behavior and exit",
                    v => {
                        if (Properties.Settings.Default.BehaviorRepos != null)
                        {
                            foreach(string repo in Properties.Settings.Default.BehaviorRepos)
                            {
                                string[] parts = repo.Split('|');
                                if(parts[0].ToLower() == v.ToLower())
                                {
                                    Properties.Settings.Default.BehaviorRepos.Remove(repo);
                                    if(Directory.Exists(InnerSpace.Path + @"\Scripts\combot\thirdparty\behaviors\" + parts[0]))
                                    {
                                        Directory.Delete(InnerSpace.Path + @"\Scripts\combot\thirdparty\behaviors\" + parts[0], true);
                                    }
                                    break;
                                }
                            }
                        }
                        Properties.Settings.Default.Save();
                        listrepos = true;
                    }
                },
                { "rem-minimode=", "Remove 3rd Party MiniMode and exit",
                    v => {
                        if (Properties.Settings.Default.MiniModeRepos != null)
                        {
                            foreach(string repo in Properties.Settings.Default.MiniModeRepos)
                            {
                                string[] parts = repo.Split('|');
                                if(parts[0].ToLower() == v.ToLower())
                                {
                                    Properties.Settings.Default.MiniModeRepos.Remove(repo);
                                    if(Directory.Exists(InnerSpace.Path + @"\Scripts\combot\thirdparty\minimodes\" + parts[0]))
                                    {
                                        Directory.Delete(InnerSpace.Path + @"\Scripts\combot\thirdparty\minimodes\" + parts[0], true);
                                    }
                                    break;
                                }
                            }
                        }
                        Properties.Settings.Default.Save();
                        listrepos = true;
                    }
                },
                {"rem-repos", "Delete all 3rd party repos",
                    v => { deleterepos = v != null; }
                }
            };

            List<string> extra = p.Parse(args);

            if (showhelp)
            {
                StringWriter desc = new StringWriter();
                p.WriteOptionDescriptions(desc);
                InnerSpace.Echo("Usage: run combot [-b <branch>] [-u|-s] [<char name>]");
                InnerSpace.Echo("       run combot -h");
                InnerSpace.Echo(desc.GetStringBuilder().ToString());
                return;
            }

            if (deleterepos)
            {
                Properties.Settings.Default.BehaviorRepos.Clear();
                Properties.Settings.Default.MiniModeRepos.Clear();
                Properties.Settings.Default.Save();
                listrepos = true;
            }

            if (listrepos)
            {
                InnerSpace.Echo("3rd Party Behaviors:");
                if (Properties.Settings.Default.BehaviorRepos != null)
                {
                    foreach (string repo in Properties.Settings.Default.BehaviorRepos)
                    {
                        string[] parts = repo.Split('|');
                        InnerSpace.Echo(string.Format("Name: {0}        Repo: {1} {2}", parts[0], parts[1], parts[2]));
                    }
                }
                InnerSpace.Echo("3rd Party MiniModes:");
                if (Properties.Settings.Default.MiniModeRepos != null)
                {
                    foreach (string repo in Properties.Settings.Default.MiniModeRepos)
                    {
                        string[] parts = repo.Split('|');
                        InnerSpace.Echo(string.Format("Name: {0}        Repo: {1} {2}", parts[0], parts[1], parts[2]));
                    }
                }
                return;
            }


            if (!skipupdate)
            {
                try
                {
                    GithubPatcher.Patch("CombotPatcher", "master/bin/Release/CombotPatcher.exe", ".Net Programs");
                }
                catch (WebException ex)
                {
                    InnerSpace.Echo("Error: " + ex.Message);
                }
                try
                {
                    GithubPatcher.Patch("CombotPatcher", "master/ComBot.iss", "Scripts");
                }
                catch (WebException ex)
                {
                    InnerSpace.Echo("Error: " + ex.Message);
                }
                try
                {
                    GithubPatcher.Patch("LSMIPC", "master/Release/LSMIPC.dll", "LavishScript Modules");
                }
                catch (WebException ex)
                {
                    InnerSpace.Echo("Error: " + ex.Message);
                }

                if (delete || fulldelete)
                {
                    foreach(string dir in Directory.GetDirectories(InnerSpace.Path + @"\Scripts\combot\", "*", SearchOption.TopDirectoryOnly))
                    {
                        if(dir.Substring(dir.Length - 6).ToLower() != "config" && dir.Substring(dir.Length - 10).ToLower() != "thirdparty")
                        {
                            Directory.Delete(dir, true);
                        }
                        if (dir.Substring(dir.Length - 10).ToLower() == "thirdparty" && fulldelete)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                    foreach (string file in Directory.GetFiles(InnerSpace.Path + @"\Scripts\combot\", "*", SearchOption.TopDirectoryOnly))
                    {
                        File.Delete(file);
                    }
                }

                try
                {
                    GithubPatcher.Patch("Combot", Properties.Settings.Default.CombotBranch, @"Scripts\combot");
                }
                catch (InvalidBranchException ex)
                {
                    InnerSpace.Echo("Invalid Branch");
                    InnerSpace.Echo("Combot NOT UPDATED");
                }
                catch (WebException ex)
                {
                    InnerSpace.Echo("Error: " + ex.Message);
                }

                Directory.CreateDirectory(InnerSpace.Path + @"\Scripts\combot\thirdparty\behaviors\");
                Directory.CreateDirectory(InnerSpace.Path + @"\Scripts\combot\thirdparty\minimodes\");

                if (Properties.Settings.Default.BehaviorRepos != null)
                {
                    foreach (string repo in Properties.Settings.Default.BehaviorRepos)
                    {
                        string[] parts = repo.Split('|');
                        try
                        {
                            GithubPatcher.Patch(parts[1], parts[2], @"Scripts\combot\thirdparty\behaviors\" + parts[0]);
                        }
                        catch (WebException ex)
                        {
                            InnerSpace.Echo("Error " + ex.Message);
                        }
                    }
                }

                if (Properties.Settings.Default.MiniModeRepos != null)
                {
                    foreach (string repo in Properties.Settings.Default.MiniModeRepos)
                    {
                        string[] parts = repo.Split('|');
                        try
                        {
                            GithubPatcher.Patch(parts[1], parts[2], @"Scripts\combot\thirdparty\minimodes\" + parts[0]);
                        }
                        catch (WebException ex)
                        {
                            InnerSpace.Echo("Error: " + ex.Message);
                        }
                    }
                }

                BuildIncludeFile(@"Scripts\combot\behavior\", @"..\behavior\", @"Scripts\combot\temp\behaviorincludes.iss");
                BuildIncludeFile(@"Scripts\combot\minimode\", @"..\minimode\", @"Scripts\combot\temp\minimodeincludes.iss");
                BuildDeclareFile(@"Scripts\combot\behavior\", @"Scripts\combot\temp\behaviordeclares.iss");
                BuildDeclareFile(@"Scripts\combot\minimode\", @"Scripts\combot\temp\minimodedeclares.iss");
                BuildIncludeFile(@"Scripts\combot\thirdparty\behaviors\", @"..\thirdparty\behaviors\", @"Scripts\combot\temp\thirdpartybehaviorincludes.iss");
                BuildIncludeFile(@"Scripts\combot\thirdparty\minimodes\", @"..\thirdparty\minimodes\", @"Scripts\combot\temp\thirdpartyminimodeincludes.iss");
                BuildDeclareFile(@"Scripts\combot\thirdparty\behaviors\", @"Scripts\combot\temp\thirdpartybehaviordeclares.iss");
                BuildDeclareFile(@"Scripts\combot\thirdparty\minimodes\", @"Scripts\combot\temp\thirdpartyminimodedeclares.iss");

                string menuFilePath = InnerSpace.Path + "\\scripts\\init-uplink\\combot-menu.iss";
                string menuContents = @"/*

ComBot  Copyright © 2012  Tehtsuo and Vendan

This file is part of ComBot.

ComBot is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ComBot is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ComBot.  If not, see <http://www.gnu.org/licenses/>.

*/

function main(bool Init=TRUE)
{
	if ${Init}
	{
		TimedCommand 20 run init-uplink/combot-menu.iss FALSE
		return
	}
	if ${ISMenu.Children(exists)}
	{
		ISMenu:AddSeparator[""ISBoxer-Combot""]
	}
	if ${ISMenu.FindChild[""Update ComBot""](exists)}
	{
		ISMenu.FindChild[""Update ComBot""]:SetCommand[""run combot -u""]
	}
	else
	{
		ISMenu:AddCommand[""Update ComBot"", ""run combot -u""]
	}
	if ${ISMenu.FindChild[""ComBot Tools""](exists)}
	{
		ISMenu.FindChild[""ComBot Tools""]:Clear
	}
	else
	{
		ISMenu:AddSubMenu[""ComBot Tools""]
	}
	
	ISMenu.FindChild[""ComBot Tools""]:AddCommand[""Clean ComBot Install"", ""run combot -du""]
	ISMenu.FindChild[""ComBot Tools""]:AddSubMenu[""Switch Branch""]
";
                string menuFinal = @"
}
";
                string menuItem = @"    ISMenu.FindChild[""ComBot Tools""].FindChild[""Switch Branch""]:AddCommand[""{0}"", ""run combot -ub {1}"", {2}]
";
                string[] branches = { "public", "experimental" };
                string[] branchNames = { "Public", "Experimental" };
                string menuItems = "";

                for (int l1 = 0; l1 < branches.Length; l1++)
                {
                    menuItems += String.Format(menuItem, branchNames[l1], branches[l1], ((branches[l1] == Properties.Settings.Default.CombotBranch) ? "TRUE" : "FALSE"));
                }

                if (!branches.Contains(Properties.Settings.Default.CombotBranch))
                {
                    menuItems += String.Format(menuItem, Properties.Settings.Default.CombotBranch, Properties.Settings.Default.CombotBranch, "TRUE");
                }

                StreamWriter menuFile;
                using (menuFile = new StreamWriter(File.Open(menuFilePath, FileMode.Create)))
                {
                    menuFile.Write(menuContents);
                    menuFile.Write(menuItems);
                    menuFile.Write(menuFinal);
                }
                Frame.Lock();
                if (LavishScript.Objects.GetObject("Session") != null)
                {
                    LavishScript.ExecuteCommand("uplink run init-uplink/combot-menu.iss FALSE");
                }
                else
                {
                    LavishScript.ExecuteCommand("run init-uplink/combot-menu.iss FALSE");
                }
                Frame.Unlock();
            }
            string arg = " \"" + string.Join("\" \"", extra.ToArray()) + "\"";
            if (arg == " \"\"")
                arg = "";



            if (!updateonly)
            {
                InnerSpace.Echo(String.Format("Running Combot Branch {0}", Properties.Settings.Default.CombotBranch));
                LavishScript.ExecuteCommand("run combot/combot.iss" + arg);
            }

        }

        public static void BuildIncludeFile(string path, string relative, string file)
        {
            if (!Path.IsPathRooted(path))
            {
                path = InnerSpace.Path + "\\" + path;
            }
            if (!Path.IsPathRooted(file))
            {
                file = InnerSpace.Path + "\\" + file;
            }
            StreamWriter includefile;
            if (Directory.Exists(Path.GetDirectoryName(file)))
            {
                using (includefile = new StreamWriter(File.Open(file, FileMode.Create)))
                {
                    foreach (string filename in Directory.GetFiles(path, "*.iss", SearchOption.AllDirectories))
                    {
                        string relfilename = filename.Replace(path, "");
                        includefile.WriteLine("#include \"{0}{1}\"", relative, relfilename);
                    }
                }
            }
        }

        public static void BuildDeclareFile(string path, string file)
        {
            if (!Path.IsPathRooted(path))
            {
                path = InnerSpace.Path + "\\" + path;
            }
            if (!Path.IsPathRooted(file))
            {
                file = InnerSpace.Path + "\\" + file;
            }
            StreamWriter includefile;
            if (Directory.Exists(Path.GetDirectoryName(file)))
            {
                using (includefile = new StreamWriter(File.Open(file, FileMode.Create)))
                {
                    foreach (string filename in Directory.GetFiles(path, "*.iss", SearchOption.AllDirectories))
                    {
                        string objectName = Path.GetFileNameWithoutExtension(filename);

                        includefile.WriteLine("declarevariable {0} obj_{0} script", objectName);
                    }
                }
            }
        }
    }
}
