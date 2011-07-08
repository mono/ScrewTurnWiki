//
//  Authors:
//    Marek Habersack <grendel@twistedcode.net>
//
//  (C) 2011 Marek Habersack
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted
//  provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of
//       conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of
//       conditions and the following disclaimer in the documentation and/or other materials
//       provided with the distribution.
//     * Neither the name of Marek Habersack nor the names of its contributors may be used to
//       endorse or promote products derived from this software without specific prior written
//       permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using ScrewTurn.Wiki;

namespace MediaWikiCompat
{
	public class Toc 
	{
		static readonly Regex TocRegex = new Regex (@"\{toc\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		static string GenerateToc (List <HPosition> hPos)
		{
			var sb = new StringBuilder ();

			hPos.Sort (new HPositionComparer ());

			sb.Append ("<div id=\"sidebar\"><div id=\"toc-parent\"><div id=\"toc\"><h5>Table of Contents</h5>");
			sb.AppendLine ();
			var levels = new Stack <int> ();
			levels.Push (1);
			var numbers = new Dictionary <int, int> () {
				{1, 0},
				{2, 0},
				{3, 0},
				{4, 0},
				{5, 0}
			};
			int hpLevel = 0, level, lowestLevel;
			var sectionNumber = new List <string> ();
			lowestLevel = hPos [0].Level;
			
			foreach (HPosition hp in hPos) {
				hpLevel = hp.Level;
				if (levels.Peek () != hpLevel) {
					level = levels.Pop ();
					if (level > lowestLevel)
						sb.Append ("</p>");
					if (level > hpLevel) {
						for (int i = level; i > hpLevel; i--)
							sb.Append ("</div>");
						
						if (hpLevel > lowestLevel)
							sb.Append ("<p>");
					} else if (hpLevel > lowestLevel) {
						for (int i = level; i < hpLevel; i++)
							sb.Append ("<div class=\"tocindent\"><p>");
					}
					levels.Push (hpLevel);
				}

				if (numbers.ContainsKey (hpLevel))
					numbers [hpLevel]++;
				else
					numbers [hpLevel] = 1;
				
				for (int i = hpLevel + 1; i < 6; i++)
					numbers [i] = 0;
				
				if (hpLevel == lowestLevel)
					sb.Append ("<div class=\"tocline\">");

				sb.Append ("<span class=\"number\">");
				bool skipLeadingZeros = true;
				sectionNumber.Clear ();
				for (int i = lowestLevel; i <= hpLevel; i++) {
					if (skipLeadingZeros) {
						if (numbers [i] == 0)
							continue;
						skipLeadingZeros = false;
					}
					sectionNumber.Add (numbers [i].ToString ());
				}
				if (sectionNumber.Count > 0)
					sb.Append (String.Join (".", sectionNumber.ToArray ()));
				
				sb.Append ("</span> ");
				
				sb.Append ("<a href=\"#");
				sb.Append (Formatter.BuildHAnchor (hp.Text, hp.ID.ToString ()));
				sb.Append ("\">");
				sb.Append (StripWikiMarkup (Formatter.StripHtml (hp.Text)));
				sb.Append ("</a><br/>");
				if (hpLevel == lowestLevel)
					sb.Append ("</div>");
			}
			if (hpLevel != lowestLevel)
				sb.Append ("</div>");
			
			sb.Append ("</div></div></div>");
			return sb.ToString ();
		}
		
		public static string Parse (string page)
		{
			if (String.IsNullOrEmpty (page))
				return page;

			Match match = TocRegex.Match (page);
			if (!match.Success)
				return page;
			
			List <HPosition> hPos = Formatter.DetectHeaders (page);
			string toc;
			if (hPos == null || hPos.Count == 0)
				toc = String.Empty;
			else
				toc = GenerateToc (hPos);
			
			return page.Replace (match.Value, toc);
		}

		static string StripWikiMarkup (string content)
		{
                        if (String.IsNullOrEmpty (content))
				return String.Empty;

                        StringBuilder sb = new StringBuilder (content);
                        sb.Replace ("*", "");
                        sb.Replace ("<", "");
                        sb.Replace (">", "");
                        sb.Replace ("[", "");
                        sb.Replace ("]", "");
                        sb.Replace ("{", "");
                        sb.Replace ("}", "");
                        sb.Replace ("'''", "");
                        sb.Replace ("''", "");
                        sb.Replace ("=====", "");
                        sb.Replace ("====", "");
                        sb.Replace ("===", "");
                        sb.Replace ("==", "");
                        sb.Replace ("<A7><A7>", "");
                        sb.Replace ("__", "");
                        sb.Replace ("--", "");
                        sb.Replace ("@@", "");

                        return sb.ToString ();
                }
	}
}
