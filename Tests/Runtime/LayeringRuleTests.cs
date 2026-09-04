using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Jeomseon.Unity.UI.Tests
{
    /// <summary>
    /// <see cref="Navigation"/>·<see cref="Transition"/> 하위 계층은 하나의 asmdef 안에 있어
    /// 컴파일러가 의존 방향을 막지 못합니다. 대신 이 reflection 테스트가 계층 규칙을 고정합니다
    /// (ADR-0009 §4). 나중에 최후순위 #6 `DependencyAnalyzer`가 이 검사를 대체합니다.
    /// </summary>
    public sealed class LayeringRuleTests
    {
        private const string Root = "Jeomseon.Unity.UI";
        private const string Channels = "Jeomseon.Unity.UI.Channels";
        private const string Components = "Jeomseon.Unity.UI.Components";
        private const string Navigation = "Jeomseon.Unity.UI.Navigation";
        private const string Transition = "Jeomseon.Unity.UI.Transition";

        private static Assembly UiAssembly => typeof(UIView).Assembly;

        private static IEnumerable<Type> TypesInNamespace(string ns) =>
            UiAssembly.GetTypes().Where(type => type.Namespace == ns);

        private static IEnumerable<string> ReferencedNamespaces(Type type)
        {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            IEnumerable<Type> Signature()
            {
                if (type.BaseType != null) yield return type.BaseType;
                foreach (Type i in type.GetInterfaces()) yield return i;
                foreach (FieldInfo f in type.GetFields(all)) yield return f.FieldType;
                foreach (PropertyInfo p in type.GetProperties(all)) yield return p.PropertyType;
                foreach (ConstructorInfo c in type.GetConstructors(all))
                    foreach (ParameterInfo p in c.GetParameters()) yield return p.ParameterType;
                foreach (MethodInfo m in type.GetMethods(all))
                {
                    yield return m.ReturnType;
                    foreach (ParameterInfo p in m.GetParameters()) yield return p.ParameterType;
                }
            }

            foreach (Type referenced in Signature())
            {
                Type t = referenced;
                if (t.IsByRef || t.IsArray || t.IsPointer) t = t.GetElementType();
                if (t == null) continue;

                if (t.IsGenericType)
                {
                    if (t.GetGenericTypeDefinition().Namespace is { } genNs) yield return genNs;
                    foreach (Type arg in t.GetGenericArguments())
                        if (arg.Namespace is { } argNs) yield return argNs;
                }
                else if (t.Namespace is { } ns)
                {
                    yield return ns;
                }
            }
        }

        private static void AssertNoReference(string fromNamespace, params string[] forbidden)
        {
            var violations = new List<string>();
            foreach (Type type in TypesInNamespace(fromNamespace))
            foreach (string referenced in ReferencedNamespaces(type).Distinct())
                if (forbidden.Contains(referenced))
                    violations.Add($"{type.FullName} -> {referenced}");

            Assert.That(violations, Is.Empty,
                $"'{fromNamespace}' 타입이 참조하면 안 되는 네임스페이스를 참조합니다:\n  " +
                string.Join("\n  ", violations));
        }

        [Test]
        public void Core_DoesNotReferenceNavigationOrTransition()
        {
            AssertNoReference(Root, Navigation, Transition);
            AssertNoReference(Channels, Navigation, Transition);
            AssertNoReference(Components, Navigation, Transition);
        }

        [Test]
        public void Transition_DoesNotReferenceNavigation()
            => AssertNoReference(Transition, Navigation);

        [Test]
        public void Navigation_DoesNotReferenceTransition()
            => AssertNoReference(Navigation, Transition);

        [Test]
        public void SubLayerNamespacesExist()
        {
            Assert.That(TypesInNamespace(Transition).Any(), Is.True, "Transition 네임스페이스에 타입이 없습니다.");
            Assert.That(TypesInNamespace(Navigation).Any(), Is.True, "Navigation 네임스페이스에 타입이 없습니다.");
        }

        // --- 소스 레벨 검사 -------------------------------------------------
        // reflection은 시그니처(필드·프로퍼티·파라미터·반환·베이스·인터페이스)만 본다. 메서드 본문에서만
        // 쓰이는 `using Jeomseon.Unity.UI.Transition;` 같은 파일 단위 using은 못 잡으므로, 하위 계층
        // 소스 파일을 직접 읽어 반대편 네임스페이스 using을 금지한다.

        [Test]
        public void NavigationSource_HasNoUsingOfTransition()
            => AssertNoCrossLayerUsing("Navigation", "Transition");

        [Test]
        public void TransitionSource_HasNoUsingOfNavigation()
            => AssertNoCrossLayerUsing("Transition", "Navigation");

        private static void AssertNoCrossLayerUsing(string fromFolder, string forbiddenLeaf)
        {
            string runtimeDir = Path.GetFullPath(Path.Combine(TestSourceDirectory(), "..", "..", "Runtime", fromFolder));
            if (!Directory.Exists(runtimeDir))
            {
                Assert.Inconclusive($"소스 폴더를 찾지 못했습니다: {runtimeDir}");
                return;
            }

            var usingPattern = new Regex($@"^\s*using\s+Jeomseon\.Unity\.UI\.{Regex.Escape(forbiddenLeaf)}\s*;", RegexOptions.Multiline);
            var qualifiedPattern = new Regex($@"\bJeomseon\.Unity\.UI\.{Regex.Escape(forbiddenLeaf)}\.");

            var violations = new List<string>();
            foreach (string file in Directory.GetFiles(runtimeDir, "*.cs", SearchOption.AllDirectories))
            {
                string source = StripComments(File.ReadAllText(file));
                if (usingPattern.IsMatch(source) || qualifiedPattern.IsMatch(source))
                    violations.Add(Path.GetFileName(file));
            }

            Assert.That(violations, Is.Empty,
                $"Runtime/{fromFolder}/*.cs가 'Jeomseon.Unity.UI.{forbiddenLeaf}'를 참조합니다: " +
                string.Join(", ", violations));
        }

        private static string StripComments(string source)
        {
            source = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            source = Regex.Replace(source, @"^\s*///.*$", string.Empty, RegexOptions.Multiline);
            source = Regex.Replace(source, @"//.*$", string.Empty, RegexOptions.Multiline);
            return source;
        }

        private static string TestSourceDirectory([CallerFilePath] string path = "")
            => Path.GetDirectoryName(path);
    }
}
