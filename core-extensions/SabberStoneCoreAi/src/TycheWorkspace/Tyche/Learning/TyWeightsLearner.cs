using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Learning
{
    class TyWeightsLearner
	{
		public float Fitness { get { return _averageWinPercent; } }

		private int _wins = 0;
		private int _matches = 0;
		public float CurWinPercent { get { return (float)_wins / (float)_matches; } }

		private TyStateWeights _weights;
		public TyStateWeights Weights { get { return _weights; } }

		private float _averageWinPercent = 0.0f;
		public float AverageWinPercent { get { return _averageWinPercent; } }

		public int NumPlays { get { return _winPercentHistory.Count; } }

		private int _id = -1;
		public int Id { get { return _id; } }

		private int _generationBorn = -1;
		public int GenerationBorn { get { return _generationBorn; } }

		private List<float> _winPercentHistory = new List<float>();

		public TyWeightsLearner(TyStateWeights p, int generation, int id)
		{
			_id = id;
			_weights = p;
			_generationBorn = generation;
		}

		public TyWeightsLearner(int generation, int id)
			: this(new TyStateWeights(), generation, id)
		{
		}

		public TyWeightsLearner(System.Random random, float minWeight, float maxWeight, int generation, int id)
			: this(new TyStateWeights(random, minWeight, maxWeight), generation, id)
		{
		}

		public static TyStateWeights GetMutatedWeights(TyStateWeights lhs, System.Random random, float deviation)
		{
			var weights = new TyStateWeights();

			for (int i = 0; i < (int)TyStateWeights.WeightType.Count; i++)
			{
				var factorType = (TyStateWeights.WeightType)i;
				float factor = lhs.GetWeight(factorType) + random.RandFloat(-0.5f * deviation, 0.5f * deviation);
				weights.SetWeight(factorType, factor);
			}

			return weights;
		}

		/// <summary>
		/// Chooses weighted (by fitness) random weights from either A or B
		/// </summary>
		public static TyStateWeights GetCrossedWeights(TyWeightsLearner lhs, TyWeightsLearner rhs, System.Random rand)
		{
			TyStateWeights newWeights = new TyStateWeights();

			for (int i = 0; i < (int)TyStateWeights.WeightType.Count; i++)
			{
				var factorType = (TyStateWeights.WeightType)i;
				float weight = rhs._weights.GetWeight(factorType);

				float chanceLhs = lhs.Fitness / (lhs.Fitness + rhs.Fitness);

				if (rand.RandFloat() < chanceLhs)
					weight = lhs._weights.GetWeight(factorType);

				newWeights.SetWeight(factorType, weight);
			}
			
			return newWeights;
		}

		public void BeforeLearn()
		{
			_matches = 0;
			_wins = 0;
		}

		public void AfterLearn(int matches, int wins)
		{
			_matches = matches;
			_wins = wins;

			float winPercent = CurWinPercent;

			_winPercentHistory.Add(winPercent);
			_averageWinPercent = ComputeAverageValue();
		}

		private float ComputeAverageValue()
		{
			float total = 0.0f;

			for (int i = 0; i < _winPercentHistory.Count; i++)
				total += _winPercentHistory[i];

			return total /= _winPercentHistory.Count;
		}
	}
}
