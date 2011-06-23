<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LessCoffee.TestWeb._Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>LessCoffee test page</title>
    <link rel="stylesheet" href="test1.less" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h1>
            LessCoffee test</h1>
        <p>
            Some quick tests to show LessCoffee working.
        </p>
        <h2>
            LESS</h2>
        <div id="foo">
            <div class="bar">
                This should be colourful thanks to a LESS stylesheet.</div>
        </div>
        <h2>
            CoffeeScript</h2>
        <div id="baz">
        </div>
        <script src="test1.coffee" type="text/javascript"></script>
        <p>
            Run a modified version of the standard <a href="CoffeeScriptTest/test.html">CoffeeScript
                tests.</a>
        </p>
    </div>
    </form>
</body>
</html>
