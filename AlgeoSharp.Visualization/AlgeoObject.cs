using System;
using System.Drawing;
using AlgeoSharp;

namespace AlgeoSharp.Visualization
{
    public class AlgeoObject
    {
        public AlgeoObject(MultiVector value, Color color)
        {
            this.value = value.Clone();
            this.Color = color;
        }

        MultiVector value;
        public MultiVector Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value.Clone();
            }
        }

        public Color Color { get; set; }
    }
}
