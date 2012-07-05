using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Configuration;
using System.Diagnostics;
using DotSmart.Properties;
using System.Web.UI;
using System.Collections.Generic;
using System.Web.Caching;

namespace DotSmart
{
    public abstract class ScriptHandlerBase : IHttpHandler
    {
        private const string CACHE_PROFILE_NAME = "LessCoffee";
        protected static DateTime CompileDate = File.GetLastWriteTime(typeof(CoffeeScriptHandler).Assembly.Location);

        protected static string NodeExe;

        static string _tempDirectory;

        static ScriptHandlerBase()
        {
            extractNodeJs();
            NodeExe = Path.Combine(TempDirectory, @"node.exe");
        }

        protected abstract void Render(string physicalFileName, TextWriter output);

        protected abstract string ContentType { get; }

        protected virtual IEnumerable<string> GetFileDependencies(string physicalFileName)
        {
            return Enumerable.Empty<string>();
        }

        protected static string TempDirectory
        {
            get
            {
                if (_tempDirectory != null)
                    return _tempDirectory;

                
                _tempDirectory = Path.Combine(HttpRuntime.AppDomainId != null ? HttpRuntime.CodegenDir : Path.GetTempPath(), "LessCoffee");
                if (!Directory.Exists(_tempDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(_tempDirectory);
                    }
                    catch (IOException) {/*another thread got there*/}
                }

                return _tempDirectory;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var physicalFileName = context.Server.MapPath(context.Request.FilePath);

            var dependentFiles = GetFileDependencies(physicalFileName) ?? Enumerable.Empty<string>();
            dependentFiles = dependentFiles.Concat(new[] { physicalFileName }); // ensure this file is included
            context.Response.AddCacheDependency(new CacheDependency(dependentFiles.ToArray()));

            Render(physicalFileName, context.Response.Output);

            // Process with caching ()
            OutputCacheParameters cacheParams = getCacheParams();
            using (OutputCachedPage page = new OutputCachedPage(cacheParams))
            {
                page.ProcessRequest(context);
            }

            // has to be done here as Page seems to overwrite it.
            context.Response.ContentType = ContentType;

            var cache = context.Response.Cache;
            cache.SetETagFromFileDependencies();
            cache.SetLastModifiedFromFileDependencies();
            cache.SetOmitVaryStar(true); // Old IE stops caching if it gets a "Vary: *" header
        }

        static OutputCacheParameters getCacheParams()
        {
            OutputCacheParameters cacheParams;
            if (cacheProfileExists(CACHE_PROFILE_NAME))
            {
                cacheParams = new OutputCacheParameters
                {
                    CacheProfile = CACHE_PROFILE_NAME
                };
            }
            else // default
            {
                cacheParams = new OutputCacheParameters
                {
                    Duration = 86400, // 1 day
                    Enabled = true,
                    Location = OutputCacheLocation.Any,
                    VaryByParam = "*"
                };
            }
            return cacheParams;
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
        static protected bool DebugMode
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

        static bool cacheProfileExists(string cacheProfileName)
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration(null);
            var outputCacheSettings = (OutputCacheSettingsSection)webConfig.GetSection("system.web/caching/outputCacheSettings");
            var profile = outputCacheSettings.OutputCacheProfiles[cacheProfileName];
            return (profile != null);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        // Borrowed from System.Web.Mvc.OutputCacheAttribute+OutputCachedPage
        class OutputCachedPage : System.Web.UI.Page
        {
            // Fields
            OutputCacheParameters _cacheParams;

            // Methods
            public OutputCachedPage(OutputCacheParameters cacheParams)
            {
                this.ID = Guid.NewGuid().ToString();
                this._cacheParams = cacheParams;
            }

            protected override void FrameworkInitialize()
            {
                base.FrameworkInitialize();
                this.InitOutputCache(this._cacheParams);
            }
        }


    }
}
