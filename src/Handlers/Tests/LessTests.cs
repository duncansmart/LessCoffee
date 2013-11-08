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

            // yeah a bit lame :-)
            Assert.AreEqual(102182, output.ToString().Length);
        }

        [Test]
        public void data_uri_test()
        {
            Assert.DoesNotThrow(() =>
            {
                LessCssHandler.RenderCss(@"..\Tests\data-uri.less", TextWriter.Null);
            });
        }

        [Test]
        public void DependsTest()
        {
            writeFile("dep-main.less", "@import 'deptest-1';");
            writeFile("deptest-1.less", "@import 'deptest/2';");
            writeFile("./deptest/2.less", "foo {bar:baz}");

            var dependencies = LessCssHandler.GetDependencies(Path.Combine(_testDir, "dep-main.less"));
            var relativePaths = dependencies.Select(f => f.Substring(_testDir.Length + 1));

            Assert.AreEqual(new[] { @"deptest-1.less", @"deptest\2.less" }, relativePaths);
        }

        [Test]
        public void LastModifiedTest()
        {
            var date1 = new DateTime(2012, 1, 1);
            var date2 = new DateTime(2013, 1, 1);
            var date3 = new DateTime(2014, 1, 1);

            writeFile("lm-main.less", "@import 'lm-dep1';", date1);
            writeFile("lm-dep1.less", "@import 'lm-dep2';", date2);
            writeFile("lm-dep2.less", "foo {bar:baz}", date3);

            var lmdate = LessCssHandler.GetLastModified(Path.Combine(_testDir, "lm-main.less"));

            Assert.AreEqual(date3, lmdate);
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

        string writeFile(string name, string text, DateTime? lastWriteTime = null)
        {
            string lessFile = Path.GetFullPath(Path.Combine(_testDir, name));
            Directory.CreateDirectory(Path.GetDirectoryName(lessFile));
            File.WriteAllText(lessFile, text);

            if (lastWriteTime != null)
                File.SetLastWriteTime(lessFile, lastWriteTime.Value);

            return lessFile;
        }
    }
}
#endif
