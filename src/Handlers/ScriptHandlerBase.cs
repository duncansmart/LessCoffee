using System;
using System.Web;
using System.IO;
using System.Web.Configuration;
using System.Diagnostics;
using DotSmart.Properties;

namespace DotSmart
{
    public abstract class ScriptHandlerBase
    {
        protected static DateTime CompileDate = File.GetLastWriteTime(typeof(CoffeeScriptHandler).Assembly.Location);

        protected static string NodeExe;

        static string _tempDirectory;

        static ScriptHandlerBase()
        {
            extractNodeJs();
            NodeExe = Path.Combine(TempDirectory, @"node.exe");
        }

        protected static string TempDirectory
        {
            get
            {
                if (_tempDirectory != null)
                    return _tempDirectory;

                _tempDirectory = Path.Combine(HttpRuntime.CodegenDir, "LessCoffee");
                if (!Directory.Exists(_tempDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(_tempDirectory);
                    }
                    catch (IOException){/*another thread got there*/}
                }

                return _tempDirectory;
            }
        }

        static void extractNodeJs()
        {
            var tgzStream = new MemoryStream(Resources.NodeJsTgz);
            var tarStream = new System.IO.Compression.GZipStream(tgzStream, System.IO.Compression.CompressionMode.Decompress, false);
            var untar = new DotSmart.LessCoffee.UnTar();
            Debug.WriteLine("Untar-ing to " + TempDirectory);
            untar.Extract(tarStream, TempDirectory);
        }

        protected static void ExportResourceIfNewer(string fileName, byte[] resource)
        {
            if (!File.Exists(fileName) || File.GetLastWriteTime(fileName) != CompileDate)
            {
                File.WriteAllBytes(fileName, resource);
                File.SetLastWriteTime(fileName, CompileDate);
            }
        }


        static bool? _debugMode;
        protected bool DebugMode
        {
            get
            {
                if (_debugMode == null)
                {
                    var compilationSection = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                    _debugMode = (compilationSection != null && compilationSection.Debug);
                }
                return _debugMode.Value;
            }
        }

        protected void SetCacheability(HttpResponse response, params string[] scriptFileNames)
        {
            response.AddCacheDependency(new System.Web.Caching.CacheDependency(scriptFileNames));

            response.Cache.SetETagFromFileDependencies();
            response.Cache.SetLastModifiedFromFileDependencies();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetExpires(DateTime.Now.AddDays(1));

            // So we can use cache-busting params on URL
            response.Cache.VaryByParams["*"] = true;

            // Old IE stops caching if it gets a "Vary: *" header
            response.Cache.SetOmitVaryStar(true);
        }
    }
}
