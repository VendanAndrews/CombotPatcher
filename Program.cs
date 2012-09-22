﻿using System;
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
                { "h|help",  "Show this message and exit", 
                    v => { showhelp = v != null; } 
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


            if (!skipupdate)
            {
                try
                {
                    GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/bin/Release/CombotPatcher.exe", ".Net Programs");
                }
                catch (WebException ex)
                {
                    InnerSpace.Echo("Error: " + ex.Message);
                }
                try
                {
                    GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/ComBot.iss", "Scripts");
                }
                catch (WebException ex)
                {
                    InnerSpace.Echo("Error: " + ex.Message);
                }
                try
                {
                    GithubPatcher.Patch("VendanAndrews", "LSMIPC", "master/Release/LSMIPC.dll", "LavishScript Modules");
                }
                catch (WebException ex)
                {
                    InnerSpace.Echo("Error: " + ex.Message);
                }
                try
                {
                    GithubPatcher.Patch("Tehtsuo", "Combot", Properties.Settings.Default.CombotBranch, @"Scripts\combot");
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

                BuildIncludeFile(@"Scripts\combot\behavior\", @"..\behavior\", @"Scripts\combot\temp\behaviorincludes.iss");
                BuildIncludeFile(@"Scripts\combot\minimode\", @"..\minimode\", @"Scripts\combot\temp\minimodeincludes.iss");
                BuildDeclareFile(@"Scripts\combot\behavior\", @"Scripts\combot\temp\behaviordeclares.iss");
                BuildDeclareFile(@"Scripts\combot\minimode\", @"Scripts\combot\temp\minimodedeclares.iss");

            }
            string arg = " \"" + string.Join("\" \"", extra.ToArray()) + "\"";
            if (arg == " \"\"")
                arg = "";



            if (!updateonly)
            {
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
            using (includefile = new StreamWriter(File.Open(file, FileMode.Create)))
            {
                foreach (string filename in Directory.GetFiles(path, "*.iss", SearchOption.AllDirectories))
                {
                    string relfilename = filename.Replace(path, "");
                    includefile.WriteLine("#include \"{0}{1}\"", relative, relfilename);
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