using System;
using System.Collections.Generic;

namespace Glicko2
{    
    public static class GlickoCalculator
    {
        private static double VolatilityChange = 0.5;
        private static double ConvergenceTolerance = 0.000001;
        private static double glickoConversion = 173.7178;

        public static GlickoPlayer CalculateRanking(GlickoPlayer competitor, List<GlickoOpponent> opponents)
        {
            var variance = ComputeVariance(competitor, opponents);

            var updatedVolatility = CalculateNewVolatility(competitor, opponents, variance);
            
            var preratingDeviation = CalculatePreRatingDeviation(competitor.GlickoRatingDeviation, updatedVolatility);

            var newRatingDeviation = CalculateNewRatingDeviation(preratingDeviation, variance);
            var newRating = CalculateNewRating(competitor, opponents, newRatingDeviation);

            competitor.RatingDeviation = ConvertRatingDeviationToOriginal(newRatingDeviation);

            if (opponents.Count == 0) return competitor;

            competitor.Rating = ConvertRatingToOriginal(newRating);
            competitor.Volatility = updatedVolatility;

            return competitor;
        }

        private static double ConvertRatingDeviationToOriginal(double glickoRatingDeviation)
        {
            return glickoConversion * glickoRatingDeviation;
        }

        private static double ConvertRatingToOriginal(double glickoRating)
        {
            return (glickoConversion * glickoRating) + 1500;
        }

        private static double CalculateNewRatingDeviation(double preratingDeviation, double variance)
        {
            return 1 / Math.Sqrt((1 / Math.Pow(preratingDeviation, 2)) + (1 / variance));
        }

        private static double CalculateNewRating(GlickoPlayer competitor, List<GlickoOpponent> opponents, double newRatingDeviation)
        {
            var sum = 0.0;

            foreach(var opponent in opponents)
            {
                sum += opponent.GPhi * (opponent.Result - Edeltaphi(competitor.GlickoRating, opponent));
            }

            return competitor.GlickoRating + ((Math.Pow(newRatingDeviation, 2)) * sum);
        }

        private static double CalculatePreRatingDeviation(double ratingDeviation, double updatedVolatility)
        {
            return Math.Sqrt(Math.Pow(ratingDeviation, 2) + Math.Pow(updatedVolatility, 2));
        }

        private static double CalculateNewVolatility(GlickoPlayer competitor, List<GlickoOpponent> opponents, double variance)
        {
            var rankingChange = RatingImprovement(competitor, opponents, variance);
            var rankDeviation = competitor.GlickoRatingDeviation;
            
            var A = VolatilityTransform(competitor.Volatility);
            var a = VolatilityTransform(competitor.Volatility);

            double B;

            if (Math.Pow(rankingChange, 2) > (Math.Pow(competitor.GlickoRatingDeviation, 2) + variance))
            {
                B = Math.Log(Math.Pow(rankingChange, 2) - Math.Pow(competitor.GlickoRatingDeviation, 2) - variance);
            }
            else
            {
                var k = 1;
                double x;

                do {
                    x = a - (k * VolatilityChange);
                    k++;
                } while (VolatilityFunction(x, rankingChange, rankDeviation, variance, competitor.Volatility) < 0);
                B = x;
            }

            var fA = VolatilityFunction(A, rankingChange, rankDeviation, variance, competitor.Volatility);
            var fB = VolatilityFunction(B, rankingChange, rankDeviation, variance, competitor.Volatility);

            while (Math.Abs(B - A) > ConvergenceTolerance)
            {
                var C = A + ((A - B) * fA / (fB - fA));
                var fC = VolatilityFunction(C, rankingChange, rankDeviation, variance, competitor.Volatility);

                if ((fC * fB) < 0)
                {
                    A = B;
                    fA = fB;
                }
                else
                {
                    fA = fA / 2;
                }

                B = C;
                fB = fC;
            }

            return Math.Exp(A / 2);
        }

        private static double VolatilityTransform(double volatility)
        {
            return Math.Log(Math.Pow(volatility, 2));
        }

        private static double VolatilityFunction(double x, double rankingChange, double rankDeviation, double variance, double volatility)
        {
            var leftNumerater = Math.Exp(x) * (Math.Pow(rankingChange, 2) - Math.Pow(rankDeviation, 2) - variance - Math.Exp(x));
            var leftDenominator = 2 * Math.Pow(Math.Pow(rankDeviation, 2) + variance + Math.Exp(x), 2);

            var rightNumerater = x - VolatilityTransform(volatility);
            var rightDenomintor = Math.Pow(VolatilityChange, 2);

            return (leftNumerater / leftDenominator - rightNumerater / rightDenomintor);
        }

        private static double RatingImprovement(GlickoPlayer competitor, List<GlickoOpponent> opponents, double variance)
        {
            double sum = 0;

            foreach (var opponent in opponents)
            {
                sum += opponent.GPhi * (opponent.Result - Edeltaphi(competitor.GlickoRating, opponent));
            }

            return variance * sum;
        }

        private static double ComputeVariance(GlickoPlayer competitor, List<GlickoOpponent> opponents)
        {
            double sum = 0;
            foreach (var opponent in opponents)
            {
                var eDeltaPhi = Edeltaphi(competitor.GlickoRating, opponent);

                sum += Math.Pow(opponent.GPhi, 2) * eDeltaPhi * (1 - eDeltaPhi);
            }

            return Math.Pow(sum, -1);
        }

        private static double Edeltaphi(double playerRating, GlickoPlayer opponent)
        {
            return 1 / (1 + (Math.Exp(-opponent.GPhi * (playerRating - opponent.GlickoRating))));
        }
    }
}