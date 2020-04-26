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
// Modified On:  2019/02/28 20:52
// Modified By:  Alexis

#endregion




using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Input;
using Anotar.Serilog;
using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Plugins.LaTeX
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  public class LaTeXPlugin : SentrySMAPluginBase<LaTeXPlugin>
  {
    #region Constructors

    public LaTeXPlugin() : base("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046") { }

    #endregion




    #region Properties & Fields - Public

    public LaTeXCfg Config { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "LaTeX";

    public override bool HasSettings => true;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void PluginInit()
    {
      LoadConfigOrDefault();

      Svc.HotKeyManager
         .RegisterGlobal(
           "LaTeXToImage",
           "Convert LaTeX to Image",
           HotKeyScopes.SMBrowser,
           new HotKey(Key.L, KeyModifiers.CtrlAlt),
           ConvertLaTeXToImage
         )
         .RegisterGlobal(
           "ImageToLaTeX",
           "Convert Image to LaTeX",
           HotKeyScopes.SMBrowser,
           new HotKey(Key.L, KeyModifiers.CtrlAltShift),
           ConvertImageToLaTeX
         );
    }
    
    /// <inheritdoc />
    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }

    #endregion




    #region Methods

    private void ConvertLaTeXToImage()
    {
      try
      {
        var (texDoc, htmlDoc) = GetDocuments();

        if (texDoc == null || htmlDoc == null)
          return;

        htmlDoc.Text = texDoc.ConvertLaTeXToImages();
      }
      catch (RemotingException ex)
      {
        LogTo.Warning(ex, "ConvertLaTeXToImage failed.");
      }
    }

    private void ConvertImageToLaTeX()
    {
      try
      {
        var (texDoc, htmlDoc) = GetDocuments();

        if (texDoc == null || htmlDoc == null)
          return;

        htmlDoc.Text = texDoc.ConvertImagesToLaTeX();
      }
      catch (RemotingException ex)
      {
        LogTo.Warning(ex, "ConvertImageToLaTeX failed.");
      }
    }

    private (LaTeXDocument texDoc, IControlHtml ctrlHtml) GetDocuments()
    {
      IControlHtml ctrlHtml = Svc.SM.UI.ElementWdw.ControlGroup.FocusedControl.AsHtml();

      if (ctrlHtml == null)
        return (null, null);

      var html = ctrlHtml.Text ?? string.Empty;
      var htmlDoc = ctrlHtml.GetDocument();
      var htmlSelObj = htmlDoc?.selection;
      string htmlSel = null;

      if (htmlSelObj?.createRange() is IHTMLTxtRange textSel)
        htmlSel = textSel.text;

      var texDoc = new LaTeXDocument(Config, html, htmlSel);

      return (texDoc, ctrlHtml);
    }

    private void LoadConfigOrDefault()
    {
      Config = Svc.Configuration.Load<LaTeXCfg>();

      if (Config == null || Config.IsValid() == false)
      {
        Config = LaTeXConst.Default;

        //Svc.Configuration.Save(Config).ConfigureAwait(false);
      }
    }

    #endregion
  }
}
