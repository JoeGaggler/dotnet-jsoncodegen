using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pingmint.CodeGen.CSharp;

public partial class CodeWriter
{
    private Int32 currentIndentation = 0;

    private readonly StringBuilder stringBuilder = new();

    public override string ToString() => this.stringBuilder.ToString();

    public void Indent() => currentIndentation++;

    public void Dedent() => currentIndentation--;

    public void Text(String text) => this.stringBuilder.Append(text);

    public void Text(String format, params String[] args) => this.stringBuilder.AppendFormat(format, args);

    public void StartLine() => Text(new String('\t', currentIndentation));

    public void Line() => this.stringBuilder.AppendLine();

    public void Line(String text)
    {
        StartLine();
        this.stringBuilder.AppendLine(text);
    }

    public void Line(String format, params String[] args)
    {
        StartLine();
        this.stringBuilder.AppendLine(String.Format(format, args));
    }

    public void UsingNamespace(String namespaceIdentifier) => Line("using {0};", namespaceIdentifier);


    public void FileNamespace(String namespaceIdentifer) => Line($"namespace {namespaceIdentifer};");

    public IDisposable Namespace(String namespaceIdentifier) => new NamespaceScope(this, namespaceIdentifier);

    private sealed class NamespaceScope : IDisposable
    {
        private readonly CodeWriter writer;

        public NamespaceScope(CodeWriter writer, String namespaceIdentifer)
        {
            this.writer = writer;

            this.writer.Line("namespace {0}", namespaceIdentifer);
            this.writer.Line("{");
            this.writer.Indent();

        }

        public void Dispose()
        {
            this.writer.Dedent();
            this.writer.Line("}");
        }
    }
}

public partial class CodeWriter
{
    public void Comment(String comment) => Line("// {0}", comment);

    public IDisposable CreateBraceScope(String? preamble = null, String? withClosingBrace = null) => new BraceScope(this, preamble, withClosingBrace);

    public sealed class BraceScope : IDisposable
    {
        private readonly CodeWriter writer;
        private readonly String? withClosingBrace;

        public BraceScope(CodeWriter codeGenerator, String? preamble = null, String? withClosingBrace = null)
        {
            this.writer = codeGenerator;
            this.withClosingBrace = withClosingBrace;

            if (preamble != null)
            {
                writer.Line(preamble + "{");
            }
            else
            {
                writer.Line("{");
            }
            writer.Indent();
        }

        public void Dispose()
        {
            writer.Dedent();
            if (this.withClosingBrace == null)
            {
                writer.Line("}");
            }
            else
            {
                writer.Line("}}{0}", this.withClosingBrace);
            }
        }
    }
    public IDisposable Class(String modifiers, String name)
    {
        Line("{0} class {1}", modifiers, name);
        return new BraceScope(this);
    }

    public IDisposable Class(String modifiers, String name, String implements)
    {
        Line("{0} class {1} : {2}", modifiers, name, implements);
        return new BraceScope(this);
    }

    public IDisposable PartialClass(String modifiers, String name)
    {
        Line("{0} partial class {1}", modifiers, name);
        return new BraceScope(this);
    }

    public IDisposable PartialClass(String modifiers, String name, String implements)
    {
        Line("{0} partial class {1} : {2}", modifiers, name, implements);
        return new BraceScope(this);
    }

    public IDisposable PartialRecordClass(String modifiers, String name)
    {
        Line("{0} partial record class {1}", modifiers, name);
        return new BraceScope(this);
    }

    public IDisposable PartialRecordClass(String modifiers, String name, String implements)
    {
        Line("{0} partial record class {1} : {2}", modifiers, name, implements);
        return new BraceScope(this);
    }

    public IDisposable Using(String disposable)
    {
        Line("using ({0})", disposable);
        return new BraceScope(this);
    }

    public IDisposable Method(String modifiers, String returnType, String name, String args)
    {
        Line("{0} {1} {2}({3})", modifiers, returnType, name, args);
        return new BraceScope(this);
    }

    public IDisposable ExplicitInterfaceMethod(String returnType, String iface, String name, String args)
    {
        Line("{0} {1}.{2}({3})", returnType, iface, name, args);
        return new BraceScope(this);
    }

    public IDisposable While(String whileCondition)
    {
        Line(String.Format("while ({0})", whileCondition));
        return new BraceScope(this);
    }

    public IDisposable DoWhile(String whileCondition)
    {
        Line("do");
        return new BraceScope(this, preamble: null, withClosingBrace: String.Format(" while ({0});", whileCondition));
    }

    public IDisposable Switch(String switchCondition)
    {
        Line(String.Format("switch ({0})", switchCondition));
        return new BraceScope(this);
    }

    public IDisposable SwitchCase(string caseString)
    {
        Line(String.Format("case {0}:", caseString));
        return new BraceScope(this);
    }

    public IDisposable SwitchCase(string caseStringFormat, params String[] caseStringArgs)
    {
        Line(String.Format("case {0}:", String.Format(caseStringFormat, caseStringArgs)));
        return new BraceScope(this);
    }

    public IDisposable SwitchDefault()
    {
        Line("default:");
        return new BraceScope(this);
    }

    public IDisposable Constructor(String access, String typeName, String parameters = "")
    {
        Line(String.Format("{0} {1}({2})", access, typeName, parameters));
        return new BraceScope(this);
    }

    public IDisposable ForEach(String enumerable)
    {
        Line("foreach ({0})", enumerable);
        return new BraceScope(this);
    }

    public IDisposable If(String condition)
    {
        Line("if ({0})", condition);
        return new BraceScope(this);
    }

    public IDisposable If(String conditionFormat, params String[] conditionArgs)
    {
        Line("if ({0})", String.Format(conditionFormat, conditionArgs));
        return new BraceScope(this);
    }

    public IDisposable ElseIf(String condition)
    {
        Line(String.Format("else if ({0})", condition));
        return new BraceScope(this);
    }

    public IDisposable ElseIf(String conditionFormat, params String[] conditionArgs)
    {
        Line(String.Format("else if ({0})", String.Format(conditionFormat, conditionArgs)));
        return new BraceScope(this);
    }

    public IDisposable Else()
    {
        Line("else");
        return new BraceScope(this);
    }

    public void Return(string returnValue)
    {
        Line("return {0};", returnValue);
    }
}
