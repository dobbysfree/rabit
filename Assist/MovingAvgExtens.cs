using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabit.Assist
{
    public static class MovingAvgExtens
    {
        public static IEnumerable<double> SimpleMovingAverage(IEnumerable<double> source)
        {
            //if (source == null) throw new ArgumentNullException("source");
            //if (sampleLength <= 0) throw new ArgumentException("Invalid sample length");

            //return SimpleMovingAverageImpl(source);

            var tempSample = new List<double>();
            var sample = new List<double>();
            foreach (double d in source)
            {
                tempSample.Add(d);
                sample.Add(tempSample.Average());
            }

            return sample;
        }

        private static IEnumerable<double> SimpleMovingAverageImpl(IEnumerable<double> source)
        {
            var sample = new Queue<double>();

            foreach (double d in source)
            {
                // if (sample.Count == sampleLength) sample.Dequeue();
                
                sample.Enqueue(d);
                yield return sample.Average();
            }
        }
    }
}