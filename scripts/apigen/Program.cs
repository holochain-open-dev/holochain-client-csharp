using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// apigen <bin dir> <out dir> <assembly name> <title> [<assembly name> <title> ...]
// Writes one Markdown API reference per assembly: every public type and member, with its XML doc summary.
string bin = args[0], outDir = args[1];
LoadReadme(Environment.GetEnvironmentVariable("APIGEN_README"));
Directory.CreateDirectory(outDir);
var alc = new AssemblyLoadContext("apigen");
alc.Resolving += (ctx, name) =>
{
    string p = Path.Combine(bin, name.Name + ".dll");
    return File.Exists(p) ? ctx.LoadFromAssemblyPath(p) : null;
};

for (int a = 2; a + 1 < args.Length; a += 2)
{
    string asmName = args[a], title = args[a + 1];
    Assembly asm = alc.LoadFromAssemblyPath(Path.Combine(bin, asmName + ".dll"));
    var docs = LoadDocs(Path.Combine(bin, asmName + ".xml"));
    Type[] types;
    try { types = asm.GetExportedTypes(); }
    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null && t.IsPublic).ToArray(); }
    types = types.Where(t => !t.IsNested || t.IsNestedPublic).OrderBy(t => t.Namespace).ThenBy(t => t.Name).ToArray();

    var sb = new StringBuilder();
    sb.AppendLine($"# {title} API reference");
    sb.AppendLine();
    sb.AppendLine($"Generated from `{asmName}` {asm.GetName().Version} (every public type and member, with its XML documentation where it has one). For guides and examples see the [documentation index](../README.md).");
    sb.AppendLine();

    foreach (var ns in types.GroupBy(t => t.Namespace ?? "(global)"))
    {
        sb.AppendLine($"- **{ns.Key}**: " + string.Join(", ", ns.Select(t => $"[{Esc(TypeName(t))}](#{Anchor(t)})")));
    }
    sb.AppendLine();

    foreach (var ns in types.GroupBy(t => t.Namespace ?? "(global)"))
    {
        sb.AppendLine($"## {ns.Key}");
        sb.AppendLine();
        foreach (Type t in ns)
        {
            try { WriteType(sb, t, docs); }
            catch (Exception ex) { sb.AppendLine($"_(could not reflect {t.Name}: {ex.GetType().Name})_\n"); }
        }
    }

    string file = Path.Combine(outDir, asmName.Replace("NextGenSoftware.Holochain.HoloNET.", "") + ".md");
    File.WriteAllText(file, sb.ToString().Replace("\r\n", "\n"));
    Console.WriteLine($"{file}: {types.Length} types, {sb.Length / 1024} KB");
}

