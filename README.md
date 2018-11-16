# IOptionsWriter

Easy way to write appsettings.json or other configuration json file

## How to use

```powershell
Install-Package IOptionsWriter -Version 0.0.1
```

### Using

```c#
IServiceCollection.ConfigureWritable(IConfigurationSection section, string settingsFile = "appsettings.json", bool reloadAfterWrite = false)
```

section - your configuration section

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
        .ConfigureWritable<MyOptions>(Configuration.GetSection(nameof(MySettings)));
}
```

Controllers/MyController.cs:

```c#
[Controller]
public class MyController : Controller {
    private readonly IOptionsWritable<MyOptions> _myOptionsAccessor;

    public TeamsController(IOptionsWritable<MyOptions> myOptionsAccessor) {
        _myOptionsAccessor = myOptionsAccessor;
    }

    [HttpPut]
    public IActionResult SetMyOptionValue(string value) {
        _myOptionsAccessor.Update(options => options.MyOptions = value);
    }
}

```
