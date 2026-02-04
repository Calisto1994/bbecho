using System.Text;

namespace Calisto
{
    public class ColorUtil
    {
        
        private ColorUtil () {} // Utility class. This is not to be instantiated.
        
        private static readonly Dictionary<string, Dictionary<string, string>> TagMap = new ()
        {
            {"b", new() {{"", "\x1b[1m"}}},
            {"u", new() {{"", "\x1b[4m"}}},
            {"i", new() {{"", "\x1b[3m"}}},
            {"blink", new() {{"", "\x1b[5m"}}},
            {"strike", new() {{"", "\x1b[9m"}}},
            {"color", new ()
                {
                    {"", "\x1b[39m"}, // default value, if someone would use [color] instead of [color=yellow]
                    {"black", "\x1b[30m"},
                    {"red", "\x1b[31m"},
                    {"green", "\x1b[32m"},
                    {"yellow", "\x1b[33m"},
                    {"blue", "\x1b[34m"},
                    {"magenta", "\x1b[35m"},
                    {"cyan", "\x1b[36m"},
                    {"white", "\x1b[37m"},
                }
            },
            {"bgcolor", new ()
                {
                    {"", "\x1b[49m"}, // default value, if someone would use [bgcolor] instead of [bgcolor=yellow]
                    {"black", "\x1b[40m"},
                    {"red", "\x1b[41m"},
                    {"green", "\x1b[42m"},
                    {"yellow", "\x1b[43m"},
                    {"blue", "\x1b[44m"},
                    {"magenta", "\x1b[45m"},
                    {"cyan", "\x1b[46m"},
                    {"white", "\x1b[47m"},
                }
            },
            {"/", new() {{"", "\x1b[0m"}}},
        };

        public static bool TryParse(ref string message, out string? exceptionMessage)
        {
            exceptionMessage = null;
            try
            {
                message = Parse(message);
                return true;
            }
            catch (Exception e) when (e is ArgumentException || e is KeyNotFoundException)
            {
                exceptionMessage = e.Message;
                return false;
            }
        }

        public static bool TryParse(ref string message)
        {
            return TryParse(ref message, out _);
        }

        public static string Parse(string message)
        {
            var tagStack = new Stack<string[]>();
            var isTag = false;
            var isEscaped = false;
            var tagString = "";
            var argString = "";
            var outMessage = "";
            var tagBuilder = new StringBuilder();
            var outBuilder = new StringBuilder();
            var selfClosing = false;
            
            
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '[')
                {
                    if (!isEscaped)
                    {
                        isTag = true;
                    }
                    else
                    {
                        outBuilder.Append(message[i]);
                        isEscaped = false;
                    }
                }
                else if (message[i] == '\\')
                {
                    if (isEscaped) outBuilder.Append(message[i]);
                    else isEscaped = true;
                }
                else if (message[i] == ']')
                {
                    if (isEscaped)
                    {
                        outBuilder.Append(message[i]);
                        isEscaped = false;
                    }
                    else
                    {
                        if (tagBuilder.Length == 0)
                        {
                            throw new ArgumentException(
                                Parse("[color=yellow]Warning[/color]: Tried to end tag without beginning."));   
                        }
                        tagString = tagBuilder.ToString();
                        tagBuilder.Clear();
                        isTag = false;
                        
                        if (tagString.EndsWith(" /"))
                        {
                                tagString = tagString.Substring(0, tagString.Length - 2); // removes the trailing " /".
                                selfClosing = true;
                        } else if (tagString.EndsWith(" //"))
                        {
                            tagString = tagString.Substring(0, tagString.Length - 3); // removes the trailing " //".
                            if (tagStack.Count > 0)
                            {
                                var newStack = new Stack<string[]>();
                                var found = 0;
                                foreach (var tagsToFilter in tagStack.Reverse())
                                {
                                    if (tagsToFilter[0] != tagString)
                                    {
                                        newStack.Push(tagsToFilter);
                                    }
                                    else
                                    {
                                        found++;
                                    }
                                }

                                if (found == 0)
                                    throw new ArgumentException(Parse(
                                        $"[color=yellow]Warning[/]: Tried to clean up \\[{tagString}\\] when there was no instance of it previously used."));

                                tagStack = newStack; // overwrite old stack. We've removed our last tag.
                            }
                            else
                            {
                                throw new ArgumentException(Parse(
                                    "[color=yellow]Warning[/]: Tried to clean up when there's nothing to clean up."));
                            }
                            selfClosing = true;
                        }
                        
                        if (tagString.Contains("="))
                        {
                            argString = tagString.Substring(tagString.IndexOf('=') + 1); // gets argument
                            tagString = tagString.Split('=')[0]; // gets tag
                        }
                        
                        if (TagMap.ContainsKey(tagString))
                        {
                            if (tagString == "/")
                            {
                                tagStack.Clear();
                                outBuilder.Append(TagMap["/"][""]);
                            }
                            else
                            {
                                if (TagMap[tagString].Count == 1)
                                {
                                    outBuilder.Append(TagMap[tagString][""]);
                                    tagStack.Push(new string[] { tagString, "" });
                                }
                                else
                                {
                                    outBuilder.Append(TagMap[tagString][argString]);
                                    tagStack.Push(new string[]{ tagString, argString });
                                }
                            }
                        }
                        else if (tagString[0] == '/')
                        {
                            if (tagStack.Count > 0)
                            {
                                if (tagStack.Peek()[0] == tagString.Substring(1))
                                {
                                    tagStack.Pop();
                                    outBuilder.Append(TagMap["/"][""]);
                                    foreach (var tagsToApply in tagStack.Reverse())
                                    {
                                        outBuilder.Append(TagMap[tagsToApply[0]][tagsToApply[1]]);
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException(Parse($"[color=yellow]Warning[/]: Tried to use \\[{tagString}\\] to close tag, but \\[/{tagStack.Peek()[0]}\\] was expected."));
                                }
                            }
                            else
                            {
                                throw new ArgumentException(Parse($"[color=yellow]Warning[/]: Tried to use \\[{tagString}\\], but no tag was opened previously."));
                            }
                        }
                        else
                        {
                            throw new ArgumentException(Parse("[color=yellow]Warning[/]: Invalid tag: " + tagString));
                        }
                        
                        argString = "";
                        if (selfClosing && tagStack.Count > 0) tagStack.Pop(); // this was a self-closing tag. remove it.
                        selfClosing = false;
                    }
                }
                else
                {
                    if (isTag)
                        tagBuilder.Append(message[i]);
                    else
                        outBuilder.Append(message[i]);
                }
            }

            var openTags = new List<string>();
            if (tagStack.Count > 0)
            {
                foreach (var item in tagStack)
                {
                    if (item[1] != "0")
                    {
                        openTags.Add($"{item[0]}({item[1]})");
                    }
                    else
                    {
                        openTags.Add(item[0]);
                    }
                }

                throw new ArgumentException(Parse("[color=yellow]Warning[/]: There are still some tags open: " + string.Join(" ", openTags.ToArray())));
            }
                    
            outMessage = outBuilder.ToString();
            outBuilder.Clear();
            return outMessage;
        }
    }
}