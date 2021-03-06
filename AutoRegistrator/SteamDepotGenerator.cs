﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PlasmaDownloader;
using ZeroKWeb;
using ZkData;

namespace AutoRegistrator
{
    public class SteamDepotGenerator
    {
        string targetFolder;
        private string siteBase;
        public SteamDepotGenerator(string siteBase, string targetFolder) {
            this.targetFolder = targetFolder;
            this.siteBase = siteBase;
        }

        public void Generate() {
            Utils.CheckPath(targetFolder);
            var paths = new SpringPaths(targetFolder, false);
            try
            {
                //Directory.Delete(Path.Combine(paths.WritableDirectory, "pool"), true);
                Directory.Delete(Path.Combine(paths.WritableDirectory, "packages"), true);
                Directory.CreateDirectory(Path.Combine(paths.WritableDirectory, "packages"));
            } catch { }
            

            var downloader = new PlasmaDownloader.PlasmaDownloader(null, paths);
            downloader.GetResource(DownloadType.ENGINE, MiscVar.DefaultEngine)?.WaitHandle.WaitOne(); //for ZKL equivalent, see PlasmaShared/GlobalConst.cs
            downloader.GetResource(DownloadType.RAPID, "zk:stable")?.WaitHandle.WaitOne();
            downloader.GetResource(DownloadType.RAPID, "zk:test")?.WaitHandle.WaitOne();

            CopyResources(siteBase, paths, GetResourceList(downloader.PackageDownloader.GetByTag("zk:stable").InternalName, downloader.PackageDownloader.GetByTag("zk:test").InternalName), downloader);

            CopyLobbyProgram();
            CopyExtraImages();
        }

        private void CopyLobbyProgram() {
            var zklSource = Path.Combine(siteBase, "lobby", "Zero-K.exe");
            if (File.Exists(zklSource)) File.Copy(zklSource, Path.Combine(targetFolder, "Zero-K.exe"), true);
            else new WebClient().DownloadFile(GlobalConst.SelfUpdaterBaseUrl + "/" + "Zero-K.exe", Path.Combine(targetFolder, "Zero-K.exe"));
        }

        private void CopyExtraImages() {
            var db = new ZkDataContext();
            var configs = Path.Combine(targetFolder, "LuaUI", "Configs");
            Utils.CheckPath(configs);

            var tpath = Path.Combine(configs, "Avatars");
            Utils.CheckPath(tpath);
            Trace.TraceInformation("Copying avatars");
            var spath = Path.Combine(siteBase, "img", "Avatars");
            Utils.CheckPath(spath);
            foreach (var file in Directory.GetFiles(spath)) File.Copy(file, Path.Combine(tpath, Path.GetFileName(file)), true);

            Trace.TraceInformation("Copying clan icons");
            tpath = Path.Combine(configs, "Clans");
            Utils.CheckPath(tpath);
            spath = Path.Combine(siteBase, "img", "clans");
            foreach (var clan in db.Clans.Where(x => !x.IsDeleted))
            {
                var fileName = $"{clan.Shortcut}.png";
                var src = Path.Combine(spath, fileName);
                if (File.Exists(src)) File.Copy(src, Path.Combine(tpath, fileName), true);
            }

            Trace.TraceInformation("Copying faction icons");
            tpath = Path.Combine(configs, "Factions");
            Utils.CheckPath(tpath);
            spath = Path.Combine(siteBase, "img", "factions");

            foreach (var fac in db.Factions.Where(x => !x.IsDeleted))
            {
                var fileName = $"{fac.Shortcut}.png";
                var src = Path.Combine(spath, fileName);
                if (File.Exists(src)) File.Copy(src, Path.Combine(tpath, fileName), true);
            }
        }


        private static void CopyResources(string siteBase, SpringPaths paths, List<Resource> resources, PlasmaDownloader.PlasmaDownloader downloader) {
            var destMaps = Path.Combine(paths.WritableDirectory, "maps");
            var sourceMaps = Path.Combine(siteBase, "autoregistrator", "maps");
            var sourceMetadata = Path.Combine(siteBase, "Resources");
            var targetMetadata = Path.Combine(paths.Cache, "Resources");
            if (!Directory.Exists(targetMetadata)) Directory.CreateDirectory(targetMetadata);

            foreach (var res in resources)
            {
                Trace.TraceInformation("Copying {0}", res.InternalName);
                if (res.TypeID == ResourceType.Map)
                {
                    var fileName = res.ResourceContentFiles.ToList().Where(x=>File.Exists(Path.Combine(sourceMaps, x.FileName))).OrderByDescending(x => x.LinkCount).FirstOrDefault()?.FileName; // get registered file names

                    fileName = fileName?? res.ResourceContentFiles
                        .Where(x=>x.Links!=null).SelectMany(x => x.Links.Split('\n'))
                        .Where(x => x != null).Select(x=> x.Substring(x.LastIndexOf('/')+1, x.Length - x.LastIndexOf('/') - 1)).FirstOrDefault(x => !string.IsNullOrEmpty(x) && File.Exists(Path.Combine(sourceMaps,x))); // get filenames from url

                    if (fileName == null)
                    {

                        Trace.TraceError("Cannot find map file: {0}", res.InternalName);
                        continue;
                    }


                    if (!File.Exists(Path.Combine(destMaps, fileName))) File.Copy(Path.Combine(sourceMaps, fileName), Path.Combine(destMaps, fileName));
                } else if (res.MissionID != null) File.WriteAllBytes(Path.Combine(paths.WritableDirectory, "games", res.Mission.SanitizedFileName), res.Mission.Mutator);
                else downloader.GetResource(DownloadType.RAPID, res.InternalName)?.WaitHandle.WaitOne();

                foreach (var metaName in new[] { res.MinimapName, res.HeightmapName, res.MetalmapName, res.MetadataName, res.ThumbnailName })
                {
                    Trace.TraceInformation("Copying resource: {0}", metaName);
                    var src = Path.Combine(sourceMetadata, metaName);
                    var dst = Path.Combine(targetMetadata, metaName);
                    if (!File.Exists(dst) && File.Exists(src))
                    {
                        File.Copy(src, dst);
                    }
                }
            }
        }


        private static List<Resource> GetResourceList(params string[] extraNames) {
            

            var db = new ZkDataContext();
            var resources = db.Resources.Where(x => extraNames.Contains(x.InternalName) || (x.TypeID == ResourceType.Map && x.MapSupportLevel>=MapSupportLevel.Featured) || (x.MissionID != null && !x.Mission.IsDeleted && x.Mission.FeaturedOrder !=null)).ToList();
            foreach (var res in resources.ToList())
            {
                foreach (var requestedDependency in res.ResourceDependencies.Select(x => x.NeedsInternalName))
                {
                    if (!resources.Any(y => y.InternalName == requestedDependency))
                    {
                        var dependency = db.Resources.FirstOrDefault(x => x.InternalName == requestedDependency);
                        if (dependency != null) resources.Add(dependency);
                    }
                }
            }
            return resources;
        }

        public void RunBuild() {
            Trace.TraceInformation("Starting SteamDepot build");
            var pi = new ProcessStartInfo(Path.Combine(targetFolder,"..","builder","steamcmd.exe"), string.Format(@"+login zkbuild {0} +run_app_build_http ..\scripts\app_zk_stable.vdf +quit", new Secrets().GetSteamBuildPassword()));
            pi.UseShellExecute = false;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            var runp = Process.Start(pi);
            runp.WaitForExit();
            Trace.TraceInformation("SteamDepot build completed!");
        }
    }
}
