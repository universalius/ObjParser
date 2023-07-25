﻿using ObjParser;
using Plain3DObjectsToSvgConverter.Models;
using SvgLib;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Plain3DObjectsToSvgConverter
{
    public class ObjectsLabelsToSvgConverter
    {
        private CultureInfo culture = new CultureInfo("en-US", false);
        private readonly IEnumerable<SvgPath> _svgLetters;
        private readonly double _letterWidth;
        private readonly double _letterHeight;

        public ObjectsLabelsToSvgConverter()
        {
            var mainFolder = AppDomain.CurrentDomain.BaseDirectory;

            SvgDocument svgDocument = ParseSvgFile(Path.Combine(mainFolder, "Asserts\\Letters.svg"));
            var pathElements = svgDocument.Element.GetElementsByTagName("path").Cast<XmlElement>().ToArray();
            _svgLetters = pathElements.Select(e => new SvgPath(e));
            _letterHeight = GetOrHeight();
            _letterWidth = GetUnderscoreWidth();
        }

        public async Task<string> Convert()
        {
            SvgDocument svgDocument = ParseSvgFile(@"D:\Виталик\Cat_Hack\Svg\test_compacted.svg");
            var groupElements = svgDocument.Element.GetElementsByTagName("g").Cast<XmlElement>().ToArray();

            foreach (var element in groupElements)
            {
                var group = new SvgGroup(element);
                if (group.Transform != "translate(0 0)")
                {
                    var pathes = group.Element.GetElementsByTagName("path").Cast<XmlElement>()
                    .Select(pe => new SvgPath(pe));

                    var path = pathes.First(p => p.GetClasses().Contains("main"));
                    var label = GetLabel(path.Id);

                    var labelGroup = group.AddGroup();
                    labelGroup.Transform = GetLabelGroupTransform(path, group.GetTransformRotate());

                    await AddLabelToGroup(label, labelGroup);
                }
            }

            return svgDocument._document.OuterXml;
        }

        private string GetLabel(string pathId)
        {
            var idSections = pathId.Split("-")[0].Split("_");
            var firstPart = idSections[0];

            int secondNumber;
            var secondPart = string.Empty;
            if (idSections.Count() > 1)
            {
                secondPart = int.TryParse(idSections[1], out secondNumber) ?
                    (secondNumber > 10 ? secondNumber.ToString() : string.Empty) : string.Empty;
            }


            var label = $"{firstPart}{(string.IsNullOrEmpty(secondPart) ? string.Empty : $"_{secondPart}")}";
            return label;
        }

        private async Task AddLabelToGroup(string label, SvgGroup group)
        {
            //int spacesCount = 0;
            int i = 0;
            label.ToList().ForEach(c =>
            {
                var s = c.ToString();
                if (s != " ")
                {
                    var shiftByX = i * _letterWidth * 0.8;

                    var path = group.AddPath();
                    path.D = _svgLetters.FirstOrDefault(p => p.Id == s).D;
                    path.Fill = "#000000";
                    path.Transform = $"translate({shiftByX.ToString(culture)})";
                }

                i++;
                //labelPathes.Add(path);
            });
        }

        private Extent GetLabelCoords(SvgPath path)
        {
            var pointsString = new Regex("[mz]").Replace(path.D.ToLowerInvariant(), string.Empty).Trim().Split("l");
            var points = pointsString.Select((s, i) =>
            {
                var points = s.Trim().Split(" ");
                return new PointD(double.Parse(points[0], culture), double.Parse(points[1], culture));
            }).ToList();

            return new Extent
            {
                XMin = points.Min(p => p.X),
                XMax = points.Max(p => p.X),
                YMin = points.Min(p => p.Y),
                YMax = points.Max(p => p.Y)
            };
        }

        private string GetLabelGroupTransform(SvgPath path, string parentGroupRotate)
        {
            var leftTopPoint = GetLabelCoords(path);
            var rotate = int.Parse(parentGroupRotate);
            var shift = _letterHeight * 1.2;
            if (rotate == 0)
            {
                leftTopPoint.YMin -= shift;
            }

            if (rotate == 90)
            {
                leftTopPoint.XMin -= shift;
                leftTopPoint.YMin += leftTopPoint.YSize;
            }

            if (rotate == 180)
            {
                leftTopPoint.XMin += leftTopPoint.XSize;
                leftTopPoint.YMin += leftTopPoint.YSize + shift;
            }

            if (rotate == 270)
            {
                leftTopPoint.XMin += (leftTopPoint.XSize + shift);
            }

            return $"translate({leftTopPoint.XMin.ToString(culture)} {leftTopPoint.YMin.ToString(culture)}) rotate({-rotate})";
        }

        private SvgDocument ParseSvgFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(content);

            return new SvgDocument(xmlDocument, xmlDocument.DocumentElement);
        }

        private double GetUnderscoreWidth()
        {
            var undescorePath = _svgLetters.First(p => p.Id == "_");
            var d = undescorePath.D.ToLower();
            var marker = "h ";
            var index = d.IndexOf(marker);
            var first = index + marker.Count();
            var last = d.IndexOf(" ", first);
            return double.Parse(d.Substring(first, last - first), culture);

        }

        private double GetOrHeight()
        {
            var orPath = _svgLetters.First(p => p.Id == "|");
            var d = orPath.D.ToLower();
            var marker = "v ";
            var index = d.LastIndexOf(marker);
            var first = index + marker.Count();
            var last = d.IndexOf(" z", first);
            return double.Parse(d.Substring(first, last - first), culture);
        }
    }
}