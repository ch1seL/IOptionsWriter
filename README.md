# IOptionsWriter

Easy way to write appsettings.json or other configuration json file

## How to use

### Installation

#### Nuget Package Manager Console:

```powershell
Install-Package IOptionsWriter
```

#### .Net CLI:
```powershell
dotnet add package IOptionsWriter
```
### Using

```c#
IServiceCollection.ConfigureWritable<TOptions>(string sectionName = null, string settingsFile = "appsettings.json", bool reloadAfterWrite = false)
```

TOptions - type of your settings

sectionName - your configuration section name(default: TOptions type name)

settingsFile - default appsettings.json

reloadAfterWrite - you can enable this option if you have problems detecting file changes or if add the settings file with false value of reloadOnChange parameter (builder.AddJsonFile(appsettings.json, optional: false, reloadOnChange: false);)

### Simple sample

appsettings.json:

```JSON
{
    "MySettings": {
        "MyOption": "MyValue"
    }
}
```

Startup.cs:

```c#
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddOptions()
        .ConfigureWritable<MySettings>();
}
```

Controllers/MyController.cs:

```c#
[Controller]
public class MyController : Controller {
    private readonly IOptionsWritable<MySettings> _myOptionsAccessor;

    public MyController(IOptionsWritable<MySettings> myOptionsAccessor) {
        _myOptionsAccessor = myOptionsAccessor;
    }

    [HttpPut]
    public IActionResult SetMyOptionValue(string value) {
        _myOptionsAccessor.Update(options => options.MyOption = value);
    }
}

```
