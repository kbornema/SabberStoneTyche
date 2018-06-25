using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Learning
{
    class ParamLearner
	{
		private int _wins = 0;
		private int _matches = 0;
		public float WinPercent { get { return (float)_wins / (float)_matches; } }

		private StateAnalyzerParams _parameter;
		public StateAnalyzerParams Parameter { get { return _parameter; } }

		private float _averageWinPercent = 0.0f;
		public float AverageWinPercent { get { return _averageWinPercent; } }

		private float _minWinPercent = Single.PositiveInfinity;
		public float MinWinPercent { get { return _minWinPercent; } }

		private float _maxWinPercent = Single.NegativeInfinity;
		public float MaxWinPercent { get { return _maxWinPercent; } }

		private int _id = -1;
		public int Id { get { return _id; } }

		private int _generationBorn = -1;
		public int GenerationBorn { get { return _generationBorn; } }

		public ParamLearner(StateAnalyzerParams p, int generation, int id)
		{
			_id = id;
			_parameter = p;
			_generationBorn = generation;
		}

		public ParamLearner(int generation, int id)
			: this(new StateAnalyzerParams(), generation, id)
		{
		}

		public ParamLearner(System.Random random, float minWeight, float maxWeight, int generation, int id)
			: this(new StateAnalyzerParams(random, minWeight, maxWeight), generation, id)
		{
		}

		public void RememberWinPercent()
		{
			float winPercent = WinPercent;

			if (winPercent > _maxWinPercent)
				_maxWinPercent = winPercent;

			if (winPercent < _minWinPercent)
				_minWinPercent = winPercent;

			if(_averageWinPercent == 0.0f)
				_averageWinPercent = winPercent;

			else
				_averageWinPercent = Utility.Lerp(_averageWinPercent, winPercent, 0.5f);
		}

		public void AddStats(int matches, int wins)
		{
			_matches += matches;
			_wins += wins;
		}

		public void ResetStats()
		{
			_matches = 0;
			_wins = 0;
		}

		public static StateAnalyzerParams GetMutatedParams(StateAnalyzerParams lhs, System.Random random, float deviation)
		{
			var parameter = new StateAnalyzerParams();

			for (int i = 0; i < (int)StateAnalyzerParams.FactorType.Count; i++)
			{
				var factorType = (StateAnalyzerParams.FactorType)i;
				float factor = lhs.GetFactor(factorType) + random.RandFloat(-0.5f * deviation, 0.5f * deviation);
				parameter.SetFactor(factorType, factor);
			}

			return parameter;
		}

		public static StateAnalyzerParams GetCrossedParams(ParamLearner lhs, ParamLearner rhs, System.Random rand)
		{
			StateAnalyzerParams newParameter = new StateAnalyzerParams();

			for (int i = 0; i < (int)StateAnalyzerParams.FactorType.Count; i++)
			{
				var factorType = (StateAnalyzerParams.FactorType)i;
				float weight = rhs._parameter.GetFactor(factorType);

				float chanceLhs = lhs.WinPercent / (lhs.WinPercent + rhs.WinPercent);

				if (rand.RandFloat() < chanceLhs)
					weight = lhs._parameter.GetFactor(factorType);

				newParameter.SetFactor(factorType, weight);
			}
			
			return newParameter;
		}
    }
}
