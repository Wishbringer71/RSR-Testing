using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

// refscan <target.dll> <provider dir or dll>... : every member reference of the target into a provider assembly must
// resolve to a member of the same name and signature on the type or one of its base types (type forwarding followed).
internal static class RefScan
{
    private static readonly Dictionary<string, MetadataReader> providers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Sig prov = new();

    private static int Main(string[] args)
    {
        var target = args[0];
        var keep = new List<PEReader>();
        foreach (var p in args.Skip(1))
        {
            var files = Directory.Exists(p) ? Directory.GetFiles(p, "*.dll") : [p];
            foreach (var f in files)
            {
                try
                {
                    var pe = new PEReader(File.OpenRead(f));
                    if (!pe.HasMetadata) continue;
                    var r = pe.GetMetadataReader();
                    if (!r.IsAssembly) continue;
                    var name = r.GetString(r.GetAssemblyDefinition().Name);
                    if (!providers.ContainsKey(name)) { providers[name] = r; keep.Add(pe); }
                }
                catch { }
            }
        }
        var tpe = new PEReader(File.OpenRead(target));
        var t = tpe.GetMetadataReader();
        int checkedRefs = 0, missing = 0, skipped = 0;
        var missingByAsm = new SortedDictionary<string, List<string>>();
        var skippedKinds = new SortedDictionary<string, int>();
        foreach (var h in t.MemberReferences)
        {
            var mr = t.GetMemberReference(h);
            if (!TryParent(t, mr.Parent, out var asm, out var typeName))
            {
                skipped++;
                var kind = mr.Parent.Kind.ToString();
                if (mr.Parent.Kind == HandleKind.TypeSpecification)
                {
                    var b = t.GetBlobReader(t.GetTypeSpecification((TypeSpecificationHandle)mr.Parent).Signature);
                    var code = b.ReadSignatureTypeCode();
                    kind += ":" + code;
                    if (code == SignatureTypeCode.GenericTypeInstance) { _ = b.ReadCompressedInteger(); kind += ":" + b.ReadTypeHandle().Kind; }
                }
                skippedKinds[kind] = skippedKinds.GetValueOrDefault(kind) + 1;
                continue;
            }
            if (!providers.TryGetValue(asm, out var pr)) continue;
            var name = t.GetString(mr.Name);
            var isField = mr.GetKind() == MemberReferenceKind.Field;
            var sig = isField ? mr.DecodeFieldSignature(prov, t) : MethodSig(mr.DecodeMethodSignature(prov, t));
            checkedRefs++;
            if (!Find(asm, typeName, name, isField, sig, []))
            {
                missing++;
                (missingByAsm.TryGetValue(asm, out var l) ? l : missingByAsm[asm] = []).Add($"{typeName}::{name} {sig}");
            }
        }
        Console.WriteLine($"{Path.GetFileName(target)}: {checkedRefs} member references into {providers.Count} provider assemblies checked, {missing} unresolved, {skipped} with a parent not checked");
        foreach (var (a, l) in missingByAsm) foreach (var m in l) Console.WriteLine($"  MISSING [{a}] {m}");
        foreach (var (k, n) in skippedKinds) Console.WriteLine($"  not checked: {k} {n}");
        return missing == 0 ? 0 : 1;

    }

    static bool Find(string asm, string typeName, string member, bool isField, string sig, HashSet<string> seen)
    {
        // A type seen once on this walk ends it: a forwarding loop or a cycle in broken metadata.
        if (!seen.Add(asm + "|" + typeName) || !providers.TryGetValue(asm, out var r)) return false;
        foreach (var th in r.TypeDefinitions)
        {
            var td = r.GetTypeDefinition(th);
            if (FullName(r, th) != typeName) continue;
            if (isField)
            {
                foreach (var fh in td.GetFields()) { var fd = r.GetFieldDefinition(fh); if (r.GetString(fd.Name) == member && fd.DecodeSignature(prov, r) == sig) return true; }
            }
            else
            {
                foreach (var mh in td.GetMethods()) { var md = r.GetMethodDefinition(mh); if (r.GetString(md.Name) == member && MethodSig(md.DecodeSignature(prov, r)) == sig) return true; }
            }
            if (td.BaseType.IsNil) return false;
            return TryParent(r, td.BaseType, out var basm, out var bname, asm) && Find(basm, bname, member, isField, sig, seen);
        }
        foreach (var eh in r.ExportedTypes)
        {
            var et = r.GetExportedType(eh);
            var en = (r.GetString(et.Namespace) is { Length: > 0 } ns ? ns + "." : "") + r.GetString(et.Name);
            if (en == typeName && et.Implementation.Kind == HandleKind.AssemblyReference)
                return Find(r.GetString(r.GetAssemblyReference((AssemblyReferenceHandle)et.Implementation).Name), typeName, member, isField, sig, seen);
        }
        return false;
    }

