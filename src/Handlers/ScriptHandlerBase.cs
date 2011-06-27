using System;
using System.Web;
using System.IO;

namespace DotSmart
{
    public abstract class ScriptHandlerBase
    {
        protected static DateTime CompileDate = File.GetLastWriteTime(typeof(CoffeeScriptHandler).Assembly.Location);

        static object _mutex = new object();
        static string _tempDirectory;

        protected static string TempDirectory
        {
            get
            {
                if (_tempDirectory != null)
                    return _tempDirectory;

                _tempDirectory = Path.Combine(HttpRuntime.CodegenDir, "LessCoffee");
                lock (_mutex)
                {
                    if (!Directory.Exists(_tempDirectory))
                        Directory.CreateDirectory(_tempDirectory);
                }
                return _tempDirectory;
            }
        }

        protected static void ExportResourceIfNewer(string fileName, byte[] resource)
        {
            if (!File.Exists(fileName) || File.GetLastWriteTime(fileName) != CompileDate)
            {
                File.WriteAllBytes(fileName, resource);
                File.SetLastWriteTime(fileName, CompileDate);
            }
        }

        protected void SetCacheability(HttpResponse response, params string[] scriptFileNames)
        {
            response.AddCacheDependency(new System.Web.Caching.CacheDependency(scriptFileNames));

            response.Cache.SetETagFromFileDependencies();
            response.Cache.SetLastModifiedFromFileDependencies();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetExpires(DateTime.Now.AddDays(1));
        }
    }
}
