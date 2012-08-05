using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace DotSmart.LessCoffee.Tests
{
#if DEBUG

    [TestFixture, Explicit]
    class LessTests
    {

        private string _testDir;

        [TestFixtureSetUp]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "LessCoffee Tests " + Path.GetRandomFileName());
            Directory.CreateDirectory(_testDir);
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            Directory.Delete(_testDir, true);
        }

        [Test]
        public void BasicTest()
        {
            string less = @"
@foo: 'bar';
#hello {
    world: @foo;
}";
            Assert.AreEqual("#hello{world:'bar'}", compile(less));
        }

        [Test]
        public void ImportTest()
        {
            writeFile("mixins1.less", @"
.mymixin(){
    color: red;
}");

            var result = compile(@"
@import 'mixins1.less';
#foo {
    .mymixin();
}");

            Assert.AreEqual("#foo{color:red}", result);
        }

        [Test]
        public void PostscriptTest()
        {
            string less = @"
@logoColor: #b00;
h1 {
    color: @logoColor;
}";
            // override a variable name
            LessCssHandler.GetPostscript += (filename) => "@logoColor: #f00;";

            Assert.AreEqual("h1{color:#f00}", compile(less));
        }


        string compile(string lessSource)
        {
            string filename = writeFile(Path.GetRandomFileName() + ".less", lessSource);
            var output = new StringWriter();
            LessCssHandler.renderStylesheet(filename, output);
            string outputCss = output.ToString().Trim();
            return outputCss;
        }

        string writeFile(string name, string text)
        {
            string lessFile = Path.Combine(_testDir, name);
            File.WriteAllText(lessFile, text);
            return lessFile;
        }
    }
#endif
}
