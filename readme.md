# LessCoffee

HTTP handlers for ASP.NET web sites that transform *.less and *.coffee files into CSS and JavaScript respectively on the fly.

This implementation is based on [less.js-windows](https://github.com/duncansmart/less.js-windows) and [coffeescript-windows](https://github.com/duncansmart/coffeescript-windows) which run the original JavaScript implementations of LESS and CoffeeScript under Windows Script Host. Therefore this is a **Windows-only** solution and won't run under Mono. There are other implementations of LESS and CoffeeScript for .NET that use pure .NET-based JavaScript compilers so use those if you're wanting to target Mono.

## Install

If you're running Visual Studio 2010 then simply use the [LessCoffee NuGet package](http://nuget.org/List/Packages/LessCoffee).

    PM> Install-Package LessCoffee

If you're using Visual Studio 2008 you'll need follow these manual steps:

* Copy LessCoffee.dll to your web application's /bin directory
* Add the following entries to your web.config file:

```
    <system.web>
        <httpHandlers>
            <add path="*.coffee" type="DotSmart.CoffeeScriptHandler, LessCoffee" verb="*" validate="false"/>
            <add path="*.less" type="DotSmart.LessCssHandler, LessCoffee" verb="*" validate="false"/>
        </httpHandlers>
    </system.web>

    <!-- IIS 7 -->
    <system.webServer>
        <validation validateIntegratedModeConfiguration="false"/>
        <handlers>
            <add path="*.coffee" type="DotSmart.CoffeeScriptHandler, LessCoffee" verb="*" name="DotSmart.CoffeeScriptHandler"/>
            <add path="*.less" type="DotSmart.LessCssHandler, LessCoffee" verb="*" name="DotSmart.LessCssHandler"/>
        </handlers>
    </system.webServer>
```

* If you're using IIS 6 then you will need to map the file extensions *.less and *.coffee to aspnet_isapi.dll
