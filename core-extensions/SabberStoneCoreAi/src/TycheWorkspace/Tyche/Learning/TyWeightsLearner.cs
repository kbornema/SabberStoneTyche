using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Learning
{
    class TyWeightsLearner
	{
		private int _wins = 0;
		private int _matches = 0;
		public float WinPercent { get { return (float)_wins / (float)_matches; } }

		private TyStateWeights _weights;
		public TyStateWeights Weights { get { return _weights; } }

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

		public void RememberWinPercent()
		{
			float winPercent = WinPercent;

			if (winPercent > _maxWinPercent)
				_maxWinPercent = winPercent;

			if (winPercent < _minWinPercent)
				_minWinPercent = winPercent;

			if(_averageWinPercent == 0.0f)
				_averageWinPercent = winPercent;

			//TODO: maybe rather weight with how many times played (and thus have a more accurate estimate), otherwise new
			//individuals that have "luck" might kick out old / steady individuals:
			else 
				_averageWinPercent = TyUtility.Lerp(_averageWinPercent, winPercent, 0.5f);
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

		//returns random (in correlation to WinPercent) weight from either A or B 
		public static TyStateWeights GetCrossedWeights(TyWeightsLearner lhs, TyWeightsLearner rhs, System.Random rand)
		{
			TyStateWeights newWeights = new TyStateWeights();

			for (int i = 0; i < (int)TyStateWeights.WeightType.Count; i++)
			{
				var factorType = (TyStateWeights.WeightType)i;
				float weight = rhs._weights.GetWeight(factorType);

				float chanceLhs = lhs.WinPercent / (lhs.WinPercent + rhs.WinPercent);

				if (rand.RandFloat() < chanceLhs)
					weight = lhs._weights.GetWeight(factorType);

				newWeights.SetWeight(factorType, weight);
			}
			
			return newWeights;
		}
    }
}
