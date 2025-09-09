﻿using System.Text.RegularExpressions;

namespace BlogApp.Helpers
{
    public static class HtmlTagHelper
    {
        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>|&.*?;", string.Empty); // This removes any HTML tags.
        }
    }
}
