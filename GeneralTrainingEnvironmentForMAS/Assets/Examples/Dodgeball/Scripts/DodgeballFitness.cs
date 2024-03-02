using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DodgeballFitness : Fitness {
    /*
     Parameter	Description
        - ball_hold_bonus	(default = 0.0) A reward given to an agent at every timestep for each ball it is holding.
        - is_capture_the_flag	Set this parameter to 1 to override the scene's game mode setting, and change it to Capture the Flag. Set to 0 for Elimination.
        - time_bonus_scale	(default = 1.0 for Elimination, and 0.0 for CTF) Multiplier for negative reward given for taking too long to finish the game. Set to 1.0 for a -1.0 reward if it takes the maximum number of steps to finish the match.
        - elimination_hit_reward	(default = 0.1) In Elimination, a reward given to an agent when it hits an opponent with a ball.
        - stun_time	(default = 10.0) In Capture the Flag, the number of seconds an agent is stunned for when it is hit by a ball.
        - opponent_has_flag_penalty	(default = 0.0) In Capture the Flag, a penalty (negative reward) given to the team at every timestep if an opponent has their flag. Use a negative value here.
        - team_has_flag_bonus	(default = 0.0) In Capture the Flag, a reward given to the team at every timestep if one of the team members has the opponent's flag.
        - return_flag_bonus	(default = 0.0) In Capture the Flag, a reward given to the team when it returns their own flag to their base, after it has been dropped by an opponent.
        - ctf_hit_reward	(default = 0.02) In Capture the Flag, a reward given to an agent when it hits an opponent with a ball.
     */


}