    static string MethodSig(MethodSignature<string> s) => $"{s.ReturnType} ({string.Join(", ", s.ParameterTypes)}) g{s.GenericParameterCount}";

    static string FullName(MetadataReader r, TypeDefinitionHandle h)
    {
        var td = r.GetTypeDefinition(h);
        var n = r.GetString(td.Name);
        var dt = td.GetDeclaringType();
        if (!dt.IsNil) return FullName(r, dt) + "/" + n;
        var ns = r.GetString(td.Namespace);
        return ns.Length > 0 ? ns + "." + n : n;
    }

    static bool TryParent(MetadataReader r, EntityHandle parent, out string asm, out string typeName, string? selfAsm = null)
    {
        asm = typeName = "";
        switch (parent.Kind)
        {
            case HandleKind.TypeReference:
                return RefName(r, (TypeReferenceHandle)parent, out asm, out typeName);
            case HandleKind.TypeDefinition:
                if (selfAsm == null) return false;
                asm = selfAsm; typeName = FullName(r, (TypeDefinitionHandle)parent); return true;
            case HandleKind.TypeSpecification:
                var blob = r.GetBlobReader(r.GetTypeSpecification((TypeSpecificationHandle)parent).Signature);
                if (blob.ReadSignatureTypeCode() != SignatureTypeCode.GenericTypeInstance) return false;
                _ = blob.ReadCompressedInteger();
                var inner = blob.ReadTypeHandle();
                return TryParent(r, inner, out asm, out typeName, selfAsm);
            default: return false;
        }
    }

    static bool RefName(MetadataReader r, TypeReferenceHandle h, out string asm, out string name)
    {
        var tr = r.GetTypeReference(h);
        var n = r.GetString(tr.Name);
        asm = "";
        switch (tr.ResolutionScope.Kind)
        {
            case HandleKind.AssemblyReference:
                asm = r.GetString(r.GetAssemblyReference((AssemblyReferenceHandle)tr.ResolutionScope).Name);
                name = (r.GetString(tr.Namespace) is { Length: > 0 } ns ? ns + "." : "") + n; return true;
            case HandleKind.TypeReference:
                if (!RefName(r, (TypeReferenceHandle)tr.ResolutionScope, out asm, out var outer)) { name = ""; return false; }
                name = outer + "/" + n; return true;
            default: name = ""; return false;
        }
    }

}

sealed class Sig : ISignatureTypeProvider<string, MetadataReader>
{
    public string GetArrayType(string e, ArrayShape s) => $"{e}[{new string(',', s.Rank - 1)}]";
    public string GetByReferenceType(string e) => e + "&";
    public string GetFunctionPointerType(MethodSignature<string> s) => $"fnptr({s.ReturnType};{string.Join(",", s.ParameterTypes)})";
    public string GetGenericInstantiation(string g, ImmutableArray<string> a) => $"{g}<{string.Join(",", a)}>";
    public string GetGenericMethodParameter(MetadataReader c, int i) => "!!" + i;
    public string GetGenericTypeParameter(MetadataReader c, int i) => "!" + i;
    public string GetModifiedType(string m, string u, bool req) => req ? $"{u} modreq({m})" : u;
    public string GetPinnedType(string e) => e;
    public string GetPointerType(string e) => e + "*";
    public string GetPrimitiveType(PrimitiveTypeCode c) => c.ToString();
    public string GetSZArrayType(string e) => e + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte k) => Name(r, h);
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte k)
    {
        var tr = r.GetTypeReference(h);
        var n = r.GetString(tr.Name);
        if (tr.ResolutionScope.Kind == HandleKind.TypeReference) return GetTypeFromReference(r, (TypeReferenceHandle)tr.ResolutionScope, 0) + "/" + n;
        return (r.GetString(tr.Namespace) is { Length: > 0 } ns ? ns + "." : "") + n;
    }
    public string GetTypeFromSpecification(MetadataReader r, MetadataReader c, TypeSpecificationHandle h, byte k) => r.GetTypeSpecification(h).DecodeSignature(this, c);
    static string Name(MetadataReader r, TypeDefinitionHandle h)
    {
        var td = r.GetTypeDefinition(h);
        var n = r.GetString(td.Name);
        var dt = td.GetDeclaringType();
        if (!dt.IsNil) return Name(r, dt) + "/" + n;
        return (r.GetString(td.Namespace) is { Length: > 0 } ns ? ns + "." : "") + n;
    }
}
