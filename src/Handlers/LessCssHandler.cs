using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using DotSmart.LessCoffee;
using DotSmart.Properties;
using System.Diagnostics;

namespace DotSmart
{
    public class LessCssHandler : ScriptHandlerBase
    {

        static string _lessc;

        /// <summary>
        /// Initializes a new instance of the LessCssHandler class.
        /// </summary>
        static LessCssHandler()
        {
            _lessc = Path.Combine(TempDirectory, @"node_modules\less\bin\lessc");
        }

        protected override string ContentType
        {
            get { return "text/css"; }
        }

        protected override void Render(string physicalFileName, TextWriter output)
        {
            renderStylesheet(physicalFileName, output);
        }

        protected override IEnumerable<string> GetFileDependencies(string physicalFileName)
        {
            return GetDependencies(physicalFileName);
        }

        internal static bool renderStylesheet(string lessFilePath, TextWriter output)
        {
            try
            {
                string postscript = null;
                if (GetPostscript != null)
                    postscript = GetPostscript.GetInvocationList().Cast<Func<string, string>>().Select(func => func(lessFilePath)).Join(Environment.NewLine);

                RenderCss(lessFilePath, output, 
                    compress: !DebugMode, 
                    lessPostscript: postscript, 
                    lineNumbers: DebugMode ? "comments" : null);
                return true;
            }
            catch (Exception ex)
            {
                output.WriteLine("/* ERROR: " + ex.Message + " */");
                return false;
            }
        }

        /// <summary>
        /// Here you can dynamically append extra bits of LESS script at 
        /// runtime e.g. variable overrides. For example you could grab 
        /// user's favourite colour from database.
        /// </summary>
        public static event Func<string, string> GetPostscript;

        public static void RenderCss(string lessFilePath, TextWriter output, bool compress = true, string lessPrologue = null, string lessPostscript = null, string lineNumbers = null)
        {
            using (var errors = new StringWriter())
            {
                #region lessc usage
                /*
                    usage: lessc [option option=parameter ...] <source> [destination]

                    If source is set to `-' (dash or hyphen-minus), input is read from stdin.

                    options:
                      -h, --help               Print help (this message) and exit.
                      --include-path=PATHS     Set include paths. Separated by `:'. Use `;' on Windows.
                      -M, --depends            Output a makefile import dependency list to stdout
                      --no-color               Disable colorized output.
                      --no-ie-compat           Disable IE compatibility checks.
                      --no-js                  Disable JavaScript in less files
                      -l, --lint               Syntax check only (lint).
                      -s, --silent             Suppress output of error messages.
                      --strict-imports         Force evaluation of imports.
                      --insecure               Allow imports from insecure https hosts.
                      -v, --version            Print version number and exit.
                      -x, --compress           Compress output by removing some whitespaces.
                      --clean-css              Compress output using clean-css
                      --source-map[=FILENAME]  Outputs a v3 sourcemap to the filename (or output filename.map)
                      --source-map-rootpath=X  adds this path onto the sourcemap filename and less file paths
                      --source-map-basepath=X  Sets sourcemap base path, defaults to current working directory.
                      --source-map-less-inline puts the less files into the map instead of referencing them
                      --source-map-map-inline  puts the map (and any less files) into the output css file
                      -rp, --rootpath=URL      Set rootpath for url rewriting in relative imports and urls.
                                               Works with or without the relative-urls option.
                      -ru, --relative-urls     re-write relative urls to the base less file.
                      -sm=on|off               Turn on or off strict math, where in strict mode, math
                      --strict-math=on|off     requires brackets. This option may default to on and then
                                               be removed in the future.
                      -su=on|off               Allow mixed units, e.g. 1px+1em or 1px*1px which have units
                      --strict-units=on|off    that cannot be represented.

                    -------------------------- Deprecated ----------------
                      -O0, -O1, -O2            Set the parser's optimization level. The lower
                                               the number, the less nodes it will create in the
                                               tree. This could matter for debugging, or if you
                                               want to access the individual nodes in the tree.
                      --line-numbers=TYPE      Outputs filename and line numbers.
                                               TYPE can be either 'comments', which will output
                                               the debug info within comments, 'mediaquery'
                                               that will output the information within a fake
                                               media query which is compatible with the SASS
                                               format, and 'all' which will do both.
                      --verbose                Be verbose.

                 */
                #endregion

                string args = "\"" + _lessc + "\"";

                TextReader stdin = null;
                if (!string.IsNullOrEmpty(lessPrologue) || !string.IsNullOrEmpty(lessPostscript))
                {
                    stdin = new StringReader(
                        lessPrologue
                        + File.ReadAllText(lessFilePath)
                        + lessPostscript
                        );
                    args = " -" + args; // read from stdin  -- NOTE: THIS DOESN'T SEEM TO WORK ON AZURE?!?
                }
                else
                {
                    args += " \"" + lessFilePath + "\"";
                }

                args += (compress ? " --clean-css" : "")
                    + " --no-color"
                    + (lineNumbers != null ? " --line-numbers=" + lineNumbers : "");

                int exitCode = ProcessUtil.Exec(NodeExe,
                    args: args,
                    stdIn: stdin,
                    stdOut: output,
                    stdErr: errors,
                    workingDirectory: Path.GetDirectoryName(lessFilePath));
                if (exitCode != 0)
                {
                    throw new ApplicationException(string.Format("Error {0} in '{1}': \r\n{2}",
                                                    exitCode,
                                                    lessFilePath,
                                                    errors.ToString().Trim()));
                }
            }
        }

        /// <summary>
        /// Gets the most recent modified date of the specified *.less file and all of its dependecies
        /// </summary>
        public static DateTime GetLastModified(string lessFilePath)
        {
            var files = GetDependencies(lessFilePath).Concat(new[] { lessFilePath });
            var dates = from f in files select File.GetLastWriteTimeUtc(f);
            return dates.Max();
        }

        /// <summary>
        /// Returns an array of file names representing all of the dependencies of the specified *.less file (not including itself).
        /// </summary>
        public static string[] GetDependencies(string lessFilePath)
        {
            lessFilePath = Path.GetFullPath(lessFilePath);

            using (var output = new StringWriter())
            using (var errors = new StringWriter())
            {
                // We would use 'lessc --depends' but it returns a space-delimited list of filenames 
                // which will be problematic to parse if there are spaces in your filenames...
                var dependsJS = @"
var sys = require('util'),
    fs = require('fs'),
    path = require('path'),
    lessfile = process.argv[1],
    data = fs.readFileSync(lessfile).toString(),
    less = require('./node_modules/less/lib/less');

process.chdir(path.dirname(lessfile));

var parser = new less.Parser();
parser.parse(data, function (err, tree) {
    if (err) {
        less.writeError(err);
        return 1;
    }
    else {
        for (var file in parser.imports.files) {
            sys.puts(path.resolve(file));
        }
        return 0;
    }
});";
                string args = "--eval \"" + dependsJS + "\" \"" + lessFilePath + "\"";
                int exitCode = ProcessUtil.Exec(NodeExe,
                    args: args,
                    stdIn: null,
                    stdOut: output,
                    stdErr: errors,
                    workingDirectory: TempDirectory
                );
                if (exitCode != 0)
                {
                    string errorText = errors.ToString().Trim();
                    if (string.IsNullOrEmpty(errorText))
                        errorText = output.ToString().Trim();
                    throw new ApplicationException(string.Format("Error {0} in '{1}': \r\n{2}",
                                                    exitCode,
                                                    lessFilePath,
                                                    errorText));
                }
                return output.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
