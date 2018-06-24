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

		public ParamLearner(StateAnalyzerParams p)
		{
			_parameter = p;
		}

		public ParamLearner()
			: this(new StateAnalyzerParams())
		{
		}

		public ParamLearner(System.Random random, float minWeight, float maxWeight)
			: this(new StateAnalyzerParams(random, minWeight, maxWeight))
		{
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
