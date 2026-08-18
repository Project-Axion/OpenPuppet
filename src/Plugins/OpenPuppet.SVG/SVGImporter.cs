using OpenPuppet.SDK.vector.ColorSamplers;
using OpenPuppet.vector;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenPuppet.SVG
{
    public static class SVGImporter
    {
        static Dictionary<string, Func<XElement, RectangleF, ColoredVectorComponent[]>> Importers = new() 
        {
            {"rect",RectImporter},
            {"circle",CircleImporter},
            {"ellipse",EllipseImporter},
        };

        public static UnifiedVector ImportSVG(string path)
        {
            if (!File.Exists(path))
                throw new Exception($"SVG file {path} does not exist");

            var doc = XDocument.Parse(File.ReadAllText(path));

            if (doc.Root == null || doc.Root.Name.LocalName.ToLowerInvariant() != "svg")
                throw new Exception($"Invalid SVG file {path}");

            var viewbox = new RectangleF();

            var vbelem = doc.Root.Attribute("viewBox");

            if (vbelem != null)
            {
                var parts = vbelem.Value.Split(' ');

                viewbox.X = float.Parse(parts[0]);
                viewbox.Y = float.Parse(parts[1]);
                viewbox.Width = float.Parse(parts[2]);
                viewbox.Height = float.Parse(parts[3]);
            }
            else
            {
                viewbox.X = float.Parse(doc.Root.AttributeOrDefault("x", "0").Value);
                viewbox.Y = float.Parse(doc.Root.AttributeOrDefault("y", "0").Value);
                viewbox.Width = float.Parse(doc.Root.AttributeOrDefault("width", "200").Value);
                viewbox.Height = float.Parse(doc.Root.AttributeOrDefault("height", "200").Value);
            }

            List<ColoredVectorComponent> Components = new();

            foreach (var item in doc.Descendants())
            {
                var name = item.Name.LocalName.ToLowerInvariant();

                if (Importers.ContainsKey(name))
                {
                    var imported = Importers[name](item, viewbox);
                    Components.AddRange(imported);
                }
            }

            return new(Components);
        }

        static ColoredVectorComponent[] RectImporter(XElement element,RectangleF Viewbox)
        {
            var biggest = Math.Max(Viewbox.Width, Viewbox.Height);

            var x = (float.Parse(element.AttributeOrDefault("x","0").Value) - Viewbox.X) / biggest;
            var y = (float.Parse(element.AttributeOrDefault("y", "0").Value) - Viewbox.Y) / biggest;
            var w = float.Parse(element.AttributeOrDefault("width", "0").Value) / biggest;
            var h = float.Parse(element.AttributeOrDefault("height", "0").Value) / biggest;

            List<ColoredVectorComponent> components = new();

            IVectorASTComponent component = new RectangleComponent(new(x, y), new(w, h));

            var fill = element.Attribute("fill");

            if (fill != null && fill.Value.ToLowerInvariant() != "none")
            {
                var color4 = SixLabors.ImageSharp.Color.Parse(fill.Value).ToPixel<Rgba32>().ToVector4();

                components.Add(new(component,new VectorSolidColorSampler(color4)));
            }

            var stroke = element.Attribute("stroke");

            if (stroke != null && stroke.Value.ToLowerInvariant() != "none")
            {
                var color4 = SixLabors.ImageSharp.Color.Parse(stroke.Value).ToPixel<Rgba32>().ToVector4();
                var strokew = float.Parse(element.AttributeOrDefault("stroke-width", "1").Value) / biggest;

                components.Add(
                    new(
                        new SolidifyComponent(component, strokew / 2, strokew / 2), 
                        new VectorSolidColorSampler(color4)
                    )
                );
            }

            return components.ToArray();
        }

        static ColoredVectorComponent[] CircleImporter(XElement element, RectangleF Viewbox)
        {
            var biggest = Math.Max(Viewbox.Width, Viewbox.Height);

            var x = (float.Parse(element.AttributeOrDefault("cx", "0").Value) - Viewbox.X) / biggest;
            var y = (float.Parse(element.AttributeOrDefault("cy", "0").Value) - Viewbox.Y) / biggest;
            var r = float.Parse(element.AttributeOrDefault("r", "0").Value) / biggest;

            List<ColoredVectorComponent> components = new();

            IVectorASTComponent component = new EllipseComponent(new(x, y), new(r));

            var fill = element.Attribute("fill");

            if (fill != null && fill.Value.ToLowerInvariant() != "none")
            {
                var color4 = SixLabors.ImageSharp.Color.Parse(fill.Value).ToPixel<Rgba32>().ToVector4();

                components.Add(new(component, new VectorSolidColorSampler(color4)));
            }

            var stroke = element.Attribute("stroke");

            if (stroke != null && stroke.Value.ToLowerInvariant() != "none")
            {
                var color4 = SixLabors.ImageSharp.Color.Parse(stroke.Value).ToPixel<Rgba32>().ToVector4();
                var strokew = float.Parse(element.AttributeOrDefault("stroke-width", "1").Value) / biggest;

                components.Add(
                    new(
                        new SolidifyComponent(component, strokew / 2, strokew / 2),
                        new VectorSolidColorSampler(color4)
                    )
                );
            }

            return components.ToArray();
        }

        static ColoredVectorComponent[] EllipseImporter(XElement element, RectangleF Viewbox)
        {
            var biggest = Math.Max(Viewbox.Width, Viewbox.Height);

            var x = (float.Parse(element.AttributeOrDefault("cx", "0").Value) - Viewbox.X) / biggest;
            var y = (float.Parse(element.AttributeOrDefault("cy", "0").Value) - Viewbox.Y) / biggest;
            var rx = float.Parse(element.AttributeOrDefault("rx", "0").Value) / biggest;
            var ry = float.Parse(element.AttributeOrDefault("ry", "0").Value) / biggest;

            List<ColoredVectorComponent> components = new();

            IVectorASTComponent component = new EllipseComponent(new(x, y), new(rx,ry));

            var fill = element.Attribute("fill");

            if (fill != null && fill.Value.ToLowerInvariant() != "none")
            {
                var color4 = SixLabors.ImageSharp.Color.Parse(fill.Value).ToPixel<Rgba32>().ToVector4();

                components.Add(new(component, new VectorSolidColorSampler(color4)));
            }

            var stroke = element.Attribute("stroke");

            if (stroke != null && stroke.Value.ToLowerInvariant() != "none")
            {
                var color4 = SixLabors.ImageSharp.Color.Parse(stroke.Value).ToPixel<Rgba32>().ToVector4();
                var strokew = float.Parse(element.AttributeOrDefault("stroke-width", "1").Value) / biggest;

                components.Add(
                    new(
                        new SolidifyComponent(component, strokew / 2, strokew / 2),
                        new VectorSolidColorSampler(color4)
                    )
                );
            }

            return components.ToArray();
        }

        static XAttribute AttributeOrDefault(this XElement element, string attr,string defaultValue = "") =>
            element.Attribute(attr) ?? new XAttribute(attr, defaultValue);
    }
}
