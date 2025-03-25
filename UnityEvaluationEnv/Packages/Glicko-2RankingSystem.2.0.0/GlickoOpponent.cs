namespace Glicko2
{
    public class GlickoOpponent : GlickoPlayer
    {
        public GlickoPlayer Opponent;
        public double Result;

        public GlickoOpponent(GlickoPlayer opponent, double result) : base(opponent.Rating, opponent.RatingDeviation, opponent.Volatility)
        {
            Result = result;
        }

        public GlickoOpponent(double rating, double ratingDeviation, double result, double volatility = 0.06) : base(rating, ratingDeviation, volatility)
        {
            Result = result;
        }
    }    
}