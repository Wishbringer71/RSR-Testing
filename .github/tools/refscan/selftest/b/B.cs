public static class Use
{
    public static void Run()
    {
        var c = new LibA.C();
        c.Foo(default);
        c.Bar = default;
        c.Inherited(string.Empty);
        _ = LibA.C.Gen(default(int));
        LibA.C.Nested.N();
        new LibA.G<int>().Take(default);
    }
}
