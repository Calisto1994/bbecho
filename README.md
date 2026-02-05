# BBEcho
BBEcho is a small command-line Utility that allows for BBCode-like syntax for formatting Shell strings.\
It includes popular tags like **[b]**, *[i]* and <ins>[u]</ins> for **fat**, *italic* and <ins>underllined</ins> text.

## All supported tags

### Colors
`[color={black,white,red,yellow,green,magenta,blue,cyan}]`
for foreground colors and\
`[bgcolor={black,white,red,yellow,green,magenta,blue,cyan}]`
for background colors.
### Text options
`[b]` for **bold** text\
`[i]` for *italic* text\
`[u]` for <ins>underlined</ins> text

Using a color tag without the parameter (e.g. `[color]` instead of `[color=yellow]`)
will reset the color to the default.
### Special codes
`[/]` will end *all* previous tags.\
A self-closing tag like `[color=red /]` will make BBEcho set the color but ignore the tag otherwise.\
A double-self-closing tag like `[color //]` will remove all instances of `[color]` from the memory and allow for a "clean start".

### Example
```bash
#!/bin/bash

./bbecho '[bgcolor=black][color=white]Willkommen bei meinem Skript![/]'
```

### Functionality
Uses the `Stack` functionality of the C# programming language (and thus, the .NET Framework / .NET Core Runtime)

### Compatibility
Tested on Linux. But since it's C#, it should work on Windows as well.