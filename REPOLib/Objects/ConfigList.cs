using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Objects;

internal class ConfigList(List<string> items)
{
    public readonly List<string> Items = items;
}