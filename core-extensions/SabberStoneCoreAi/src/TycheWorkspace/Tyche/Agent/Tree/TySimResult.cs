using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
    class TySimResult
    {	
		public POGame.POGame state;
		public PlayerTask task;
		public float value;

		public bool IsBuggy { get { return state == null; } }
		
		public TySimResult(POGame.POGame state, PlayerTask task, float value)
		{
			this.state = state;
			this.value = value;
			this.task = task;
		}
	}
}
