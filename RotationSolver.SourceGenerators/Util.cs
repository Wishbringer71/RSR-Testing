using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.SourceGenerators
{
	internal static class Util
	{
		/// <summary>
		/// Gets the parent node of the specified type.
		/// </summary>
		/// <typeparam name="TS">The type of the parent node.</typeparam>
		/// <param name="node">The starting syntax node.</param>
		/// <returns>The parent node of the specified type, or null if not found.</returns>
		public static TS? GetParent<TS>(this SyntaxNode? node) where TS : SyntaxNode
		{
			if (node == null)
			{
				return null;
			}

			if (node is TS result)
			{
				return result;
			}

			return GetParent<TS>(node.Parent);
		}

		/// <summary>
		/// Gets the full metadata name of the specified symbol.
		/// </summary>
		/// <param name="s">The symbol.</param>
		/// <returns>The full metadata name of the symbol.</returns>
		public static string GetFullMetadataName(this ISymbol s)
		{
			if (s == null || s is INamespaceSymbol)
			{
				return string.Empty;
			}

			while (s is not ITypeSymbol)
			{
				s = s.ContainingSymbol;
			}

			if (s == null)
			{
				return string.Empty;
			}

			var sb = new StringBuilder(s.GetTypeSymbolName());

			s = s.ContainingSymbol;
			while (!IsRootNamespace(s))
			{
				try
				{
					sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) + '.');
				}
				catch
				{
					break;
				}

				s = s.ContainingSymbol;
			}

			return sb.ToString();

			static bool IsRootNamespace(ISymbol symbol)
			{
				return symbol is INamespaceSymbol s && s.IsGlobalNamespace;
			}
		}

		private static string GetTypeSymbolName(this ISymbol symbol)
		{
			if (symbol is IArrayTypeSymbol arrayTypeSymbol)
			{
				return arrayTypeSymbol.ElementType.GetFullMetadataName() + "[]";
			}

			var str = symbol.MetadataName;
			if (symbol is INamedTypeSymbol symbolType)
			{
				var strs = str.Split('`');
				if (strs.Length < 2)
				{
					return str;
				}

				str = strs[0];

				var sb2 = new StringBuilder();
				for (var i = 0; i < symbolType.TypeArguments.Length; i++)
				{
					if (i > 0)
					{
						sb2.Append(", ");
					}

					sb2.Append(symbolType.TypeArguments[i].GetFullMetadataName());
				}
				str += "<" + sb2.ToString() + ">";
			}
			return str;
		}

		/// <summary>
		/// Indents each line of the string with four spaces.
		/// </summary>
		/// <param name="str">The input string.</param>
		/// <returns>The indented string.</returns>
		public static string Table(this string str) => "    " + str.Replace("\n", "\n    ");

		/// <summary>
		/// The expression that gives a job its own default of a per-job setting field (A646). A value or a string is
		/// copied by the assignment and keeps the field; any other reference type - a list of action, item or rotation
		/// settings - is copied through the serializer that also saves it, so no two jobs share one object and the
		/// default itself never changes.
		/// </summary>
		/// <param name="fieldType">The field's type.</param>
		/// <param name="fieldTypeName">The field's type as the generated code names it.</param>
		/// <param name="variableName">The field.</param>
		public static string CopyOfDefault(ITypeSymbol fieldType, string fieldTypeName, string variableName)
		{
			return fieldType.IsReferenceType && fieldType.SpecialType != SpecialType.System_String
				? $"Newtonsoft.Json.JsonConvert.DeserializeObject<{fieldTypeName}>(Newtonsoft.Json.JsonConvert.SerializeObject({variableName}))!"
				: variableName;
		}

		/// <summary>
		/// Converts the input string to PascalCase.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>The PascalCase string.</returns>
		public static string ToPascalCase(this string input)
		{
			var parts = input.Split('.');
			for (var i = 0; i < parts.Length; i++)
			{
				parts[i] = ConvertToPascalCase(parts[i]);
			}
			return string.Join(".", parts);

			static string ConvertToPascalCase(string input)
			{
				Regex invalidCharsRgx = new(@"[^_a-zA-Z0-9]");
				Regex whiteSpace = new(@"(?<=\s)");
				Regex startsWithLowerCaseChar = new("^[a-z]");
				Regex firstCharFollowedByUpperCasesOnly = new("(?<=[A-Z])[A-Z0-9]+$");
				Regex lowerCaseNextToNumber = new("(?<=[0-9])[a-z]");
				Regex upperCaseInside = new("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

				var cleaned = invalidCharsRgx.Replace(whiteSpace.Replace(input, "_"), string.Empty);
				var parts = cleaned.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
				var sb = new StringBuilder();
				foreach (var w0 in parts)
				{
					var w = startsWithLowerCaseChar.Replace(w0, m => m.Value.ToUpper());
					w = firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower());
					w = lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper());
					w = upperCaseInside.Replace(w, m => m.Value.ToLower());
					sb.Append(w);
				}

				return sb.ToString();
			}
		}
	}
}