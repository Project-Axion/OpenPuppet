using Newtonsoft.Json;
using OpenPuppet.SDK;
using OpenPuppet.SDK.vector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.vector
{
    public struct UnifiedVector(List<ColoredVectorComponent> components)
    {
        public uint FormatVersion { get; set; } = 1;

        public List<ColoredVectorComponent> Components { get; set; } = components;
    }

    public struct ColoredVectorComponent(IVectorASTComponent ast, IVectorColorSampler colorSampler)
    {
        public IVectorASTComponent AST { get; set; } = ast;
        public IVectorColorSampler ColorSampler { get; set; } = colorSampler;
    }

    public interface IVectorASTComponent
    {
        public VectorMeshPrototype Flatten(uint density);

        public List<(Vector3 point, Vector3 normal)> GetIntersectedPN(double y);

        public static UnifiedVector LoadFromDisk(string vectorAssetPath) => JsonConvert.DeserializeObject<UnifiedVector>
        (
            File.ReadAllText(vectorAssetPath),
            new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = SDK.SDK.JsonTypeBinder
            }
        )!;

        public static void SaveToDisk(UnifiedVector component, string vectorAssetPath)
        {
            var json = JsonConvert.SerializeObject(component, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = SDK.SDK.JsonTypeBinder
            });

            File.WriteAllText(vectorAssetPath, json);
        }
    }

    public interface IVectorASTCommand
    {
        public List<Vector3> Flatten(double Y);

        public List<(Vector3 point, Vector3 normal)> FlattenN(double y);
    }

    public class VectorPathComponent(List<IVectorASTCommand> commands) : IVectorASTComponent
    {
        public List<IVectorASTCommand> Commands { get; } = commands;

        public List<(Vector3 point, Vector3 normal)> GetIntersectedPN(double y)
        {
            var pts = new List<(Vector3 point, Vector3 normal)>();
            foreach (var cmd in Commands)
                pts.AddRange(cmd.FlattenN(y));
            return pts.OrderBy(p => p.point.X).ToList();
        }

        List<Vector3> SampleCommandsAt(double y)
        {
            var pts = new List<Vector3>();
            foreach (var cmd in Commands)
                pts.AddRange(cmd.Flatten(y));
            return pts.OrderBy(p => p.X).ToList();
        }

        double BisectTopologyChange(double y0, double y1, int countAt0, int iterations = 24)
        {
            for (int i = 0; i < iterations; i++)
            {
                double mid = (y0 + y1) * 0.5;
                int c = SampleCommandsAt(mid).Count;
                if (c == countAt0) y0 = mid;
                else y1 = mid;
            }
            return (y0 + y1) * 0.5;
        }

        public VectorMeshPrototype Flatten(uint density)
        {
            double step = 1d / density;
            List<Vector3> positions = new();
            List<List<int>> flatMap = new();

            void AddScanline(List<Vector3> sample)
            {
                var map = Enumerable.Range(positions.Count, sample.Count).ToList();
                positions.AddRange(sample);
                flatMap.Add(map);
            }

            double prevY = 0d;
            List<Vector3> prevSample = SampleCommandsAt(prevY);
            AddScanline(prevSample);

            for (uint i = 1; i <= density; i++)
            {
                double y = i == density ? 1d : i * step;
                List<Vector3> currSample = SampleCommandsAt(y);

                if (currSample.Count != prevSample.Count)
                {
                    double tY = BisectTopologyChange(prevY, y, prevSample.Count);

                    AddScanline(SampleCommandsAt(tY - 1e-9));
                    AddScanline(SampleCommandsAt(tY + 1e-9));
                }

                AddScanline(currSample);
                prevY = y;
                prevSample = currSample;
            }

            return new(positions, flatMap);
        }
    }

    public class SolidifyComponent(IVectorASTComponent flat, float thicknessIN, float thicknessOUT) : IVectorASTComponent
    {
        public IVectorASTComponent FlatVector { get; set; } = flat;
        public float LineThicknessIN { get; set; } = thicknessIN;
        public float LineThicknessOUT { get; set; } = thicknessOUT;

        public List<(Vector3 point, Vector3 normal)> GetIntersectedPN(double y)
        {
            return null!;
        }

        List<(Vector3 point, Vector3 normal)> SampleAt(double y) => FlatVector.GetIntersectedPN(y);

        double BisectTopologyChange(double y0, double y1, int countAt0, int iterations = 32)
        {
            for (int i = 0; i < iterations; i++)
            {
                double mid = (y0 + y1) * 0.5;
                int c = SampleAt(mid).Count;
                if (c == countAt0) y0 = mid;
                else y1 = mid;
            }
            return (y0 + y1) * 0.5;
        }

        public VectorMeshPrototype Flatten(uint density)
        {
            double step = 1d / density;
            List<Vector3> positions = new();
            List<List<int>> flatMap = new();

            void AddScanline(List<(Vector3 point, Vector3 normal)> points)
            {
                List<int> map = new();

                foreach (var (point, normal) in points)
                {
                    int currentIndex = positions.Count;

                    positions.Add(point + normal * LineThicknessOUT);
                    positions.Add(point - normal * LineThicknessIN);

                    map.Add(currentIndex);
                    map.Add(currentIndex + 1);
                }

                flatMap.Add(map);
            }

            double prevY = 0d;
            var prevSample = SampleAt(prevY);
            AddScanline(prevSample);

            for (uint i = 1; i <= density; i++)
            {
                double y = i == density ? 1d : i * step;
                var currSample = SampleAt(y);

                if (currSample.Count != prevSample.Count)
                {
                    double tY = BisectTopologyChange(prevY, y, prevSample.Count);

                    bool enteringFromEmpty = prevSample.Count == 0;
                    bool leavingToEmpty = currSample.Count == 0;

                    if (enteringFromEmpty || leavingToEmpty)
                    {
                        double dir = enteringFromEmpty ? 1e-9 : -1e-9;
                        var boundarySample = SampleAt(tY);
                        if (boundarySample.Count == 0)
                            boundarySample = SampleAt(tY + dir);

                        AddScanline(boundarySample);
                    }
                    else
                    {
                        AddScanline(SampleAt(tY - 1e-9));
                        AddScanline(SampleAt(tY + 1e-9));
                    }
                }

                AddScanline(currSample);
                prevY = y;
                prevSample = currSample;
            }

            return new(positions, flatMap);
        }
    }


    public class EllipseComponent(Vector2 center, Vector2 radii) : IVectorASTComponent
    {
        public Vector2 Center { get; set; } = center;
        public Vector2 Radii { get; set; } = radii;

        public List<(Vector3 point, Vector3 normal)> GetIntersectedPN(double y)
        {
            if (Math.Abs(y - Center.Y) > Radii.Y) return [];

            double deltaradiusY = 1 / (Radii.Y * Radii.Y);
            double x = Math.Sqrt(Math.Max(0, 1 - (y - Center.Y) * (y - Center.Y) * deltaradiusY)) * Radii.X;

            var left = new Vector3((float)(Center.X - x), (float)y, 0f);
            var right = new Vector3((float)(Center.X + x), (float)y, 0f);

            float rxSq = (float)(Radii.X * Radii.X);
            float rySq = (float)(Radii.Y * Radii.Y);

            Vector3 normalLeft = Vector3.Normalize(new Vector3(
                (left.X - (float)Center.X) / rxSq,
                (left.Y - (float)Center.Y) / rySq,
                0f
            ));

            Vector3 normalRight = Vector3.Normalize(new Vector3(
                (right.X - (float)Center.X) / rxSq,
                (right.Y - (float)Center.Y) / rySq,
                0f
            ));

            return [(left, normalLeft), (right, normalRight)];
        }

        public VectorMeshPrototype Flatten(uint density)
        {
            double step = 1d / density;
            List<Vector3> positions = new();
            List<List<int>> flatMap = new();

            double deltaradiusY = 1 / (Radii.Y * Radii.Y);

            for (uint i = 0; i <= density; i++)
            {
                double y = i == density ? 1d : i * step;

                if (Math.Abs(y - Center.Y) > Radii.Y) continue;

                double x = Math.Sqrt(Math.Max(0, 1 - (y - Center.Y) * (y - Center.Y) * deltaradiusY)) * Radii.X;

                positions.Add(new((float)(Center.X - x), (float)y, 0f));
                positions.Add(new((float)(Center.X + x), (float)y, 0f));

                flatMap.Add([positions.Count - 2, positions.Count - 1]);
            }

            return new(positions, flatMap);
        }
    }

    public class RectangleComponent(Vector2 position, Vector2 size) : IVectorASTComponent
    {
        public Vector2 Position { get; set; } = position;
        public Vector2 Size { get; set; } = size;

        public List<(Vector3 point, Vector3 normal)> GetIntersectedPN(double y)
        {
            if (y < Position.Y || y > Position.Y + Size.Y) return [];
            var left = new Vector3(Position.X, (float)y, 0f);
            var right = new Vector3(Position.X + Size.X, (float)y, 0f);
            var normalLeft = new Vector3(-1, 0, 0);
            var normalRight = new Vector3(1, 0, 0);
            return [(left, normalLeft), (right, normalRight)];
        }

        public VectorMeshPrototype Flatten(uint density)
        {
            double step = 1d / density;
            List<Vector3> positions = new();
            List<List<int>> flatMap = new();

            for (uint i = 0; i <= density; i++)
            {
                double y = i == density ? 1d : i * step;

                if (y < Position.Y || y > Position.Y + Size.Y) continue;

                positions.Add(new(Position.X, (float)y, 0f));
                positions.Add(new(Position.X + Size.X, (float)y, 0f));

                flatMap.Add([positions.Count - 2, positions.Count - 1]);
            }

            return new(positions, flatMap);
        }
    }

    public class LineCommand(Vector2 start, Vector2 end) : IVectorASTCommand
    {
        public Vector2 Start { get; } = start;
        public Vector2 End { get; } = end;

        public List<Vector3> Flatten(double Y) =>
            FlattenN(Y).Select(x => x.point).ToList();

        public List<(Vector3 point, Vector3 normal)> FlattenN(double Y)
        {
            Vector2 dir = End - Start;
            float len = dir.Length();
            var n = new Vector3(-dir.Y / len, dir.X / len, 0f);

            if (Start.Y == End.Y)
            {
                if (Math.Abs(Start.Y - Y) > double.Epsilon)
                    return [];

                return [
                    (new(Start.X, (float)Y, 0f),n),
                    (new(End.X, (float)Y, 0f),n),
                ];
            }

            double t = (Y - Start.Y) / (End.Y - Start.Y);

            if (t < 0.0 || t > 1.0)
                return [];

            float x = (float)(Start.X + (End.X - Start.X) * t);
            return [(new(x, (float)Y, 0f), n)];
        }
    }

    public class ArcCommand : IVectorASTCommand
    {
        public Vector2 Origin { get; set; }
        public Vector2 Destination { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public double XAxisRotation { get; set; }
        public bool LargeArcFlag { get; set; }
        public bool SweepFlag { get; set; }

        private double _cx, _cy;
        private double _theta1, _dTheta;
        private double _cosTeta, _sinTeta;

        public ArcCommand(Vector2 origin, double rx, double ry, double xRot, bool largeArc, bool sweep, Vector2 destination)
        {
            Origin = origin;
            Destination = destination;
            RadiusX = Math.Abs(rx);
            RadiusY = Math.Abs(ry);
            XAxisRotation = xRot;
            LargeArcFlag = largeArc;
            SweepFlag = sweep;
            ComputeCenter();
        }

        private void ComputeCenter()
        {
            double Teta = XAxisRotation * Math.PI / 180.0;
            _cosTeta = Math.Cos(Teta);
            _sinTeta = Math.Sin(Teta);

            double dx = (Origin.X - Destination.X) / 2.0;
            double dy = (Origin.Y - Destination.Y) / 2.0;
            double x1p = _cosTeta * dx + _sinTeta * dy;
            double y1p = -_sinTeta * dx + _cosTeta * dy;

            double rx = RadiusX, ry = RadiusY;
            double lambda = (x1p * x1p) / (rx * rx) + (y1p * y1p) / (ry * ry);
            if (lambda > 1.0)
            {
                double sqrtL = Math.Sqrt(lambda);
                rx *= sqrtL;
                ry *= sqrtL;
                RadiusX = rx;
                RadiusY = ry;
            }

            double rx2 = rx * rx, ry2 = ry * ry;
            double x1p2 = x1p * x1p, y1p2 = y1p * y1p;

            double num = rx2 * ry2 - rx2 * y1p2 - ry2 * x1p2;
            double den = rx2 * y1p2 + ry2 * x1p2;
            double sq = Math.Sqrt(Math.Max(0, num / den));
            if (LargeArcFlag == SweepFlag) sq = -sq;

            double cxp = sq * rx * y1p / ry;
            double cyp = -sq * ry * x1p / rx;

            _cx = _cosTeta * cxp - _sinTeta * cyp + (Origin.X + Destination.X) / 2.0;
            _cy = _sinTeta * cxp + _cosTeta * cyp + (Origin.Y + Destination.Y) / 2.0;

            _theta1 = Angle(1, 0, (x1p - cxp) / rx, (y1p - cyp) / ry);
            double dTheta = Angle(
                (x1p - cxp) / rx, (y1p - cyp) / ry,
                (-x1p - cxp) / rx, (-y1p - cyp) / ry
            ) % (2 * Math.PI);

            if (!SweepFlag && dTheta > 0) dTheta -= 2 * Math.PI;
            if (SweepFlag && dTheta < 0) dTheta += 2 * Math.PI;
            _dTheta = dTheta;
        }

        private static double Angle(double ux, double uy, double vx, double vy)
        {
            double dot = ux * vx + uy * vy;
            double len = Math.Sqrt(ux * ux + uy * uy) * Math.Sqrt(vx * vx + vy * vy);
            double angle = Math.Acos(Math.Clamp(dot / len, -1, 1));
            return (ux * vy - uy * vx) < 0 ? -angle : angle;
        }

        private (double x, double y) EllipsePoint(double t)
        {
            double ex = RadiusX * Math.Cos(t);
            double ey = RadiusY * Math.Sin(t);
            return (
                _cosTeta * ex - _sinTeta * ey + _cx,
                _sinTeta * ex + _cosTeta * ey + _cy
            );
        }

        private Vector3 EllipseNormal(double t)
        {
            double nxLocal = Math.Cos(t) / RadiusX;
            double nyLocal = Math.Sin(t) / RadiusY;

            double nx = _cosTeta * nxLocal - _sinTeta * nyLocal;
            double ny = _sinTeta * nxLocal + _cosTeta * nyLocal;

            if (SweepFlag)
            {
                nx = -nx;
                ny = -ny;
            }

            double len = Math.Sqrt(nx * nx + ny * ny);
            return new Vector3((float)(nx / len), (float)(ny / len), 0f);
        }


        public List<Vector3> Flatten(double Y) =>
            FlattenN(Y).Select(x => x.point).ToList();

        public List<(Vector3 point, Vector3 normal)> FlattenN(double Y)
        {
            var results = new List<(Vector3 point, Vector3 normal)>();

            double A = _sinTeta * RadiusX;
            double B = _cosTeta * RadiusY;
            double C = Y - _cy;

            double R = Math.Sqrt(A * A + B * B);
            if (R < 1e-10) return results;

            double ratio = C / R;
            if (Math.Abs(ratio) > 1.0) return results;

            double alpha = Math.Atan2(B, A);
            double acos = Math.Acos(Math.Clamp(ratio, -1, 1));

            double[] candidates = { alpha + acos, alpha - acos };

            foreach (var t in candidates)
            {
                if (!IsInArc(t)) continue;
                var (px, _) = EllipsePoint(t);
                results.Add((new Vector3((float)px, (float)Y, 0f), EllipseNormal(t)));
            }

            return results;
        }

        private bool IsInArc(double t)
        {
            double offset = t - _theta1;

            offset = offset % (2 * Math.PI);
            if (offset > Math.PI) offset -= 2 * Math.PI;
            if (offset < -Math.PI) offset += 2 * Math.PI;

            return _dTheta >= 0
                ? offset >= -1e-9 && offset <= _dTheta + 1e-9
                : offset <= 1e-9 && offset >= _dTheta - 1e-9;
        }
    }

    public class CubicBezierCommand : IVectorASTCommand
    {
        public Vector2 Origin { get; set; }
        public Vector2 Destination { get; set; }
        public Vector2 Control1 { get; set; }
        public Vector2 Control2 { get; set; }

        public CubicBezierCommand(Vector2 origin, Vector2 control1, Vector2 control2, Vector2 destination)
        {
            Origin = origin;
            Control1 = control1;
            Control2 = control2;
            Destination = destination;
        }

        public List<Vector3> Flatten(double Y) =>
            FlattenN(Y).Select(x => x.point).ToList();

        public List<(Vector3 point, Vector3 normal)> FlattenN(double Y)
        {
            double p0 = Origin.Y, p1 = Control1.Y, p2 = Control2.Y, p3 = Destination.Y;

            double a = -p0 + 3 * p1 - 3 * p2 + p3;
            double b = 3 * p0 - 6 * p1 + 3 * p2;
            double c = -3 * p0 + 3 * p1;
            double d = p0 - Y;

            var roots = SolveCubic(a, b, c, d);
            var results = new List<(Vector3 point, Vector3 normal)>();

            foreach (var t in roots)
            {
                if (t < 0 || t > 1) continue;
                double x = Math.Pow(1 - t, 3) * Origin.X
                         + 3 * Math.Pow(1 - t, 2) * t * Control1.X
                         + 3 * (1 - t) * t * t * Control2.X
                         + t * t * t * Destination.X;
                results.Add((new Vector3((float)x, (float)Y, 0f), BezierNormal(t)));
            }

            return results;
        }

        private Vector2 BezierTangent(double t)
        {
            double ax = 3 * Math.Pow(1 - t, 2) * (Control1.X - Origin.X)
                      + 6 * (1 - t) * t * (Control2.X - Control1.X)
                      + 3 * t * t * (Destination.X - Control2.X);

            double ay = 3 * Math.Pow(1 - t, 2) * (Control1.Y - Origin.Y)
                      + 6 * (1 - t) * t * (Control2.Y - Control1.Y)
                      + 3 * t * t * (Destination.Y - Control2.Y);

            return new Vector2((float)ax, (float)ay);
        }

        private Vector3 BezierNormal(double t)
        {
            Vector2 tangent = BezierTangent(t);

            if (tangent.LengthSquared() < 1e-12)
            {
                tangent = Destination - Origin;
                if (tangent.LengthSquared() < 1e-12) tangent = new Vector2(1, 0);
            }

            Vector2 normal = new Vector2(-tangent.Y, tangent.X);
            normal = Vector2.Normalize(normal);

            return new Vector3(normal, 0f);
        }

        private static List<double> SolveCubic(double a, double b, double c, double d)
        {
            var roots = new List<double>();

            if (Math.Abs(a) < 1e-10)
            {
                roots.AddRange(SolveQuadratic(b, c, d));
                return roots;
            }

            double A = b / a, B = c / a, C = d / a;
            double p = B - A * A / 3;
            double q = 2 * A * A * A / 27 - A * B / 3 + C;
            double D = q * q / 4 + p * p * p / 27;

            if (D > 1e-10)
            {
                double sqrtD = Math.Sqrt(D);
                double u = Math.Cbrt(-q / 2 + sqrtD);
                double v = Math.Cbrt(-q / 2 - sqrtD);
                roots.Add(u + v - A / 3);
            }
            else if (D < -1e-10)
            {
                double r = Math.Sqrt(-p * p * p / 27);
                double theta = Math.Acos(-q / (2 * r));
                double m = 2 * Math.Cbrt(r);
                roots.Add(m * Math.Cos(theta / 3) - A / 3);
                roots.Add(m * Math.Cos((theta + 2 * Math.PI) / 3) - A / 3);
                roots.Add(m * Math.Cos((theta + 4 * Math.PI) / 3) - A / 3);
            }
            else
            {
                double u = Math.Cbrt(-q / 2);
                roots.Add(2 * u - A / 3);
                roots.Add(-u - A / 3);
            }

            return roots;
        }

        private static List<double> SolveQuadratic(double a, double b, double c)
        {
            var roots = new List<double>();
            if (Math.Abs(a) < 1e-10)
            {
                if (Math.Abs(b) > 1e-10) roots.Add(-c / b);
                return roots;
            }
            double disc = b * b - 4 * a * c;
            if (disc < 0) return roots;
            double sq = Math.Sqrt(disc);
            roots.Add((-b + sq) / (2 * a));
            roots.Add((-b - sq) / (2 * a));
            return roots;
        }
    }

    public class QuadraticBezierCommand : IVectorASTCommand
    {
        public Vector2 Origin { get; set; }
        public Vector2 Destination { get; set; }
        public Vector2 Control { get; set; }

        public QuadraticBezierCommand(Vector2 origin, Vector2 control, Vector2 destination)
        {
            Origin = origin;
            Control = control;
            Destination = destination;
        }

        public List<Vector3> Flatten(double Y) =>
            FlattenN(Y).Select(x => x.point).ToList();

        public List<(Vector3 point, Vector3 normal)> FlattenN(double Y)
        {
            double p0 = Origin.Y, p1 = Control.Y, p2 = Destination.Y;

            double a = p0 - 2 * p1 + p2;
            double b = -2 * p0 + 2 * p1;
            double c = p0 - Y;

            var roots = SolveQuadratic(a, b, c);
            var results = new List<(Vector3 point, Vector3 normal)>();

            foreach (var t in roots)
            {
                if (t < 0 || t > 1) continue;
                double x = Math.Pow(1 - t, 2) * Origin.X
                         + 2 * (1 - t) * t * Control.X
                         + t * t * Destination.X;
                results.Add((new Vector3((float)x, (float)Y, 0f), QuadraticNormal(t)));
            }

            return results;
        }

        private Vector2 QuadraticTangent(double t)
        {
            Vector2 a = Control - Origin;
            Vector2 b = Destination - Control;

            double tx = 2 * (1 - t) * a.X + 2 * t * b.X;
            double ty = 2 * (1 - t) * a.Y + 2 * t * b.Y;

            return new Vector2((float)tx, (float)ty);
        }

        private Vector3 QuadraticNormal(double t)
        {
            Vector2 tangent = QuadraticTangent(t);

            if (tangent.LengthSquared() < 1e-12)
            {
                tangent = Destination - Origin;
                if (tangent.LengthSquared() < 1e-12) tangent = new Vector2(1, 0);
            }

            Vector2 normal = new Vector2(-tangent.Y, tangent.X);
            normal = Vector2.Normalize(normal);

            return new(normal, 0);
        }

        private static List<double> SolveQuadratic(double a, double b, double c)
        {
            var roots = new List<double>();
            if (Math.Abs(a) < 1e-10)
            {
                if (Math.Abs(b) > 1e-10) roots.Add(-c / b);
                return roots;
            }
            double disc = b * b - 4 * a * c;
            if (disc < 0) return roots;
            double sq = Math.Sqrt(disc);
            roots.Add((-b + sq) / (2 * a));
            roots.Add((-b - sq) / (2 * a));
            return roots;
        }
    }
}