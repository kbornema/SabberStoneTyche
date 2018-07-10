using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
    static class TyConst
    {
		//TODO: deactivate before the competition:

		public const bool LOG_UNKNOWN_CORRECTIONS = true;
		public const bool LOG_SIMULATION_TIME_BREAKS = true;
		
		public const double MAX_EPISODE_TIME = 5.0f;
		public const double MAX_SIMULATION_TIME = 20.0f;
		public const double MAX_TURN_TIME = 60.0;

		public const double DECREASE_SIMULATION_TIME = MAX_SIMULATION_TIME * 0.4;
	}
}
