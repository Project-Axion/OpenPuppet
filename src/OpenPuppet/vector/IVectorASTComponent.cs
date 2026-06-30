using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.vector
{
    public interface IVectorASTComponent
    {
        public VectorMeshPrototype Flatten(uint density);
    }

    public interface IVectorASTCommand
    {
        public List<Vector3> Flatten(double Y);
    }

    public class VectorPathComponent(List<IVectorASTCommand> commands) : IVectorASTComponent
    {
        public List<IVectorASTCommand> Commands { get; } = commands;

        public VectorMeshPrototype Flatten(uint density)
        {
            double step = 1d / density;

            List<Vector3> positions = new();
            List<List<int>> flatMap = new();

            for (uint i = 0; i <= density; i++)
            {
                double y = i == density ? 1d : i * step;

                List<int> map = new();

                List<Vector3> lpositions = new();

                foreach (var item in Commands)
                    lpositions.AddRange(item.Flatten(y));

                lpositions = lpositions.OrderBy(x => x.X).ToList();

                map.AddRange(Enumerable.Range(positions.Count, lpositions.Count));

                positions.AddRange(lpositions);
                flatMap.Add(map);
            }

            return new(positions, flatMap);
        }
    }

    public class LineCommand(Vector2 start, Vector2 end) : IVectorASTCommand
    {
        public Vector2 Start { get; } = start;
        public Vector2 End { get; } = end;

        public List<Vector3> Flatten(double Y)
        {
            if (Start.Y == End.Y)
            {
                if (Math.Abs(Start.Y - Y) > double.Epsilon)
                    return [];

                return [
                    new(Start.X, (float)Y, 0f),
                    new(End.X, (float)Y, 0f),
                ];
            }

            double t = (Y - Start.Y) / (End.Y - Start.Y);

            if (t < 0.0 || t > 1.0)
                return [];

            float x = (float)(Start.X + (End.X - Start.X) * t);
            return [new(x, (float)Y, 0f)];
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

        public List<Vector3> Flatten(double Y)
        {
            var results = new List<Vector3>();

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
                results.Add(new Vector3((float)px, (float)Y, 0f));
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

        public List<Vector3> Flatten(double Y)
        {
            double p0 = Origin.Y, p1 = Control1.Y, p2 = Control2.Y, p3 = Destination.Y;

            double a = -p0 + 3 * p1 - 3 * p2 + p3;
            double b = 3 * p0 - 6 * p1 + 3 * p2;
            double c = -3 * p0 + 3 * p1;
            double d = p0 - Y;

            var roots = SolveCubic(a, b, c, d);
            var results = new List<Vector3>();

            foreach (var t in roots)
            {
                if (t < 0 || t > 1) continue;
                double x = Math.Pow(1 - t, 3) * Origin.X
                         + 3 * Math.Pow(1 - t, 2) * t * Control1.X
                         + 3 * (1 - t) * t * t * Control2.X
                         + t * t * t * Destination.X;
                results.Add(new Vector3((float)x, (float)Y, 0f));
            }

            return results;
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

        public List<Vector3> Flatten(double Y)
        {
            double p0 = Origin.Y, p1 = Control.Y, p2 = Destination.Y;

            double a = p0 - 2 * p1 + p2;
            double b = -2 * p0 + 2 * p1;
            double c = p0 - Y;

            var roots = SolveQuadratic(a, b, c);
            var results = new List<Vector3>();

            foreach (var t in roots)
            {
                if (t < 0 || t > 1) continue;
                double x = Math.Pow(1 - t, 2) * Origin.X
                         + 2 * (1 - t) * t * Control.X
                         + t * t * Destination.X;
                results.Add(new Vector3((float)x, (float)Y, 0f));
            }

            return results;
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
