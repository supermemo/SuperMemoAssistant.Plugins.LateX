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
// Created On:   2018/05/30 17:20
// Modified On:  2019/01/14 21:12
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Windows.Input;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.ComponentModel;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Plugins.LaTeX
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  public class LaTeXPlugin : SMAPluginBase<LaTeXPlugin>
  {
    #region Constructors

    public LaTeXPlugin() { }

    #endregion




    #region Properties & Fields - Public

    public LaTeXCfg Config { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "LaTeX";

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void OnInit()
    {
      LoadConfigOrDefault();
      SettingsModels = new List<INotifyPropertyChangedEx>
      {
        Config
      };

      Svc.KeyboardHotKey.RegisterHotKey(
        new HotKey(true,
                   true,
                   false,
                   false,
                   Key.L,
                   "LaTeX: Convert LaTeX to Image"),
        ConvertLaTeXToImages);
      Svc.KeyboardHotKey.RegisterHotKey(
        new HotKey(true,
                   true,
                   true,
                   false,
                   Key.L,
                   "LaTeX: Convert Image to LaTeX"),
        ConvertImagesToLaTeX);
    }


    public override void SettingsSaved(object _)
    {
      Svc<LaTeXPlugin>.Configuration.Save<LaTeXCfg>(Config).Wait();

      Config.GenerateTagsRegex();
    }

    #endregion




    #region Methods

    private void ConvertLaTeXToImages()
    {
      var (texDoc, htmlDoc) = GetDocuments();

      if (texDoc == null || htmlDoc == null)
        return;

      htmlDoc.Text = texDoc.ConvertLaTeXToImages();
    }

    private void ConvertImagesToLaTeX()
    {
      var (texDoc, htmlDoc) = GetDocuments();

      if (texDoc == null || htmlDoc == null)
        return;

      htmlDoc.Text = texDoc.ConvertImagesToLaTeX();
    }

    private (LaTeXDocument texDoc, IControlHtml ctrlHtml) GetDocuments()
    {
      IControlHtml ctrlHtml = Svc.SMA.UI.ElementWindow.ControlGroup.FocusedControl.AsHtml();

      if (ctrlHtml == null)
        return (null, null);

      string html = ctrlHtml.Text ?? string.Empty;

      var texDoc = new LaTeXDocument(Config,
                                     html);

      return (texDoc, ctrlHtml);
    }

    private void LoadConfigOrDefault()
    {
      Config = Svc<LaTeXPlugin>.Configuration.Load<LaTeXCfg>().Result;

      if (Config == null || Config.IsValid() == false)
      {
        Config = LaTeXConst.Default;

        Svc<LaTeXPlugin>.Configuration.Save(Config);
      }
    }

    #endregion
  }
}
