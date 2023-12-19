using System;

namespace Rabit.Models
{
    public class Sise
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public long Volume { get; set; }
        public int Sign { get; set; }
        public float Diff { get; set; }
    }
}