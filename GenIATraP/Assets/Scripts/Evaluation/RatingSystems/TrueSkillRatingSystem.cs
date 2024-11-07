using Evaluators.TournamentOrganizations;
using System.Collections.Generic;
using Moserware.Skills;
using Moserware.Skills.TrueSkill;
using Unity.Mathematics;
using System;
using AgentOrganizations;
using Fitnesses;

namespace Evaluators.RatingSystems
{
    public class TrueSkillRatingSystem : RatingSystem
    {
        public List<TrueSkillPlayer> Players;
        public GameInfo GameInfo;
        public SkillCalculator SkillCalculator;
        public float MinRating;
        public float MaxRating;

        public TrueSkillRatingSystem()
        {
            Players = new List<TrueSkillPlayer>();
            GameInfo = GameInfo.DefaultGameInfo;
            SkillCalculator = new TwoTeamTrueSkillCalculator();
            MinRating = 0;
            MaxRating = 100;
        }

        public override void DefinePlayers(List<TournamentTeam> teams, RatingSystemRating[] initialPlayerRaitings)
        {
            // Find unique tournament players in teams and add them to the list of players
            throw new NotImplementedException();
        }

        public override void UpdateRatings(List<MatchFitness> tournamentMatchFitnesses)
        {
           throw new NotImplementedException();
        }

        public TrueSkillPlayer GetPlayer(int id)
        {
            return Players.Find(player => player.Player.Id.Equals(id));
        }

        public override void DisplayRatings()
        {
            List<TrueSkillPlayer> playersSorted = new List<TrueSkillPlayer>(Players);
            playersSorted.Sort((player1, player2) => player2.Rating.Mean.CompareTo(player1.Rating.Mean));

            foreach (TrueSkillPlayer player in playersSorted)
            {
                UnityEngine.Debug.Log($"Player {player.Player.Id}: {player.Rating}");
            }
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            RatingSystemRating[] fitnesses = new RatingSystemRating[Players.Count];
            for (int i = 0; i < fitnesses.Length; i++)
            {
                fitnesses[i] = new RatingSystemRating(Players[i].Rating.Mean, Players[i].Rating.StandardDeviation);
            }

            return fitnesses;
        }
    }

    public class TrueSkillPlayer
    {
        public Player Player { get; set; }
        public Rating Rating { get; set; }

        public TrueSkillPlayer(Player player, Rating rating)
        {
            Player = player;
            Rating = rating;
        }

        public void UpdateRating(Rating newRating)
        {
            Rating = newRating;
        }
    }
}