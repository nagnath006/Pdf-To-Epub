using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace PdfToHtmlJS
{
    public class CssParser
    {
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
                                     where element.Contains(searchStyle)
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
        /// 
        /// </summary>
        /// <param name="cssContent"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string RemoveCssClassByName(string cssContent, string className)
        {
            String styleText = string.Empty;
            if (!string.IsNullOrEmpty(cssContent))
            {
                string[] parts = cssContent.Split('}');

                if (parts != null && parts.Length > 0)
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!parts[i].ToLower().Contains(className.ToLower()))
                        {
                            styleText = styleText + parts[i] + "}" ;
                           
                        }
                        else
                        { 
                        
                        }
                    }
                    return styleText.ToString();
                }
            }
            return cssContent;
        }

        //public static string CheckWellFormed(string parts)
        //{
        //    string[] things = parts.Split('}');
        //    var stack = new Stack<char>();
        //    // dictionary of matching starting and ending pairs
        //    var allowedChars = new Dictionary<char, char>() { { '(', ')' }, { '[', ']' }, { '{', '}' } };

        //    var wellFormated = true;
        //    foreach (var chr in parts)
        //    {
        //        if (allowedChars.ContainsKey(chr))
        //        {
        //            
        //            stack.Push(chr);
        //        }
        //        
        //        else if (allowedChars.ContainsValue(chr))
        //        {
        //            // make sure something to pop if not then know it's not well formated
        //            wellFormated = stack.Any();
        //            if (wellFormated)
        //            {
        //                r
        //                var startingChar = stack.Pop();
        //                // check it is in the dictionary
        //                wellFormated = allowedChars.Contains(new KeyValuePair<char, char>(startingChar, chr));
        //            }
        //            // if not wellformated exit loop no need to continue
        //            if (!wellFormated)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return parts;


        //}
    


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
                    var style = (from element in parts
                                 where element.Contains(searchStyle)
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

        public static List<ElementDimension> GetPositionByCSS(string cssContent)
        {
            List<ElementDimension> lstDimensions = new List<ElementDimension>();
            if (!string.IsNullOrEmpty(cssContent))
            {
                string[] parts = cssContent.Split('}');
                if (parts != null && parts.Length > 0)
                {

                    foreach (string part in parts)
                    {
                        if (part.Split('{').Length == 2)
                        {
                            string className = part.Split('{')[0].Trim();
                            string styleText = CleanUp(part.Split('{')[1]).Trim().ToLower();
                            if (styleText.IndexOf("left:") > -1 || styleText.IndexOf("top:") > -1 || styleText.IndexOf("bottom:") > -1)
                            {
                                double left = 0.0, top = 0.0, bottom = 0.0;
                                var splittedStyle = styleText.Split(';');
                                foreach (var s in splittedStyle)
                                {
                                    if (s.Split(':')[0].Trim().Equals("left"))
                                    {
                                        left = Convert.ToDouble(s.Split(':')[1].Replace("px", ""));

                                    }
                                    if (s.Split(':')[0].Trim().Equals("top"))
                                    {
                                        top = Convert.ToDouble(s.Split(':')[1].Replace("px", ""));

                                    }
                                    if (s.Split(':')[0].Trim().Equals("bottom"))
                                    {
                                        bottom = Convert.ToDouble(s.Split(':')[1].Replace("px", ""));

                                    }
                                }
                                lstDimensions.Add(new ElementDimension { ClassName = className, Left = left, Top = top, Bottom = bottom });

                            }
                        }
                    }
                }

            }
            return lstDimensions;
        }
        public static double GetPageTransformScaleValueByClass(string cssTransformContent)
        {
            if (!string.IsNullOrEmpty(cssTransformContent))
            {
                cssTransformContent = cssTransformContent.Replace(";", ";\r\n");
                const string scaleSearchString = "transform:scale(";
                using (var reader = new StringReader(cssTransformContent))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains(scaleSearchString))
                        {
                            var scaleValues = line.Split(new string[] { scaleSearchString }, StringSplitOptions.None);
                            if (scaleValues.Length > 0)
                                return Convert.ToDouble(scaleValues[scaleValues.Length - 1].Replace(");", ""));
                        }
                    }
                }
            }
            return 1.0D;
        }

        private static string GetStyle(string style)
        {
            string[] parts = style.Split('{');
            return CleanUp(parts[1]).Trim().ToLower();
        }

        public static string CleanUp(string s)
        {
            string temp = s;
            string reg = "(/\\*(.|[\r\n])*?\\*/)";
            Regex r = new Regex(reg);
            temp = r.Replace(temp, "");
            temp = temp.Replace("\r", "").Replace("\n", "");
            return temp;
        }

        public static void AddElementIntoCSS(string cssPath, string css)
        {
            string styleAttributes = string.Empty;
            if (!string.IsNullOrEmpty(cssPath) && File.Exists(cssPath))
            {
                string cssContent = (File.ReadAllText(cssPath));
                cssContent += css;

                File.WriteAllText(cssPath, cssContent);
            }
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

                    if (!CssDict.ContainsKey(className))
                    {
                        CssDict.Add(className, styleText);
                    }
                }


            }
            return CssDict;
        }
    }

    public class ElementDimension
    {
        public string ClassName { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
    }
}


