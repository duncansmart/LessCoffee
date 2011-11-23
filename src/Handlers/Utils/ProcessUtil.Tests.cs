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
                int exitCode = executeJScript(_jscriptPath, null, stdin, stdout, stderr);
                Assert.AreEqual(0, exitCode);
                Assert.AreEqual(inputString, stdout.ToString().TrimEnd());
            }
        }

        int executeJScript(string scriptPath, string args, TextReader stdin, TextWriter stdout, TextWriter stderr)
        {
            int exitCode = ProcessUtil.Exec("cscript.exe", "//B //U //E:JScript //nologo \"" + scriptPath + "\" " + args, stdin, stdout, stderr, Encoding.Unicode);
            return exitCode;
        }

	}
}
#endif
