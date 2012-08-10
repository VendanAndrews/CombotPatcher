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

namespace CombotPatcher
{
    class Program
    {
        public static void Main(string[] args)
        {
            GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/bin/Release/CombotPatcher.exe", ".Net Programs");
            GithubPatcher.Patch("VendanAndrews", "CombotPatcher", "master/ComBot.iss", "Scripts");
            GithubPatcher.Patch("VendanAndrews", "LSMIPC", "master/Release/LSMIPC.dll", "LavishScript Modules");
            GithubPatcher.Patch("Tehtsuo", "Combot", "experimental", @"Scripts\combot");
            LavishScript.ExecuteCommand("run combot/combot.iss");

        }
    }
}
