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
// Modified On:  2020/03/12 13:10
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace SuperMemoAssistant.Plugins.LaTeX
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
  [Form(Mode = DefaultFields.None)]
  [Title("LaTeX Settings",
         IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
                "Cancel",
                IsCancel = true)]
  [DialogAction("save",
                "Save",
                IsDefault = true,
                Validates = true)]
  public class LaTeXCfg : CfgBase<LaTeXCfg>, INotifyPropertyChangedEx
  {
    #region Properties & Fields - Non-Public

    [JsonIgnore] private Dictionary<Regex, LaTeXTag> _filters = null;

    #endregion




    #region Properties & Fields - Public

    [JsonIgnore]
    public Dictionary<Regex, LaTeXTag> Filters => _filters ?? GenerateTagsRegex();

    public List<string>                 DviGenerationCmd   { get; set; }
    public List<string>                 ImageGenerationCmd { get; set; }
    public Dictionary<string, LaTeXTag> Tags               { get; set; }

    [JsonIgnore]
    [Field(Name = "(Step 1) DVI Generation command line")]
    [MultiLine]
    public string DviGenerationCmdConfig
    {
      get => string.Join("\n", DviGenerationCmd);
      set => DviGenerationCmd = value.Replace("\r\n", "\n").Split('\n').ToList();
    }

    [JsonIgnore]
    [Field(Name = "(Step 2) Image Generation command line")]
    public string ImageGenerationCmdConfig
    {
      get => string.Join("\n", ImageGenerationCmd);
      set => ImageGenerationCmd = value.Replace("\r\n", "\n").Split('\n').ToList();
    }

    [Field(Name = "(Step 3) HTML LaTeX <img> tag")]
    public string LaTeXImageTag { get; set; } = LaTeXConst.Html.LaTeXImagePng;

    [Field]
    [Value(Must.BeGreaterThan, 0, StrictValidation = true)]
    public int ExecutionTimeout { get; set; } = 3000;

    #endregion




    #region Properties Impl - Public

    [JsonIgnore]
    public bool IsChanged { get; set; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return "LaTeX";
    }

    #endregion




    #region Methods

    public bool IsValid()
    {
      return Tags != null && Tags.Count > 0
        && DviGenerationCmd != null && DviGenerationCmd.Count > 0
        && ImageGenerationCmd != null && ImageGenerationCmd.Count > 0
        && ExecutionTimeout > 0;
    }

    public Dictionary<Regex, LaTeXTag> GenerateTagsRegex()
    {
      Dictionary<Regex, LaTeXTag> ret = new Dictionary<Regex, LaTeXTag>();

      foreach (var tag in Tags.Values)
      {
        string escaped = Regex.Escape(tag.TagBegin) + "(.+?)" + Regex.Escape(tag.TagEnd);

        ret[new Regex(escaped,
                      RegexOptions.Compiled | RegexOptions.IgnoreCase)] = tag;
      }

      return _filters = ret;
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
