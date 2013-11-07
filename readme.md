# LessCoffee

HTTP handlers for ASP.NET web sites that transform *.less and *.coffee files into CSS and JavaScript respectively on the fly.

As of version 2.0 the project uses an embedded version of nodejs (embedded as a resource and extracted at runtime) to run the latest versions of the LESS and CoffeeScript (previously it used [less.js-windows](https://github.com/duncansmart/less.js-windows) and [coffeescript-windows](https://github.com/duncansmart/coffeescript-windows)).

## Install

If you're running Visual Studio 2010 then simply use the [LessCoffee NuGet package](http://nuget.org/List/Packages/LessCoffee).

    PM> Install-Package LessCoffee

If you're using Visual Studio 2008 you'll need follow these manual steps:

* Copy LessCoffee.dll to your web application's /bin directory
* Add the following entries to your web.config file:

```
    <system.web>
        <httpHandlers>
            <add path="*.coffee" type="DotSmart.CoffeeScriptHandler, LessCoffee" verb="GET,HEAD" validate="false"/>
            <add path="*.less" type="DotSmart.LessCssHandler, LessCoffee" verb="GET,HEAD" validate="false"/>
        </httpHandlers>
    </system.web>

    <!-- IIS 7 -->
    <system.webServer>
        <validation validateIntegratedModeConfiguration="false"/>
        <handlers>
            <add path="*.coffee" type="DotSmart.CoffeeScriptHandler, LessCoffee" verb="GET,HEAD" name="DotSmart.CoffeeScriptHandler"/>
            <add path="*.less" type="DotSmart.LessCssHandler, LessCoffee" verb="GET,HEAD" name="DotSmart.LessCssHandler"/>
        </handlers>
    </system.webServer>
```

* If you're using IIS 6 then you will need to map the file extensions *.less and *.coffee to aspnet_isapi.dll
