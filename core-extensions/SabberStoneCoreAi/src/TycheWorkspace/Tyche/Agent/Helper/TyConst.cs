using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
    static class TyConst
    {
		public const bool LOG_UNKNOWN_CORRECTIONS = false;
		public const bool LOG_UNKNOWN_SECRETS = false;

		public const double MAX_EPISODE_TIME = 5.0f;
		public const double MAX_SIMULATION_TIME = 20.0f;
		public const double MAX_TURN_TIME = 50.0;

		public const double DECREASE_SIMULATION_TIME = MAX_SIMULATION_TIME * 0.4;
	}
}
