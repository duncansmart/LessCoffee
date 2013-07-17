using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotSmart.Properties;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace DotSmart
{
    public class CoffeeScriptHandler : ScriptHandlerBase, IHttpHandler
    {
        static string _coffee;

        static CoffeeScriptHandler()
        {
            _coffee = Path.Combine(TempDirectory, @"node_modules\coffee-script\bin\coffee");
        }

        protected override string ContentType
        {
            get { return "application/x-javascript"; }
        }

        protected override void Render(string physicalFileName, TextWriter output)
        {
            if (!DebugMode)
            {
                var min = new Microsoft.Ajax.Utilities.Minifier();
                var writer = new StringWriter();
                renderScript(physicalFileName, writer);
                var compressedJs = min.MinifyJavaScript(writer.ToString());
                output.Write(compressedJs);
            }
            else
            {
                renderScript(physicalFileName, output);
            }
        }

        protected override IEnumerable<string> GetFileDependencies(string physicalFileName)
        {
            yield return physicalFileName;
        }

        void renderScript(string scriptFileName, TextWriter output)
        {
            /*
            Usage: coffee [options] path/to/script.coffee -- [args]
            If called without options, `coffee` will run your script.

              -b, --bare         compile without a top-level function wrapper
              -c, --compile      compile to JavaScript and save as .js files
              -e, --eval         pass a string from the command line as input
              -h, --help         display this help message
              -i, --interactive  run an interactive CoffeeScript REPL
              -j, --join         concatenate the source CoffeeScript before compiling
              -m, --map          generate source map and save as .map files
              -n, --nodes        print out the parse tree that the parser produces
                  --nodejs       pass options directly to the "node" binary
              -o, --output       set the output directory for compiled JavaScript
              -p, --print        print out the compiled JavaScript
              -s, --stdio        listen for and compile scripts over stdio
              -l, --literate     treat stdio as literate style coffee-script
              -t, --tokens       print out the tokens that the lexer/rewriter produce
              -v, --version      display the version number
              -w, --watch        watch scripts for changes and rerun commands
            */
            using (var scriptFile = new StreamReader(scriptFileName))
            using (var stdErr = new StringWriter())
            {
                Debug.WriteLine("Compiling " + scriptFileName);
                int exitCode = ProcessUtil.Exec(NodeExe, 
                    "\"" + _coffee + "\" --compile --stdio", 
                    scriptFile, output, stdErr);
                if (exitCode != 0)
                {
                    output.WriteLine("throw \"Error in " + Path.GetFileName(scriptFileName).JsEncode() + ": " 
                        + stdErr.ToString().Trim().JsEncode() + "\";");
                }
            }
        }
    }
}
