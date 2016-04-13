using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace PdfToHtmlJS
{
    /// <summary>
    /// Contains multiple methods to Parse CSS file
    /// </summary>
    public class CssParserold
    {
        #region Public methods
        /// <summary>
        /// Get element style 
        /// </summary>
        /// <param name="cssPath">CSS File Path</param>
        /// <param name="searchStyle">Style keyword to be searched</param>
        /// <returns></returns>
        public static string GetElementStyle(string cssPath, string searchStyle)
        {
            string styleAttributes = string.Empty;
            if (!string.IsNullOrEmpty(cssPath) && File.Exists(cssPath) && !string.IsNullOrEmpty(searchStyle))
            {
                string content = CleanUp(File.ReadAllText(cssPath));
                if (!string.IsNullOrEmpty(content))
                {
                    string[] parts = content.Split('}');
                    if (parts != null && parts.Length > 0)
                    {
                        var style = (from element in parts
                                     where !string.IsNullOrEmpty(element) && element.Split('{')[0].Trim().Substring(1).Equals(searchStyle)
                                     select element).FirstOrDefault();
                        if (!string.IsNullOrEmpty(style))
                        {
                            if (CleanUp(style).IndexOf('{') > -1)
                            {
                                styleAttributes = GetStyle(style);
                            }
                        }
                    }
                }
            }
            return styleAttributes;
        }

        /// <summary>
        /// Get matching font family style
        /// </summary>
        /// <param name="cssPath"></param>
        /// <param name="searchStyle"></param>
        /// <param name="fontFamilyName"></param>
        /// <returns></returns>
        public static string GetMatchingFontFamilyElementStyle(string cssContent, string searchStyle, string fontFamilyName)
        {
            string styleAttributes = string.Empty;
            if (!string.IsNullOrEmpty(cssContent) && !string.IsNullOrEmpty(searchStyle))
            {
                string[] parts = cssContent.Split('}');
                if (parts != null && parts.Length > 0)
                {
                    var styles = (from element in parts
                                  where !string.IsNullOrEmpty(element) && element.Split('{')[0].Trim().Substring(1).Equals(searchStyle)
                                  select element);
                    foreach (var style in styles)
                    {
                        if (!string.IsNullOrEmpty(style))
                        {
                            if (CleanUp(style).IndexOf('{') > -1)
                            {
                                styleAttributes = GetStyle(style);
                                string[] attributes = styleAttributes.Split(';');
                                foreach (var atr in attributes)
                                {
                                    if (atr.Contains("font-family"))
                                    {
                                        string fontName = atr.Split(':')[1];
                                        fontName = Regex.Replace(fontName, @"\""", "");
                                        if (fontName.ToLower().Equals(fontFamilyName.ToLower()))
                                        {
                                            return styleAttributes;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return styleAttributes;
        }
      
        /// <summary>
        /// Get Font Family collection
        /// </summary>
        /// <param name="cssContent">CSS Text</param>
        /// <param name="searchStyle">Search Style keyword</param>
        /// <param name="fontFamilyName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFontFamilyElements(string cssContent, string searchStyle)
        {
            string styleAttributes = string.Empty;
            var dictionayStyleAttributes = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(cssContent) && !string.IsNullOrEmpty(searchStyle))
            {
                string[] parts = cssContent.Split('}');
                if (parts != null && parts.Length > 0)
                {
                    var styles = (from element in parts
                                  where !string.IsNullOrEmpty(element) && element.Split('{')[0].Trim().Substring(1).Equals(searchStyle)
                                  select element);
                    foreach (var style in styles)
                    {
                        if (!string.IsNullOrEmpty(style))
                        {
                            if (CleanUp(style).IndexOf('{') > -1)
                            {
                                styleAttributes = GetStyle(style);
                                string[] attributes = styleAttributes.Split(';');
                                foreach (var atr in attributes)
                                {
                                    if (atr.Contains("font-family"))
                                    {
                                        string fontName = atr.Split(':')[1];
                                        fontName = Regex.Replace(fontName, @"\""", "");
                                        if (!dictionayStyleAttributes.ContainsKey(fontName.ToLower()))
                                            dictionayStyleAttributes.Add(fontName.ToLower(), styleAttributes);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return dictionayStyleAttributes;
        }
        
        /// <summary>
        /// Remove element from CSS
        /// </summary>
        /// <param name="cssPath"></param>
        /// <param name="searchStyle"></param>
        public static void RemoveElementFromCSS(string cssPath, string searchStyle)
        {
            string styleAttributes = string.Empty;
            if (!string.IsNullOrEmpty(cssPath) && File.Exists(cssPath) && !string.IsNullOrEmpty(searchStyle))
            {
                string cssContent = CleanUp(File.ReadAllText(cssPath));
                if (!string.IsNullOrEmpty(cssContent))
                {
                    string[] parts = cssContent.Split('}');
                    if (parts != null && parts.Length > 0)
                    {
                        var style = (from element in parts
                                     where !string.IsNullOrEmpty(element) && element.Split('{')[0].Trim().Substring(1).Equals(searchStyle)
                                     select element).FirstOrDefault();
                        if (!string.IsNullOrEmpty(style))
                        {
                            if (CleanUp(style).IndexOf('{') > -1)
                            {
                                if (!style.Contains("}")) style = string.Concat(style, "}");
                                cssContent = cssContent.Replace(style, "").Replace("{", "{\n\t").Replace("}", "\n}\n"); ;
                                File.WriteAllText(cssPath, cssContent);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get element style by csscontent
        /// </summary>
        /// <param name="cssContent"></param>
        /// <param name="searchStyle"></param>
        /// <returns></returns>
        public static string GetElementStyleByCSS(string cssContent, string searchStyle)
        {
            string styleAttributes = string.Empty;
            if (!string.IsNullOrEmpty(cssContent))
                {
                    string[] parts = cssContent.Split('}');
                    if (parts != null && parts.Length > 0)
                    {
                        //TODO: To do with best approach (might be minify)
                        var style = (from element in parts
                                     where !string.IsNullOrEmpty(element) && element.Split('{')[0].Trim().Substring(1).Equals(searchStyle)
                                     select element).FirstOrDefault();
                        if (!string.IsNullOrEmpty(style))
                        {
                            if (CleanUp(style).IndexOf('{') > -1)
                            {
                                styleAttributes = GetStyle(style);
                            }
                        }
                    }
                }
            
            return styleAttributes;
        }

        /// <summary>
        /// Clean extra space , tab and enter
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string CleanUp(string s)
        {
            string temp = s;
            string reg = "(/\\*(.|[\r\n])*?\\*/)";
            Regex r = new Regex(reg);
            temp = r.Replace(temp, "");
            temp = temp.Replace("\r", "").Replace("\n", "");
            return temp;
        }

        /// <summary>
        /// To parse css and return a key value pair collection.
        /// </summary>
        /// <param name="CssString">CSS text</param>
        /// <returns>Dictionary object with class and their values.</returns>
        public static Dictionary<string, string> ParseCSS(string CssString)
        {
            Dictionary<string, string> CssDict = new Dictionary<string, string>();
            string cssContent = CleanUp(CssString);
            string[] parts = cssContent.Split('}');
            foreach (string s in parts)
            {
                if (s.IndexOf('{') > 0)
                {
                    string className = s.Substring(0, s.IndexOf('{'));
                    string styleText = s.Substring(s.IndexOf('{') + 1).Trim();
                    if (className.Contains("@font-face"))
                    {
                        string key = styleText.Split(';')[0].Split(':')[1];
                        if (!CssDict.ContainsKey(key))
                        {
                            CssDict.Add(key, styleText);
                        }
                    }
                    else
                    {
                        if (!CssDict.ContainsKey(className))
                        {
                            CssDict.Add(className, styleText);
                        }
                    }

                }
            }
            return CssDict;

        }
        
        #endregion

        #region Private methods

        private static string GetStyle(string style)
        {
            string[] parts = style.Split('{');
            return CleanUp(parts[1]).Trim().ToLower();
        }

        #endregion
    }

   
}
