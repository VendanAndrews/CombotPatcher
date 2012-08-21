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

            OptionSet p = new OptionSet() {
                { "b|branch", v => {
                    Properties.Settings.Default.CombotBranch = v;
                    Properties.Settings.Default.Save(); } },
                { "h|help",  "show this message and exit", 
                    v => { showhelp = true; } },
            };
            
            List<string> extra = p.Parse(args);

            if (showhelp)
            {
                MemoryStream desc = new MemoryStream();
                p.WriteOptionDescriptions(new StreamWriter(desc));
                InnerSpace.Echo(desc.ToString());
            }


            GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/bin/Release/CombotPatcher.exe", ".Net Programs");
            GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/ComBot.iss", "Scripts");
            GithubPatcher.Patch("VendanAndrews", "LSMIPC", "master/Release/LSMIPC.dll", "LavishScript Modules");
            GithubPatcher.Patch("Tehtsuo", "Combot", Properties.Settings.Default.CombotBranch, @"Scripts\combot");

            string arg = " \"" + string.Join("\" \"", extra.ToArray()) + "\"";
            if(arg==" \"\"")
                arg = "";

            LavishScript.ExecuteCommand("run combot/combot.iss" + arg);

        }
    }
}
