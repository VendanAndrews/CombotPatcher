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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CombotPatcher
{
    class GithubPatcher
    {
        static WebClient GitHubClient;
        static WebClient GitHubClientFiles;
        static ShaTree GitHubShaTree;
        static Boolean Updated;
        public static string patchurl = "http://combot.vendaria.net/gitapi";

        public static String Patch(string repo, string tag, string path, string filekey = "")
        {
            Queue<String> repoPaths;
            Updated = false;

            if (!Path.IsPathRooted(path))
            {
                path = InnerSpace.Path + "\\" + path;
            }

            if (File.Exists(path + "\\" + repo + "-ShaTree" + filekey + ".JSON"))
            {
                GitHubShaTree = JsonConvert.DeserializeObject<ShaTree>(File.ReadAllText(path + "\\" + repo + "-ShaTree" + filekey + ".JSON"));
            }
            
            if(GitHubShaTree == null)
            {
                GitHubShaTree = new ShaTree();
            }

            

            if (tag.IndexOf('/') > 0)
            {
                repoPaths = new Queue<string>(tag.Split(new char[] { '/' }));
                tag = repoPaths.Dequeue();
            }
            else
            {
                repoPaths = new Queue<string>();
            }

            GitHubClient = new WebClient();

            GitHubClientFiles = new WebClient();

            String GitHubData = "";
            JObject GitHubJSON;
            String GitHubURL;
            String GitHubSha;

            InnerSpace.Echo(String.Format("Updating {0} {1} in directory {2}", repo, tag, path));

            GitHubClient.Headers.Add("Accept: application/vnd.github.v3+json");
            try
            {
                GitHubData = GitHubClient.DownloadString(String.Format(patchurl + "/GetTree.php?repo={0}&branch={1}", repo, tag));
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new InvalidBranchException();
                    }
                }
                throw ex;
            }

            GitHubJSON = JObject.Parse(GitHubData);

            JToken Tree = GitHubJSON["tree"];

            GitHubSha = (string)GitHubJSON["sha"];

            foreach (String repoPath in repoPaths)
            {
                foreach (JProperty fileProp in Tree.Values<JProperty>())
                {
                    JToken file = fileProp.Value;
                    if ((String)file["path"] == repoPath)
                    {
                        if ((String)file["type"] == "tree")
                        {
                            Tree = file["tree"];
                        }
                        else
                        {
                            if (GitHubShaTree.TreeSha != (String)file["sha"])
                            {
                                UpdateFile(path + "\\" + (String)file["path"], repo, (String)file["sha"]);
                                InnerSpace.Echo((String)file["path"] + " Downloaded");
                                GitHubShaTree.TreeSha = (String)file["sha"];
                            }
                            GitHubSha = GitHubShaTree.TreeSha;
                        }
                        break;
                    }
                }
            }

            if (GitHubShaTree.TreeSha != GitHubSha)
            {
                GitHubShaTree.TreeSha = GitHubSha;
                RecursiveTree(path, Tree, GitHubShaTree, repo);
            }

            File.WriteAllText(path + "\\" + repo + "-ShaTree" + filekey + ".JSON", JsonConvert.SerializeObject(GitHubShaTree));
            InnerSpace.Echo(String.Format("{0} {1} Updated in directory {2}", repo, tag, path));
            return GitHubShaTree.TreeSha;
        }

        static void RecursiveTree(String path, JToken Tree, ShaTree ThisShaTree, String repo)
        {

            Directory.CreateDirectory(path);
            foreach (JProperty fileProp in Tree.Values<JProperty>())
            {
                JToken file = fileProp.Value;
                if ((String)file["type"] == "tree")
                {
                    if (!ThisShaTree.SubTrees.ContainsKey((String)file["path"]))
                    {
                        ThisShaTree.SubTrees.Add((String)file["path"], new ShaTree());
                    }
                    RecursiveTree(path + "\\" + (String)file["path"], file["tree"], ThisShaTree.SubTrees[(String)file["path"]], repo);
                }
                else
                {
                    if (((String)file["path"])[0] != '.')
                    {
                        if (!ThisShaTree.FileShas.ContainsKey((String)file["path"]))
                        {
                            ThisShaTree.FileShas.Add((String)file["path"], "");
                        }
                        if (ThisShaTree.FileShas[(String)file["path"]] != (String)file["sha"])
                        {
                            ThisShaTree.FileShas[(String)file["path"]] = (String)file["sha"];
                            UpdateFile(path + "\\" + (String)file["path"], repo, (String)file["sha"]);
                            InnerSpace.Echo((String)file["path"] + " Downloaded");
                        }
                    }
                }
            }
        }

        static void UpdateFile(String name, String repo, String sha)
        {
            if (File.Exists(name))
            {
                try
                {
                    File.Delete(name);
                }
                catch (UnauthorizedAccessException e)
                {
                    if (File.Exists(name + ".old"))
                    {
                        File.Delete(name + ".old");
                    }
                    File.Move(name, name + ".old");
                }
            }
            GitHubClientFiles.DownloadFile(String.Format(patchurl + "/GetBlob.php?repo={0}&sha={1}", repo, sha), name);
            Updated = true;
        }
    }

    class ShaTree
    {
        public Dictionary<String, String> FileShas = new Dictionary<string, string>();
        public Dictionary<String, ShaTree> SubTrees = new Dictionary<string, ShaTree>();
        public String TreeSha;
    }

    class InvalidBranchException : ApplicationException
    {
        public InvalidBranchException() : base("Invalid Branch")
        {
        }
    }
}
