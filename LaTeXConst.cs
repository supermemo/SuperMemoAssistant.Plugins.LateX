#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2018/05/31 17:47
// Modified On:  2019/01/14 19:45
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Text.RegularExpressions;
using SuperMemoAssistant.Extensions;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace SuperMemoAssistant.Plugins.LaTeX
{
  public static class LaTeXConst
  {
    #region Constants & Statics

    public static LaTeXCfg Default => new LaTeXCfg
    {
      DviGenerationCmd   = LaTeXCommands.LaTeXCmdTemplate,
      ImageGenerationCmd = LaTeXCommands.DviPngCmdTemplate,
      Tags = new Dictionary<string, LaTeXTag>
      {
        { "Full", LaTeXDocument.FullTag },
        { "Expression", LaTeXDocument.ExpTag },
        { "Maths", LaTeXDocument.MathsTag }
      }
    };

    #endregion




    public static class LaTeXCommands
    {
      #region Constants & Statics

      public static List<string> LaTeXCmdTemplate => new List<string> { "latex", "-interaction=nonstopmode", "{inTex}" };
      public static List<string> DviPngCmdTemplate => new List<string>
        { "dvipng", "-D", "200", "-T", "tight", "{inDvi}", "-o", "{outImg}.png" };

      #endregion
    }


    public static class LaTeXDocument
    {
      #region Constants & Statics

      public static LaTeXTag FullTag => new LaTeXTag
      {
        LaTeXBegin = @"\documentclass[12pt]{article}
\special{papersize=3in,5in}
\usepackage[utf8]{inputenc}
\usepackage{amssymb,amsmath}
\pagestyle{empty}
\setlength{\parindent}{0in}
\begin{document}
",
        LaTeXEnd = @"
\end{document}",
        TagBegin = "[latex]",
        TagEnd   = "[/latex]"
      };
      public static LaTeXTag ExpTag => new LaTeXTag
      {
        LaTeXBegin = @"\documentclass[12pt]{article}
\special{papersize=3in,5in}
\usepackage[utf8]{inputenc}
\usepackage{amssymb,amsmath}
\pagestyle{empty}
\setlength{\parindent}{0in}
\begin{document}
$
",
        LaTeXEnd = @"
$
\end{document}",
        TagBegin = "[$]",
        TagEnd   = "[/$]"
      };
      public static LaTeXTag MathsTag => new LaTeXTag
      {
        LaTeXBegin = @"\documentclass[12pt]{article}
\special{papersize=3in,5in}
\usepackage[utf8]{inputenc}
\usepackage{amssymb,amsmath}
\pagestyle{empty}
\setlength{\parindent}{0in}
\begin{document}
\begin{displaymath}
",
        LaTeXEnd = @"
\end{displaymath}
\end{document}",
        TagBegin = "[$$]",
        TagEnd   = "[/$$]"
      };

      #endregion
    }


    public static class RE
    {
      #region Constants & Statics

      public static readonly Regex Br = new Regex("<br( \\t)*/>",
                                                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex DivOpen =
        new Regex("<div[ \\t]*>",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex DivClose =
        new Regex("<div[ \\t]*/>",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex Html =
        new Regex("<[^>]+>|&nbsp;",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

      public static readonly Regex LaTeXImage =
        new Regex(@"<img[^>]+class=""?sma-latex-img""?[^>]*>",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex LaTeXImageLaTeXCode =
        new Regex(@"data-latex-code=""?([^""]+)""?",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

      public static readonly Regex LaTeXError =
        new Regex(
          "(?:[\\s]*<br[\\s]*/>|<br[\\s]*>)*<div class=\"?sma-latex-error\"?><font color=\"?#ff0000\"?>.*?</font></div>[\\s]*(?:<br[\\s]*/>|<br[\\s]*>){0,2}",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

      public static readonly Regex SvgPxDimension =
        new Regex(
          "([\\d]+)px",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

      #endregion
    }

    public static class Html
    {
      #region Constants & Statics

      public const string LaTeXImagePng =
        @"<img class=""sma-latex-img"" width=""{0}"" height=""{1}"" style=""background-image:url('data:image/png;base64,{2}'); background-repeat: no-repeat"" data-latex-code=""{3}"" />";
      public const string LaTeXError = "\n<br />\n<br /><div class=\"sma-latex-error\"><font color=\"#ff0000\">{0}</font></div>";

      public const string CSS = @"
.sma-latex-error {
  padding: 15px;
  margin-bottom: 10px;
  border: 1px solid transparent;
  border-radius: 4px;
  color: #a94442;
  background-color: #f2dede;
  border-color: #ebccd1;
}";

      #endregion
    }


    public static class Paths
    {
      #region Constants & Statics

      public static string TempFilePath =>
        System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                               AssemblyEx.GetAssemblyGuid().ToString("D"));
      public static string TempTexFilePath =>
        TempFilePath + ".tex";
      public static string TempDviFilePath =>
        TempFilePath + ".dvi";

      #endregion
    }
  }
}
