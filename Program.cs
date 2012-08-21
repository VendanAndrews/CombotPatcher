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
                GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/bin/Release/CombotPatcher.exe", ".Net Programs");
                GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/ComBot.iss", "Scripts");
                GithubPatcher.Patch("VendanAndrews", "LSMIPC", "master/Release/LSMIPC.dll", "LavishScript Modules");
                GithubPatcher.Patch("Tehtsuo", "Combot", Properties.Settings.Default.CombotBranch, @"Scripts\combot");
            }
            string arg = " \"" + string.Join("\" \"", extra.ToArray()) + "\"";
            if(arg==" \"\"")
                arg = "";
            if (!updateonly)
            {
                LavishScript.ExecuteCommand("run combot/combot.iss" + arg);
            }

        }
    }
}
