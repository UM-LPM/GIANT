using System;

namespace Glicko2
{
    public class GlickoPlayer
    {
        private double glickoConversion = 173.7178;

        public GlickoPlayer(double rating = 1500, double ratingDeviation = 350, double volatility = 0.06)
        {
            Rating = rating;
            RatingDeviation = ratingDeviation;
            Volatility = volatility;
        }
        public string Name { get; set; }
        public double Rating { get; set; }
        public double RatingDeviation { get; set; }
        public double Volatility { get; set; }
        public double GlickoRating { get { return (Rating - 1500) / glickoConversion; } }
        public double GlickoRatingDeviation { get { return RatingDeviation / glickoConversion; } }
        public double GPhi { get { return 1 / Math.Sqrt(1 + (3 * Math.Pow(GlickoRatingDeviation, 2) / Math.Pow(Math.PI, 2))); } }
    }
}