static void WriteType(StringBuilder sb, Type t, Dictionary<string, string> docs)
{
    string kind = t.IsEnum ? "enum" : t.IsInterface ? "interface" : t.IsValueType ? "struct" : typeof(Delegate).IsAssignableFrom(t) ? "delegate" : t.IsAbstract && t.IsSealed ? "static class" : t.IsAbstract ? "abstract class" : "class";
    sb.AppendLine($"<a id=\"{Anchor(t)}\"></a>");
    sb.AppendLine($"### {Esc(TypeName(t))} ({kind})");
    sb.AppendLine();
    if (Doc(docs, "T:" + DocId(t)) is string ts) { sb.AppendLine(ts); sb.AppendLine(); }

    var bases = new List<string>();
    if (t.BaseType != null && t.BaseType != typeof(object) && t.BaseType != typeof(ValueType) && t.BaseType != typeof(Enum)) bases.Add(TypeName(t.BaseType));
    bases.AddRange(t.GetInterfaces().Where(i => i.IsPublic).Select(TypeName).Take(8));
    if (bases.Count > 0 && !t.IsEnum) { sb.AppendLine("Inherits / implements: `" + string.Join("`, `", bases) + "`"); sb.AppendLine(); }

    if (t.IsEnum)
    {
        sb.AppendLine("| Value | Description |");
        sb.AppendLine("|---|---|");
        foreach (string n in Enum.GetNames(t))
            sb.AppendLine($"| `{n}` | {Cell(Doc(docs, $"F:{DocId(t)}.{n}"))} |");
        sb.AppendLine();
        return;
    }

    const BindingFlags F = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    var rows = new List<(string Kind, string Sig, string Doc)>();

    foreach (var c in t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
    {
        string ctorDoc = Doc(docs, "M:" + DocId(t) + ".#ctor" + ParamIds(c.GetParameters()));
        if (c.GetParameters().Length == 0 && ctorDoc == null) continue; // an undocumented default constructor says nothing
        rows.Add(("ctor", $"{TypeName(t).Split('<')[0]}({Params(c.GetParameters())})", ctorDoc));
    }
    foreach (var p in t.GetProperties(F).OrderBy(p => p.Name))
        rows.Add(("property", $"{TypeName(p.PropertyType)} {p.Name}" + (p.CanWrite && p.SetMethod?.IsPublic == true ? " { get; set; }" : " { get; }") + (IsStatic(p) ? " (static)" : ""), Doc(docs, $"P:{DocId(t)}.{p.Name}")));
    foreach (var f in t.GetFields(F).Where(f => !f.IsSpecialName).OrderBy(f => f.Name))
        rows.Add(("field", $"{(f.IsLiteral ? "const " : f.IsStatic ? "static " : "")}{TypeName(f.FieldType)} {f.Name}", Doc(docs, $"F:{DocId(t)}.{f.Name}")));
    foreach (var e in t.GetEvents(F).OrderBy(e => e.Name))
        rows.Add(("event", $"{TypeName(e.EventHandlerType)} {e.Name}", Doc(docs, $"E:{DocId(t)}.{e.Name}")));
    foreach (var m in t.GetMethods(F).Where(m => !m.IsSpecialName).OrderBy(m => m.Name).ThenBy(m => m.GetParameters().Length))
    {
        string generic = m.IsGenericMethodDefinition ? "<" + string.Join(", ", m.GetGenericArguments().Select(g => g.Name)) + ">" : "";
        string id = $"M:{DocId(t)}.{m.Name}" + (m.IsGenericMethodDefinition ? "``" + m.GetGenericArguments().Length : "") + ParamIds(m.GetParameters());
        string obsolete = m.GetCustomAttributesData().Any(a => a.AttributeType.Name == "ObsoleteAttribute") ? " **(obsolete)**" : "";
        rows.Add(("method", $"{(m.IsStatic ? "static " : "")}{TypeName(m.ReturnType)} {m.Name}{generic}({Params(m.GetParameters())})", (Doc(docs, id) ?? "") + obsolete));
    }

    // Members without their own XML doc: use a documented overload of the same name, then the sync/async twin, then the
    // README's tables for this class (or a member name only one README table describes).
    var byName = new Dictionary<string, string>();
    foreach (var r in rows.Where(r => !string.IsNullOrEmpty(r.Doc)))
    {
        string n = MemberName(r.Sig, r.Kind);
        if (n != null && !byName.ContainsKey(n)) byName[n] = r.Doc;
    }
    for (int i = 0; i < rows.Count; i++)
    {
        if (!string.IsNullOrEmpty(rows[i].Doc) && rows[i].Doc != " **(obsolete)**") continue;
        string n = MemberName(rows[i].Sig, rows[i].Kind);
        if (n == null) continue;
        string twin = n.EndsWith("Async") ? n.Substring(0, n.Length - 5) : n + "Async";
        string found = byName.GetValueOrDefault(n) ?? byName.GetValueOrDefault(twin) ?? Readme(TypeName(t).Split('<')[0], n) ?? Readme(TypeName(t).Split('<')[0], twin);
        if (found != null) rows[i] = (rows[i].Kind, rows[i].Sig, found + (rows[i].Doc ?? ""));
    }

    // Request / response objects (the .Data namespaces) mirror Holochain's conductor API structs field for field.
    if ((t.Namespace ?? "").Contains(".Data."))
        for (int i = 0; i < rows.Count; i++)
        {
            if (!string.IsNullOrEmpty(rows[i].Doc) || rows[i].Kind != "property") continue;
            string n = MemberName(rows[i].Sig, rows[i].Kind);
            if (n == null) continue;
            string field = Regex.Replace(n, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
            rows[i] = (rows[i].Kind, rows[i].Sig, $"Holochain's `{field}` field (sent to / received from the conductor as-is).");
        }

    // An interface member without docs: use the implementing class's (IHoloNETClientAdmin -> HoloNETClientAdmin).
    if (t.IsInterface && t.Name.StartsWith("I"))
    {
        string implId = (t.Namespace ?? "") + "." + t.Name.Substring(1);
        Type impl = t.Assembly.GetExportedTypes().FirstOrDefault(x => !x.IsInterface && x.Name == t.Name.Substring(1) && t.IsAssignableFrom(x));
        if (impl != null)
        {
            string implDocId = DocId(impl);
            for (int i = 0; i < rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(rows[i].Doc)) continue;
                string n = MemberName(rows[i].Sig, rows[i].Kind);
                if (n == null) continue;
                string prefix = rows[i].Kind switch { "property" => "P:", "event" => "E:", "field" => "F:", _ => "M:" };
                string found = docs.Where(kv => kv.Key.StartsWith(prefix + implDocId + "." + n + "(") || kv.Key == prefix + implDocId + "." + n).Select(kv => kv.Value).FirstOrDefault();
                if (found != null) rows[i] = (rows[i].Kind, rows[i].Sig, found);
            }
        }
    }

    if (rows.Count == 0) return;
    sb.AppendLine("| Member | Kind | Description |");
    sb.AppendLine("|---|---|---|");
    foreach (var r in rows)
        sb.AppendLine($"| `{r.Sig.Replace("|", "\\|")}` | {r.Kind} | {Cell(r.Doc)} |");
    sb.AppendLine();
}

static bool IsStatic(PropertyInfo p) => (p.GetMethod ?? p.SetMethod)?.IsStatic == true;

static string Params(ParameterInfo[] ps) => string.Join(", ", ps.Select(p =>
    (p.ParameterType.IsByRef ? (p.IsOut ? "out " : "ref ") : "") + TypeName(p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType) + " " + p.Name +
    (p.HasDefaultValue ? " = " + (p.DefaultValue == null ? "null" : p.DefaultValue is string s ? "\"" + s + "\"" : p.DefaultValue is bool b ? (b ? "true" : "false") : p.DefaultValue.ToString()) : "")));

static string TypeName(Type t)
{
    if (t == null) return "void";
    if (t.IsByRef) return TypeName(t.GetElementType());
    if (t.IsArray) return TypeName(t.GetElementType()) + "[" + new string(',', t.GetArrayRank() - 1) + "]";
    var alias = new Dictionary<Type, string> { [typeof(void)] = "void", [typeof(string)] = "string", [typeof(int)] = "int", [typeof(bool)] = "bool", [typeof(object)] = "object", [typeof(long)] = "long", [typeof(double)] = "double", [typeof(float)] = "float", [typeof(byte)] = "byte", [typeof(uint)] = "uint", [typeof(ulong)] = "ulong", [typeof(ushort)] = "ushort", [typeof(short)] = "short", [typeof(decimal)] = "decimal", [typeof(char)] = "char" };
    foreach (var kv in alias) if (kv.Key.FullName == t.FullName && t.FullName != null) return kv.Value;
    if (t.IsGenericType)
    {
        if (t.GetGenericTypeDefinition().FullName == "System.Nullable`1") return TypeName(t.GetGenericArguments()[0]) + "?";
        string n = t.Name.Split('`')[0];
        return n + "<" + string.Join(", ", t.GetGenericArguments().Select(TypeName)) + ">";
    }
    return t.IsNested && !t.IsGenericParameter ? TypeName(t.DeclaringType) + "." + t.Name : t.Name;
}

static string DocId(Type t) => (t.IsNested ? DocId(t.DeclaringType) + "." + t.Name : (t.Namespace != null ? t.Namespace + "." : "") + t.Name);

static string ParamIds(ParameterInfo[] ps) => ps.Length == 0 ? "" : "(" + string.Join(",", ps.Select(p => ParamId(p.ParameterType))) + ")";

static string ParamId(Type t)
{
    if (t.IsByRef) return ParamId(t.GetElementType()) + "@";
    if (t.IsArray) return ParamId(t.GetElementType()) + "[]";
    if (t.IsGenericParameter) return (t.DeclaringMethod != null ? "``" : "`") + t.GenericParameterPosition;
    if (t.IsGenericType)
        return (t.Namespace + "." + t.Name.Split('`')[0]) + "{" + string.Join(",", t.GetGenericArguments().Select(ParamId)) + "}";
    return t.IsNested ? ParamId(t.DeclaringType) + "." + t.Name : t.Namespace + "." + t.Name;
}

static Dictionary<string, string> LoadDocs(string path)
{
    var d = new Dictionary<string, string>();
    if (!File.Exists(path)) return d;
    foreach (var m in XDocument.Load(path).Descendants("member"))
    {
        var s = m.Element("summary");
        if (s == null) continue;
        string text = string.Concat(s.Nodes().Select(n => n is XElement e
            ? (e.Attribute("cref") is XAttribute c ? "`" + c.Value.Split(':').Last().Split('(')[0].Split('.').Last() + "`" : e.Attribute("langword") is XAttribute l ? "`" + l.Value + "`" : e.Name.LocalName == "c" ? "`" + e.Value + "`" : e.Value)
            : n.ToString()));
        d[m.Attribute("name").Value] = Regex.Replace(System.Net.WebUtility.HtmlDecode(text), @"\s+", " ").Trim();
    }
    return d;
}

static string Doc(Dictionary<string, string> d, string id) => d.TryGetValue(id, out string s) && s.Length > 0 ? s : null;
static string Cell(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("|", "\\|");
static string Esc(string s) => s.Replace("<", "&lt;").Replace(">", "&gt;");
static string Anchor(Type t) => Regex.Replace(DocId(t).ToLowerInvariant(), "[^a-z0-9]+", "-");

static string MemberName(string sig, string kind)
{
    if (kind == "ctor") return null;
    var m = Regex.Match(sig, kind == "method" ? @"([A-Za-z0-9_]+)(<[^(]*>)?\(" : @"([A-Za-z0-9_]+)( \{| \(static\)|$)");
    return m.Success ? m.Groups[1].Value : null;
}


static void LoadReadme(string path)
{
    if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
    string cls = "";
    string[] all = File.ReadAllLines(path);
    // "##### MemberName" sections: the first prose paragraph after the heading (skipping code blocks) describes the member.
    for (int i = 0; i < all.Length; i++)
    {
        var top = Regex.Match(all[i], @"^### +([A-Za-z0-9]+)");
        if (top.Success) cls = top.Groups[1].Value.Replace("Class", "");
        var sec = Regex.Match(all[i], @"^#{4,5} +`?([A-Z][A-Za-z0-9]+)`?\s*$");
        if (!sec.Success) continue;
        bool inCode = false;
        for (int j = i + 1; j < all.Length && j < i + 40; j++)
        {
            string l = all[j].Trim();
            if (l.StartsWith("```")) { inCode = !inCode; continue; }
            if (inCode || l.Length == 0 || l.StartsWith("<a ")) continue;
            if (l.StartsWith("#") || l.StartsWith("|")) break;
            string desc = Regex.Replace(Regex.Replace(l, @"\[([^\]]+)\]\([^)]*\)", "$1"), @"\*\*", "");
            ReadmeData.ReadmeByClass.TryAdd((cls, sec.Groups[1].Value), desc);
            break;
        }
    }
    cls = "";
    foreach (string line in all)
    {
        var h = Regex.Match(line, @"^#{2,4} +([A-Za-z0-9]+)");
        if (h.Success && line.StartsWith("### ") ) cls = h.Groups[1].Value.Replace("Class", "");
        var row = Regex.Match(line, @"^\| *\[?([A-Z][A-Za-z0-9]+)\]?(\([^)]*\))? *\| *(.+?) *\|\s*$");
        if (!row.Success) continue;
        string name = row.Groups[1].Value, desc = Regex.Replace(row.Groups[3].Value, @"\[([^\]]+)\]\([^)]*\)", "$1").Trim();
        if (desc.Length < 8 || desc.StartsWith("---") || name is "Event" or "Method" or "Property" or "Parameter" or "Package") continue;
        ReadmeData.ReadmeByClass.TryAdd((cls, name), desc);
        if (!ReadmeData.ReadmeByName.TryGetValue(name, out var l)) ReadmeData.ReadmeByName[name] = l = new List<string>();
        if (!l.Contains(desc)) l.Add(desc);
    }
}

static string Readme(string cls, string name)
{
    foreach (string c in new[] { cls, cls.TrimStart('I'), cls.Replace("Base", ""), cls.Replace("HoloNETClientAppAgent", "HoloNETClient").Replace("HoloNETClientApp", "HoloNETClient") })
        if (ReadmeData.ReadmeByClass.TryGetValue((c, name), out string d)) return d;
    return ReadmeData.ReadmeByName.TryGetValue(name, out var l) && l.Count == 1 ? l[0] : null;
}

static class ReadmeData
{
    internal static readonly Dictionary<(string, string), string> ReadmeByClass = new();
    internal static readonly Dictionary<string, List<string>> ReadmeByName = new();
}
