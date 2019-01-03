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
// Created On:   2018/05/31 19:22
// Modified On:  2018/06/03 01:33
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SuperMemoAssistant.Plugins.LateX
{
  public class LateXCfg
  {
    #region Properties & Fields - Public
    
    [JsonIgnore]
    private Dictionary<Regex, LateXTag> _filters = null;
    [JsonIgnore]
    public Dictionary<Regex, LateXTag> Filters => _filters ?? (_filters = GenerateTagsRegex());

    public List<string>                 DviGenerationCmd   { get; set; }
    public List<string>                 ImageGenerationCmd { get; set; }
    public Dictionary<string, LateXTag> Tags               { get; set; }

    #endregion




    #region Methods

    public bool IsValid()
    {
      return Tags != null && Tags.Count > 0
        && DviGenerationCmd != null && DviGenerationCmd.Count > 0
        && ImageGenerationCmd != null && ImageGenerationCmd.Count > 0;
    }

    private Dictionary<Regex, LateXTag> GenerateTagsRegex()
    {
      Dictionary<Regex, LateXTag> ret = new Dictionary<Regex, LateXTag>();

      foreach (var tag in Tags.Values)
      {
        string escaped = Regex.Escape(tag.TagBegin) + "(.+?)" + Regex.Escape(tag.TagEnd);

        ret[new Regex(escaped, RegexOptions.Compiled | RegexOptions.IgnoreCase)] = tag;
      }

      return ret;
    }

    #endregion
  }
}
