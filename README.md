# NETCore Action Template
With this template you can build your Actions without having to configure anything. You just have to change the name and modify the C# code to your liking

## How to use?

### Edit action.yml

Edit action.yml Name, Description and Inputs but don't modify Runs

``` yaml
name: 'Template NET Core Action'
description: 'Console greet whoever you tell'
inputs:
  who-to-greet: 
    required: true
runs:
  using: 'node12'
  main: 'index.js'
```

### Create your Console with NETCore

The folder 'src' has a Console Project very basic.
If you want, you can delete it and create a new project. But you have to have a few indications.


the developer needs to get the parameters assigned to it in the workflow.
For this need I made a small library to obtain the parameter values in a simple way.
This library is [GitHubActionSharp](https://github.com/lauchacarro/GitHubActionSharp) and is available at NuGet

``` csharp
using GitHubActionSharp;
using System;

namespace WhoToGreetAction
{
    enum Parameters
    {
        [Parameter("who-to-greet")]
        WhoToGreet
    }

    class Program
    {
        static void Main(string[] args)
        {
            GitHubActionContext actionContext = new GitHubActionContext(args);
            actionContext.LoadParameters();

            string value = actionContext.GetParameter(Parameters.WhoToGreet);

            Console.WriteLine($"Hello {value}!");
        }
    }
}
```
