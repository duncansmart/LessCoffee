#if DEBUG
using System;
using System.Diagnostics;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace DotSmart
{
    [TestFixture]
    public class ProcessUtil_Tests
    {
        string _jscriptPath;

        [TestFixtureSetUp]
        public void Setup()
        {
            _jscriptPath = Path.GetTempFileName();
            File.WriteAllText(_jscriptPath, "WScript.StdOut.Write(WScript.StdIn.ReadAll())");
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            File.Delete(_jscriptPath);
        }

        [TestCase("hello world")]
        [TestCase("¡pʃɹoʍ oʃʃǝH")]
        [TestCase("こんにちは、世界！")]
        public void AssertUnicodeStringsAreCorrectlyHandled(string inputString)
        {
            using (var stdin = new StringReader(inputString))
            using (var stderr = new StringWriter())
            using (var stdout = new StringWriter())
            {
                int exitCode = executeJScript(_jscriptPath, null, stdin, stdout, stderr, Encoding.Unicode);
                Assert.AreEqual(0, exitCode);
                Assert.AreEqual(inputString, stdout.ToString().TrimEnd());
            }
        }

        [Test]
        public void StdErrorIsReadable()
        {
            var jscriptPath = Path.GetTempFileName();
            File.WriteAllText(jscriptPath, "cause_error()");
            Debug.WriteLine(jscriptPath);

            using (var stdin = new StringReader("Hello world!"))
            using (var stderr = new StringWriter())
            using (var stdout = new StringWriter())
            {
                int exitCode = executeJScript(jscriptPath, null, stdin, stdout, stderr, Encoding.Default);
                //Debug.WriteLine("exit: '" + exitCode + "'");
                //Debug.WriteLine("stdout: '" + stdout + "'");
                //Debug.WriteLine("stderr: '" + stderr + "'");
                
                Assert.That(stderr.ToString(), Is.StringContaining("Microsoft JScript runtime error: Object expected"));
            }
            File.Delete(jscriptPath);
        }

        int executeJScript(string scriptPath, string args, TextReader stdin, TextWriter stdout, TextWriter stderr, Encoding encoding)
        {
            args = "//E:JScript //nologo \"" + scriptPath + "\" " + args;

            if (encoding == Encoding.Unicode)
                args = "//U " + args;

            int exitCode = ProcessUtil.Exec("cscript.exe", args, stdin, stdout, stderr, encoding);
            return exitCode;
        }

    }
}
#endif
