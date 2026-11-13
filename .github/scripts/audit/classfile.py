"""A class file with its partial parts (A740, A746, concept 24, "Fork-Code in eigenen Teildateien").

The fork's own members of an upstream class stand in `Name.Topic.cs` beside `Name.cs`. A check that reads `Name.cs`
alone would check less the moment a member moves, and pass. Every check that names one class file reads it through
here instead.
"""
import tempfile
from pathlib import Path


def parts(path):
    """The class file first, then its partial parts in name order; empty when the class file is absent."""
    path = Path(path)
    if not path.exists():
        return []
    return [path] + sorted(path.parent.glob(path.stem + '.*.cs'))


def read(path, encoding='utf-8', errors='strict'):
    """The text of a class file and its partial parts, joined by a line break."""
    return '\n'.join(p.read_text(encoding=encoding, errors=errors) for p in parts(path))


def self_test():
    with tempfile.TemporaryDirectory() as folder:
        root = Path(folder)
        (root / 'Foo.cs').write_text('class Foo { int A; }', encoding='utf-8')
        (root / 'Foo.AreaDamage.cs').write_text('partial class Foo { int B; }', encoding='utf-8')
        (root / 'FooBar.cs').write_text('class FooBar { int C; }', encoding='utf-8')
        text = read(root / 'Foo.cs')
        assert 'int A' in text and 'int B' in text, 'a partial part is read with its class'
        assert 'int C' not in text, 'another class with the same prefix is not'
        assert read(root / 'Missing.cs') == '', 'an absent class reads empty'
