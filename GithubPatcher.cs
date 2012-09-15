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

        public static void Patch(string user, string repo, string tag, string path)
        {
            Queue<String> repoPaths;
            Updated = false;

            if (!Path.IsPathRooted(path))
            {
                path = InnerSpace.Path + "\\" + path;
            }

            if (File.Exists(path + "\\ShaTree.JSON"))
            {
                GitHubShaTree = JsonConvert.DeserializeObject<ShaTree>(File.ReadAllText(path + "\\ShaTree.JSON"));
            }
            else
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

            InnerSpace.Echo(String.Format("Updating {0} {1} {2} in directory {3}", user, repo, tag, path));

            GitHubClient.Headers.Add("Accept: application/vnd.github.v3+json");
            try
            {
                GitHubData = GitHubClient.DownloadString(String.Format("https://api.github.com/repos/{0}/{1}/git/refs/heads/{2}", user, repo, tag));
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

            GitHubURL = (String)GitHubJSON["object"]["url"];

            GitHubClient.Headers.Add("Accept: application/vnd.github.v3+json");
            GitHubData = GitHubClient.DownloadString(GitHubURL);

            GitHubJSON = JObject.Parse(GitHubData);

            GitHubURL = (String)GitHubJSON["tree"]["url"];
            GitHubSha = (String)GitHubJSON["tree"]["sha"];

            foreach (String repoPath in repoPaths)
            {
                GitHubClient.Headers.Add("Accept: application/vnd.github.v3+json");
                GitHubData = GitHubClient.DownloadString(GitHubURL);
                GitHubJSON = JObject.Parse(GitHubData);
                foreach (JToken file in GitHubJSON["tree"])
                {
                    if ((String)file["path"] == repoPath)
                    {
                        if ((String)file["type"] == "tree")
                        {
                            GitHubURL = (String)file["url"];
                            GitHubSha = (String)file["sha"];
                        }
                        else
                        {
                            if (GitHubShaTree.TreeSha != (String)file["sha"])
                            {
                                UpdateFile(path + "\\" + (String)file["path"], (String)file["url"]);
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
                RecursiveTree(path, GitHubURL, GitHubShaTree);
            }

            File.WriteAllText(path + "\\ShaTree.JSON", JsonConvert.SerializeObject(GitHubShaTree));
            InnerSpace.Echo(String.Format("{0} {1} {2} Updated in directory {3}", user, repo, tag, path));
        }

        static void RecursiveTree(String path, String url, ShaTree ThisShaTree)
        {
            String TreeData;
            JObject TreeJSON;

            Directory.CreateDirectory(path);

            GitHubClient.Headers.Add("Accept: application/vnd.github.v3+json");
            TreeData = GitHubClient.DownloadString(url);
            TreeJSON = JObject.Parse(TreeData);
            foreach (JToken file in TreeJSON["tree"])
            {
                if ((String)file["type"] == "tree")
                {
                    if (!ThisShaTree.SubTrees.ContainsKey((String)file["path"]))
                    {
                        ThisShaTree.SubTrees.Add((String)file["path"], new ShaTree());
                    }
                    if (ThisShaTree.SubTrees[(String)file["path"]].TreeSha != (String)file["sha"])
                    {
                        ThisShaTree.SubTrees[(String)file["path"]].TreeSha = (String)file["sha"];
                        RecursiveTree(path + "\\" + (String)file["path"], (String)file["url"], ThisShaTree.SubTrees[(String)file["path"]]);
                    }
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
                            UpdateFile(path + "\\" + (String)file["path"], (String)file["url"]);
                            InnerSpace.Echo((String)file["path"] + " Downloaded");
                        }
                    }
                }
            }
        }

        static void UpdateFile(String name, String url)
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
            GitHubClientFiles.Headers.Add("Accept: application/vnd.github.v3.raw");
            GitHubClientFiles.DownloadFile(url, name);
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
