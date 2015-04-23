# C# Essentials

C# Essentials is a collection of Roslyn diagnostic analyzers, code fixes and
refactorings that make it easy to work with C# 6 language features,
such as [nameof expressions](https://github.com/dotnet/roslyn/wiki/New-Language-Features-in-C%23-6#nameof-expressions),
[getter-only auto-properties](https://github.com/dotnet/roslyn/wiki/New-Language-Features-in-C%23-6#getter-only-auto-properties),
[expression-bodied members](https://github.com/dotnet/roslyn/wiki/New-Language-Features-in-C%23-6#expression-bodied-function-members),
and [string interpolation](https://github.com/dotnet/roslyn/wiki/New-Language-Features-in-C%23-6#string-interpolation).

Supports Visual Studio 2015

## Features

### Use NameOf

Identifies calls where a parameter name is passed as a string to an argument
named "paramName". This is a simple-yet-effective heuristic for detecting
cases like the one below:

![](http://i.imgur.com/JnNB8nZ.jpg)

### Use Getter-Only Auto-Property

Determines when the ```private set``` in an auto-property can be removed.

![](http://i.imgur.com/je8HpdD.jpg)

### Use Expression-Bodied Member

Makes it clear when a member can be converted into an expression-bodied
member.

![](http://i.imgur.com/vF4PY9o.jpg)

### Convert to Interpolated String

This handy refactoring makes it a breeze to transform a ```String.Format```
call into an interpolated strings.

![](http://i.imgur.com/Q1CMKD5.jpg)

