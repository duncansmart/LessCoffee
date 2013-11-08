#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace DotSmart.LessCoffee.Tests
{

    [TestFixture, Explicit]
    class LessTests
    {

        private string _testDir;

        [TestFixtureSetUp]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "LessCoffee-Tests-" + Path.GetRandomFileName());
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
.hello {
    world: @foo;
}";
            Assert.AreEqual(".hello{world:'bar'}", compile(less));
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
.foo {
    .mymixin();
}");

            Assert.AreEqual(".foo{color:red}", result);
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

            Assert.AreEqual("h1{color:red}", compile(less));
        }

        [Test]
        public void CompileBootstrap()
        {
            var output = new StringWriter();
            LessCssHandler.RenderCss(@"..\..\packages\Twitter.Bootstrap.Less.3.0.1\content\Content\bootstrap\bootstrap.less", output);

            Assert.AreEqual(102182, output.ToString().Length);
        }

        [Test]
        public void data_uri_test()
        {
            LessCssHandler.RenderCss(@"..\Tests\data-uri.less", TextWriter.Null);
        }

        [Test]
        public void DependsTest()
        {
            string lessfile = @"..\..\packages\Twitter.Bootstrap.Less.3.0.1\content\Content\bootstrap\bootstrap.less";
            var deps = LessCssHandler.Depends(lessfile);
            Assert.AreEqual(38, deps.Length);
        }

        string compile(string lessSource)
        {
            string filename = writeFile(Path.GetRandomFileName() + ".less", lessSource);
            var output = new StringWriter();
            bool success = LessCssHandler.renderStylesheet(filename, output);
            if (!success)
                throw new ApplicationException("lessc error: " + output);
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
}
#endif
