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
            // look for "@import" and add those to dependencies also
            return parseImports(physicalFileName);
        }

        static IEnumerable<string> parseImports(string lessFileName)
        {
            string dir = Path.GetDirectoryName(lessFileName);

            var importRegex = new Regex(@"@import\s+[""'](.*)[""'];");

            return (from line in File.ReadAllLines(lessFileName)
                    let match = importRegex.Match(line)
                    let file = match.Groups[1].Value
                    where match.Success
                      && !file.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                    select Path.Combine(dir, Path.ChangeExtension(file, ".less"))
            );
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
            TextReader lessStream;

            string lessSrc = File.ReadAllText(lessFilePath);
            lessStream = new StringReader(
                lessPrologue
                + lessSrc
                + lessPostscript
                );

            using (lessStream)
            using (var errors = new StringWriter())
            {
                /*
                    usage: lessc [option option=parameter ...] <source> [destination]

                    If source is set to `-' (dash or hyphen-minus), input is read from stdin.

                    	options:
                        -h, --help              Print help (this message) and exit.
                        --include-path=PATHS    Set include paths. Separated by `:'. Use `;' on Windows.
                        -M, --depends           Output a makefile import dependency list to stdout
                        --no-color              Disable colorized output.
                        --no-ie-compat          Disable IE compatibility checks.
                        -l, --lint              Syntax check only (lint).
                        -s, --silent            Suppress output of error messages.
                        --strict-imports        Force evaluation of imports.
                        --verbose               Be verbose.
                        -v, --version           Print version number and exit.
                        -x, --compress          Compress output by removing some whitespaces.
                        --yui-compress          Compress output using ycssmin
                        --max-line-len=LINELEN  Max line length used by ycssmin
                        -O0, -O1, -O2           Set the parser's optimization level. The lower
                                                the number, the less nodes it will create in the
                                                tree. This could matter for debugging, or if you
                                                want to access the individual nodes in the tree.
                        --line-numbers=TYPE     Outputs filename and line numbers.
                                                TYPE can be either 'comments', which will output
                                                the debug info within comments, 'mediaquery'
                                                that will output the information within a fake
                                                media query which is compatible with the SASS
                                                format, and 'all' which will do both.
                        -rp, --rootpath=URL     Set rootpath for url rewriting in relative imports and urls.
                                                Works with or without the relative-urls option.
                        -ru, --relative-urls    re-write relative urls to the base less file.
                        -sm=on|off              Turn on or off strict math, where in strict mode, math
                        --strict-math=on|off    requires brackets. This option may default to on and then
                                                be removed in the future.
                        -su=on|off              Allow mixed units, e.g. 1px+1em or 1px*1px which have units
                    --strict-units=on|off   that cannot be represented.
                 */

                string args = "\"" + _lessc + "\""
                    + " -" // read from stdin
                    + (compress ? " --clean-css" : "")
                    + " --no-color"
                    + (lineNumbers != null ? " --line-numbers=" + lineNumbers : "");
                int exitCode = ProcessUtil.Exec(NodeExe,
                    args: args,
                    stdIn: lessStream,
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

    }
}
