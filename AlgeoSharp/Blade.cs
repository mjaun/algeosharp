using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgeoSharp
{
    public struct Blade
    {
        public static readonly Blade Zero = new Blade(Basis.S, 0.0);

        public Blade(Basis basis, double value)
        {
            this.basis = basis;
            this.value = value;
        }


        Basis basis;
        public Basis Basis 
        {
            get { return basis; }
        }

        double value;
        public double Value
        {
            get { return value; }
        }


        public static implicit operator Blade(Basis e)
        {
            return new Blade(e, 1.0);
        }

        public static implicit operator Blade(double value)
        {
            return new Blade(Basis.S, value);
        }

        public static explicit operator double(Blade b)
        {
            if (b.Basis != Basis.S)
                throw new InvalidCastException();

            return b.Value;
        }

        public static explicit operator Basis(Blade b)
        {
            if (b.Value != 1.0)
                throw new InvalidCastException();

            return b.Basis;
        }


        public static MultiVector operator +(Blade b1, Blade b2)
        {
            MultiVector ret = new MultiVector();
            ret[b1.Basis] = b1.Value;
            ret[b2.Basis] += b2.Value;
            return ret;
        }

        public static MultiVector operator -(Blade b1, Blade b2)
        {
            MultiVector ret = new MultiVector();
            ret[b1.Basis] = b1.Value;
            ret[b2.Basis] -= b2.Value;
            return ret;
        }

        public static Blade operator -(Blade b)
        {
            return new Blade(b.Basis, -b.Value);
        }

        public static Blade operator *(double f, Blade b)
        {
            return new Blade(b.Basis, f * b.Value);
        }

        public static Blade operator *(Blade b, double f)
        {
            return f * b;
        }

        public static Blade operator /(Blade b, double f)
        {
            return (1.0 / f) * b;
        }

        public static MultiVector operator /(Blade b1, Blade b2)
        {
            return b1 * ((MultiVector)b2).Inverse;
        }

        public static MultiVector operator *(Blade b1, Blade b2)
        {
            return Blade.GeometricProduct(b1, b2);
        }

        public static Blade operator ^(Blade b1, Blade b2)
        {
            return Blade.OuterProduct(b1, b2);
        }


        public static bool operator ==(Blade b1, Blade b2)
        {
            if ((object)b1 == null && (object)b2 == null) return true;
            if ((object)b1 == null) return false;
            if ((object)b2 == null) return false;

            return b1.Basis == b2.Basis && b1.Value == b2.Value;
        }

        public static bool operator !=(Blade b1, Blade b2)
        {
            return !(b1 == b2);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode() ^ this.Basis.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Blade)
                return this == (Blade)obj;
            else
                return false;
        }


        public static MultiVector GeometricProduct(Blade b1, Blade b2)
        {
            return (b1.Value * b2.Value) * Basis.GeometricProduct(b1.Basis, b2.Basis);
        }

        public static Blade OuterProduct(Blade b1, Blade b2)
        {
            return (b1.Value * b2.Value) * Basis.OuterProduct(b1.Basis, b2.Basis);
        }

        public static Blade InnerProduct(Blade b1, Blade b2)
        {
            return (b1.Value * b2.Value) * Basis.InnerProduct(b1.Basis, b2.Basis);
        }


        public MultiVector ToMinkowskiMetric()
        {
            return this.Value * this.Basis.ToMinkowskiMetric();
        }

        public MultiVector ToNullMetric()
        {
            return this.Value * this.Basis.ToNullMetric();
        }

        public override string ToString()
        {
            string basis = this.Basis.ToString();
            return (basis != "") ? "(" + Value.ToString() + "*" + Basis.ToString() + ")" : "(" + Value.ToString() + ")";
        }
    }
}
