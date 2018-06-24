using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.POGame;
using SabberStoneCoreAi.Tyche.Testing;
using System;
using System.Collections.Generic;

namespace SabberStoneCoreAi.Tyche
{
	/// <summary> An <see cref="AbstractAgent"/> that simulates each possible <see cref="PlayerTask"/> (only one step deep), and chooses the best <see cref="PlayerTask"/> according to <see cref="StateAnalyzer"/>, </summary>
	class TycheAgent : AbstractAgent
	{
		private Random _random;

		private StateAnalyzer _analyzer;
		public StateAnalyzer Analyzer { get { return _analyzer; } }

		private bool _hasInitialized;
		private POGame.POGame _initialState;

		public TycheAgent()
		{
			_analyzer = StateAnalyzer.GetDefault();
			_random = new Random();
		}
		
		private PlayerTask GetGreedyBestTask(POGame.POGame poGame)
		{
			var options = poGame.CurrentPlayer.Options();

			//if there is only one option, then there is nothing to choose:
			if (options.Count == 1)
				return options[0];

			var simulationResults = poGame.Simulate(options);

			int bestStateIndex = -1;
			float bestStateValue = Single.NegativeInfinity;


			List<PlayerTask> buggyTasks = new List<PlayerTask>();
			HashSet<PlayerTask> isLosingTask = new HashSet<PlayerTask>();

			for (int i = 0; i < options.Count; i++)
			{
				var resultState = simulationResults[options[i]];

				if (resultState == null)
				{
					buggyTasks.Add(options[i]);
					continue;
				}
							
				float stateValue = 0.0f;

				//after END_TURN the players will be swapped:
				if (options[i].PlayerTaskType == PlayerTaskType.END_TURN)
					stateValue = _analyzer.GetStateValue(resultState, resultState.CurrentOpponent, resultState.CurrentPlayer);
				else
					stateValue = _analyzer.GetStateValue(resultState, resultState.CurrentPlayer, resultState.CurrentOpponent);

				//if the player wins, just choose this Task immediately:
				if (Single.IsPositiveInfinity(stateValue))
					return options[i];

				if (Single.IsNegativeInfinity(stateValue))
					isLosingTask.Add(options[i]);

				if (bestStateIndex == -1 || stateValue > bestStateValue)
				{	
					bestStateValue = stateValue;
					bestStateIndex = i;
				}
			}

			//could not find a best state, either all of them are buggy or they are losing states:
			if (bestStateIndex == -1)
			{	
				var tasksToChoose = new List<PlayerTask>(options);

				//remove all tasks where the player loses:
				tasksToChoose.RemoveAll(x => isLosingTask.Contains(x));

				//if there is no non-losing task left, choose any task
				if(tasksToChoose.Count == 0)
					return options.GetUniformRandom(_random);

				//prefer the tasks where the player doesn't lose:
				return tasksToChoose.GetUniformRandom(_random);
			}

			if(buggyTasks.Count > 0)
			{
				//TODO: in case of warrior it might be better, to use actually other PlayerTasks that have resulted in null, of the best option is not THAT promising
				//TODO: find out if the task is promising in case of buggy PlayerTasks:
				//e.g. compute average state values, and pick a random action of the best action is below average
			}

			return options[bestStateIndex];
		}

		private PlayerTask GetRandomTaskNonTurnEnd(POGame.POGame poGame, List<PlayerTask> options)
		{
			if (options.Count == 1)
				return options[0];

			int curId = _random.Next(options.Count);
			PlayerTask curTask = options[curId];

			if (curTask.PlayerTaskType == PlayerTaskType.END_TURN)
				options.RemoveAt(curId);
			else
				return curTask;

			return GetRandomTaskNonTurnEnd(poGame, options);
		}

		public override PlayerTask GetMove(POGame.POGame poGame)
		{
			if (!_hasInitialized)
				CustomInit(poGame);

			return GetGreedyBestTask(poGame);
		}

		private void CustomInit(POGame.POGame poGame)
		{
			_hasInitialized = true;
			_initialState = poGame;

			//TODO: find out who is playing against who and choose StateAnalyzer weights accordingly:
		}

		public override void InitializeGame()
		{
			_hasInitialized = false;
		}

		public override void InitializeAgent() { }
		public override void FinalizeAgent() { }
		public override void FinalizeGame() { }
	}
}
