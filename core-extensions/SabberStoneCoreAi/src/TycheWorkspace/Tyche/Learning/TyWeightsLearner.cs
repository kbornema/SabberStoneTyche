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

		private float _minWinPercent = Single.PositiveInfinity;
		public float MinWinPercent { get { return _minWinPercent; } }

		private float _maxWinPercent = Single.NegativeInfinity;
		public float MaxWinPercent { get { return _maxWinPercent; } }

		private int _numPlays = 0;
		public int NumPlays { get { return _numPlays; } }

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
			_numPlays++;

			_matches = matches;
			_wins = wins;

			float winPercent = CurWinPercent;

			if (winPercent > _maxWinPercent)
				_maxWinPercent = winPercent;

			if (winPercent < _minWinPercent)
				_minWinPercent = winPercent;

			//TODO: maybe rather weight with how many times played (and thus have a more accurate estimate), otherwise new
			//individuals that are "lucky" might kick out old but accurate/steady individuals

			//0 -> never trust new values, 1 -> never trust old values
			const float TRUST_VALUE = 0.33f;

			if (_averageWinPercent == 0.0f)
				_averageWinPercent = winPercent;
			else
				_averageWinPercent = TyUtility.Lerp(_averageWinPercent, winPercent, TRUST_VALUE);
		}
	}
}
