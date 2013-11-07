# LessCoffee

HTTP handlers for ASP.NET web sites that transform *.less and *.coffee files into CSS and JavaScript respectively on the fly.

As of version 2.0 the project uses an embedded version of nodejs (embedded as a resource and extracted at runtime) to run the latest versions of the LESS and CoffeeScript (previously it used [less.js-windows](https://github.com/duncansmart/less.js-windows) and [coffeescript-windows](https://github.com/duncansmart/coffeescript-windows)).

## Bundling

LESS has its own native bundling built-in using `@import`. In addition, this allows you to override the included files variables, which is very powerful, e.g.:

**~/Content/main.less**:

```less
@import "bootstrap.less";
@import "mybrand.less";       // defines default @brand-color1 and @brand-color2
@import (less) "chosen.css";  // note how we can transclude CSS

// override brand colors from mybrand.less
@brand-color1: #f00;
@brand-color2: #baa;

// also override Bootstrap ones
@linkColor: #0064cd;
@headingsFontWeight: normal;
@navbarText: #555;
```

**~/Views/_Layout.cshtml**:

```html
<link rel="stylesheet" href="~/Content/main.less" />
```

## Minification

Minification is enabled/disabled by the `system.web/compilation/@debug` attribute in your `Web.config` file. If `debug=true` then no minification is done, otherwise the output is minified.

### Microsoft ASP.NET Web Optimization 

LessCoffee doesn't *currently* support the ASP.NET bundling and minification framework because I find using `@imports` more effective for LESS files at least. `LessCssHandler` does expose a `RenderCss` method that could be adapted if you fancy submitting a pull-request!

## Caching

Control the cachebility of the output using the `LessCoffee` cache profile, e.g.

```xml
<system.web>
    <caching>
        <outputCacheSettings>
            <outputCacheProfiles>
                <!-- 1 year = 31536000 secs -->
                <add name="LessCoffee" duration="31536000" location="Any" varyByParam="*" />
            </outputCacheProfiles>
        </outputCacheSettings>
    </caching>
</system.web>
```    

## Install

If you're running Visual Studio 2010 or later then simply use the [LessCoffee NuGet package](http://nuget.org/List/Packages/LessCoffee).

    PM> Install-Package LessCoffee

If you're using Visual Studio 2008 you'll need follow these manual steps:

* Copy LessCoffee.dll to your web application's /bin directory
* Add the following entries to your web.config file:

```xml
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
