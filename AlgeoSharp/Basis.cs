using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgeoSharp
{
    public struct Basis
    {
        // Scalar
        public static readonly Basis S  = new Basis(0x00, false);
        public static readonly Basis PS = new Basis(0x1F, false);

        // Euclidean Metric
        public static readonly Basis E1 = new Basis(1 << 0, false);
        public static readonly Basis E2 = new Basis(1 << 1, false);
        public static readonly Basis E3 = new Basis(1 << 2, false);
        
        // Minkowski Metric
        public static readonly Basis EMINUS = new Basis(1 << 3, false);
        public static readonly Basis EPLUS = new Basis(1 << 4, false);

        // Null Metric
        public static readonly Basis E0 = new Basis(1 << 3, true);
        public static readonly Basis E8 = new Basis(1 << 4, true);

        // Combined
        public static readonly Basis EPLANE = new Basis(Basis.EMINUS.bitMask | Basis.EPLUS.bitMask, false);
        
        public static readonly Basis E12 = new Basis(Basis.E1.bitMask | Basis.E2.bitMask, false);
        public static readonly Basis E13 = new Basis(Basis.E1.bitMask | Basis.E3.bitMask, false);
        public static readonly Basis E23 = new Basis(Basis.E2.bitMask | Basis.E3.bitMask, false);
        public static readonly Basis E123 = new Basis(Basis.E1.bitMask | Basis.E2.bitMask | Basis.E3.bitMask, false);

        
        private static Dictionary<int, MultiVector> geometricProductCache = new Dictionary<int, MultiVector>();
        private static Dictionary<int, Blade> outerProductCache = new Dictionary<int, Blade>();
        private static Dictionary<int, Blade> innerProductCache = new Dictionary<int, Blade>();

        private static readonly double[] minkowskiSignature = { 1.0, 1.0, 1.0, -1.0, 1.0 };


        private Basis(int bitMask, bool nullMetric)
        {
            this.bitMask = bitMask;
            this.nullMetric = nullMetric;

            grade = 0;
            int temp = bitMask;
            while (temp != 0)
            {
                grade++;
                temp &= (temp - 1);
            }
        }


        readonly int bitMask;
        readonly int grade;
        readonly bool nullMetric;


        public int Grade
        {
            get
            {
                return grade;
            }
        }

        public bool IsNullMetric
        {
            get
            {
                return nullMetric;
            }
        }

        public bool IsMinkowskiMetric
        {
            get
            {
                if (IsNullMetric)
                    return false;

                return (this.bitMask & Basis.EPLANE.bitMask) != 0;
            }
        }

        public bool Contains(Basis basis)
        {
            if (!Basis.IsCompatible(this, basis))
                return false;

            return (this.bitMask & basis.bitMask) == basis.bitMask;
        }


        public static MultiVector operator +(Basis e1, Basis e2)
        {
            MultiVector ret = new MultiVector();
            ret[e1] = 1.0;
            ret[e2] += 1.0;
            return ret;
        }

        public static MultiVector operator -(Basis e1, Basis e2)
        {
            MultiVector ret = new MultiVector();
            ret[e1] = 1.0;
            ret[e2] -= 1.0;
            return ret;
        }

        public static Blade operator -(Basis e)
        {
            return new Blade(e, -1.0);
        }

        public static Blade operator *(double f, Basis e)
        {
            return new Blade(e, f);
        }

        public static Blade operator *(Basis e, double f)
        {
            return f * e;
        }

        public static Blade operator /(Basis e, double f)
        {
            return (1.0 / f) * e;
        }

        public static MultiVector operator *(Basis e1, Basis e2)
        {
            return Basis.GeometricProduct(e1, e2);
        }

        public static Blade operator ^(Basis e1, Basis e2)
        {
            return Basis.OuterProduct(e1, e2);
        }

        public static MultiVector operator /(Basis e1, Basis e2)
        {
            return e1 * ((MultiVector)e2).Inverse;
        }


        public static bool operator ==(Basis e1, Basis e2)
        {
            if ((object)e1 == null && (object)e2 == null) return true;
            if ((object)e1 == null) return false;
            if ((object)e2 == null) return false;

            return e1.bitMask == e2.bitMask && e1.IsNullMetric == e2.IsNullMetric;
        }

        public static bool operator !=(Basis e1, Basis e2)
        {
            return !(e1 == e2);
        }

        public override int GetHashCode()
        {
            int hash = (int)this.bitMask;
            if (this.IsNullMetric)
                hash |= 1 << 15;
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is Basis)
                return this == (Basis)obj;
            else
                return false;
        }

        public static int GetHashPair(Basis e1, Basis e2)
        {
            return (e1.GetHashCode() << 16) | e2.GetHashCode();
        }


        public static MultiVector GeometricProduct(Basis e1, Basis e2)
        {
            int hashPair = Basis.GetHashPair(e1, e2);

            if (geometricProductCache.ContainsKey(hashPair))
                return geometricProductCache[hashPair];

            MultiVector result;

            if (e1.IsNullMetric || e2.IsNullMetric)
            {
                MultiVector m1 = e1.ToMinkowskiMetric();
                MultiVector m2 = e2.ToMinkowskiMetric();

                result = (m1 * m2).ToNullMetric();
            }
            else
            {
                int rBitMask = e1.bitMask ^ e2.bitMask;
                double rValue = Basis.Order(e1, e2);

                int common = e1.bitMask & e2.bitMask;
                int i = 0;

                while (common != 0)
                {
                    if ((common & 0x1) != 0)
                        rValue *= Basis.minkowskiSignature[i];

                    common >>= 1;
                    i++;
                }

                result = new Blade(new Basis(rBitMask, false), rValue);
            }

            geometricProductCache.Add(hashPair, result);

            return result;
        }

        public static Blade OuterProduct(Basis e1, Basis e2)
        {
            int hashPair = Basis.GetHashPair(e1, e2);

            if (outerProductCache.ContainsKey(hashPair))
                return outerProductCache[hashPair];

            Blade result;

            if (e1.IsNullMetric || e2.IsNullMetric)
            {
                MultiVector m1 = e1.ToMinkowskiMetric();
                MultiVector m2 = e2.ToMinkowskiMetric();

                result = (Blade)(m1 ^ m2).ToNullMetric();
            }
            else
            {
                if ((e1.bitMask & e2.bitMask) == 0)
                    result = (Blade)(e1 * e2);
                else
                    result = Blade.Zero;
            }

            outerProductCache.Add(hashPair, result);

            return result;
        }

        public static Blade InnerProduct(Basis e1, Basis e2)
        {
            int hashPair = Basis.GetHashPair(e1, e2);

            if (innerProductCache.ContainsKey(hashPair))
                return innerProductCache[hashPair];

            Blade result;

            if (e1.IsNullMetric || e2.IsNullMetric)
            {
                MultiVector m1 = e1.ToMinkowskiMetric();
                MultiVector m2 = e2.ToMinkowskiMetric();

                result = (Blade)MultiVector.InnerProduct(m1, m2).ToNullMetric();
            }
            else
            {
                if ((e1.bitMask & ~e2.bitMask) == 0)
                    result = (Blade)(e1 * e2);
                else
                    result = Blade.Zero;
            }

            innerProductCache.Add(hashPair, result);

            return result;
        }


        public static bool IsCompatible(Basis e1, Basis e2)
        {
            return !(e1.IsNullMetric && e2.IsMinkowskiMetric ||
                e1.IsMinkowskiMetric && e2.IsNullMetric);
        }

        public static int Order(Basis e1, Basis e2)
        {
            int a = e1.bitMask;
            int b = e2.bitMask;
            int n = 0;

            do
            {
                a >>= 1;
                n += (new Basis(a & b, false)).Grade;
            } while (a != 0);

            if (n % 2 == 0)
                return 1;
            else
                return -1;
        }

        public static int DisplayOrder(Basis e1, Basis e2)
        {
            if (e1.Grade != e2.Grade)
            {
                return e1.Grade - e2.Grade;
            }
            else
            {
                return (int)(e1.bitMask - e2.bitMask);
            }
        }


        public MultiVector ToMinkowskiMetric()
        {
            if (!this.IsNullMetric)
                return this;

            if (this.Contains(Basis.EPLANE))
                return this;

            int b = this.bitMask & ~Basis.EPLANE.bitMask;
            Basis bp = new Basis(b | Basis.EPLUS.bitMask, false);
            Basis bm = new Basis(b | Basis.EMINUS.bitMask, false);

            if (this.Contains(Basis.E0))
                return 0.5 * (bm + bp);

            if (this.Contains(Basis.E8))
                return bm - bp;

            // not possible
            return this;
        }

        public MultiVector ToNullMetric()
        {
            if (!this.IsMinkowskiMetric)
                return this;

            if (this.Contains(Basis.EPLANE))
                return this;

            int b = this.bitMask & ~Basis.EPLANE.bitMask;
            Basis b0 = new Basis(b | Basis.E0.bitMask, true);
            Basis b8 = new Basis(b | Basis.E8.bitMask, true);

            if (this.Contains(Basis.EPLUS))
                return b0 - 0.5 * b8;

            if (this.Contains(Basis.EMINUS))
                return b0 + 0.5 * b8;

            return this;
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();

            if (this.Contains(Basis.E1))
                ret.Append("e1");
            if (this.Contains(Basis.E2))
                ret.Append("e2");
            if (this.Contains(Basis.E3))
                ret.Append("e3");
            if (this.Contains(Basis.EPLANE))
                ret.Append("E");
            else
            {
                if (this.Contains(Basis.EPLUS))
                    ret.Append("e+");
                if (this.Contains(Basis.EMINUS))
                    ret.Append("e-");
                if (this.Contains(Basis.E0))
                    ret.Append("e0");
                if (this.Contains(Basis.E8))
                    ret.Append("e8");
            }

            return ret.ToString();
        }
    }
}
