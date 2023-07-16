﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjParserExecutor
{
    public class SvgConverter
    {
        public string Convert(IEnumerable<IEnumerable<IEnumerable<PointF>>> objectLoopsPoints)
        {

            var svgGroups = objectLoopsPoints.Select(curvePoints =>
            {
                var pathCoords = curvePoints.Select(points =>
                    string.Join(" ",
                        points.Select(p => $"{p.X.ToString(new CultureInfo("en-US", false))} {p.Y.ToString(new CultureInfo("en-US", false))}")));

                var pathes = pathCoords.Select(pc => $@"<path d=""M {pc} z"" style=""fill:none;stroke-width:0.264583;stroke:#000000;"" />");
                var pathesString = string.Join("\r\n", pathes);
                return @$"<g>
                            {pathesString}
                          </g>";
            });

            var svgGroupsString = string.Join("\r\n", svgGroups);

            var body = @$"
            <?xml version=""1.0"" standalone=""no""?>
            <svg width=""20cm"" height=""20cm"" viewBox=""0 0 400 400"" xmlns=""http://www.w3.org/2000/svg"" version=""1.1"">
              <title>Example triangle01- simple example of a 'path'</title>
              <desc>A path that draws a triangle</desc>
              {svgGroupsString}
            </svg>
            ";

            return body;
        }
    }
}