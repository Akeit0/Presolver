#region

using System.Text;

#endregion

namespace Presolver.Generator;

public class CodeWriter
{
    const string INDENT = "    ";
    static readonly int indentSpace = INDENT.Length;

    readonly StringBuilder _text = new();

    public CodeWriter(int initialIndent)
    {
        Indent = initialIndent;
        BeginLine();
    }

    public int Indent { get; set; }

    public void Append(string str)
    {
        _text.Append(str);
    }

    public void Append(char c)
    {
        _text.Append(c);
    }

    public void Append(int c)
    {
        _text.Append(c);
    }

    public void AppendIndented(string str)
    {
        _text.Append(INDENT);
        _text.Append(str);
    }

    public void BeginBlock()
    {
        Indent++;
        AppendLine('{');
    }

    public void EndBlock()
    {
        Indent--;
        _text.Remove(_text.Length - (indentSpace + 1), indentSpace);
        if (Indent == 0) _text.AppendLine();
        AppendLine('}');
    }


    public void AppendLine(char c)
    {
        _text.Append(c);
        _text.AppendLine();
        BeginLine();
    }

    public void AppendLineIndented(string str)
    {
        _text.Append(INDENT);
        _text.AppendLine(str);
        BeginLine();
    }

    public void AppendLine(string str)
    {
        _text.AppendLine(str);
        BeginLine();
    }


    void BeginLine()
    {
        for (var i = 0; i < Indent; i++) _text.Append(INDENT);
    }

    public override string ToString()
    {
        return _text.ToString();
    }

    public CodeWriter GetSubIndenter(int indent = 1)
    {
        return new(Indent + indent);
    }

    public void Append(CodeWriter subIndenter)
    {
        //Debug.Assert(subIndenter._indent == _indent);
        _text.Remove(_text.Length - Indent * 4, Indent * 4);
        _text.Append(subIndenter._text);
    }

    public void Clear()
    {
        _text.Clear();
        Indent = 0;
    }
}