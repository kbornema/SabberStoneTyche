using SabberStoneCore.Enums;
using System;
using System.Globalization;

namespace SabberStoneCoreAi.Tyche
{
	public class TyStateWeights
	{	
		public enum WeightType { EmptyField, HealthFactor, DeckFactor, HandFactor, MinionFactor, Count}

		private float[] _weights;

		public float GetWeight(WeightType t) { return _weights[(int)t]; }
		public void SetWeight(WeightType t, float value) { _weights[(int)t] = value; }

		public TyStateWeights()
		{
			_weights = new float[(int)WeightType.Count];
		}

		public TyStateWeights(float defaultValues)
			: this()
		{
			for (int i = 0; i < _weights.Length; i++)
				_weights[i] = defaultValues;
		}

		public TyStateWeights(params float[] defaultValues)
		: this()
		{
			for (int i = 0; i < _weights.Length; i++)
				_weights[i] = defaultValues[i];
		}

		public TyStateWeights(System.Random random, float minValue, float maxValue)
			: this()
		{
			for (int i = 0; i < _weights.Length; i++)
				_weights[i] = random.RandFloat(minValue, maxValue);
		}

		public TyStateWeights(TyStateWeights other)
			: this()
		{
			for (int i = 0; i < _weights.Length; i++)
				_weights[i] = other._weights[i];
		}

		public void Clamp(float min, float max)
		{
			for (int i = 0; i < _weights.Length; i++)
				_weights[i] = Math.Clamp(_weights[i], min, max);
		}

		public override string ToString()
		{
			return ToCsvString(", ");
		}

		public string ToCsvString(string seperator)
		{
			string s = "";

			for (int i = 0; i < _weights.Length; i++)
				s += _weights[i].ToString(CultureInfo.InvariantCulture) + seperator;

			return s;
		}

		public static TyStateWeights UniformRandLerp(TyStateWeights lhs, TyStateWeights rhs, System.Random random, float tMin, float tMax)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
			{
				float t = random.RandFloat(tMin, tMax);
				p._weights[i] = TyUtility.Lerp(lhs._weights[i], rhs._weights[i], t);
			}

			return p;
		}

		public static TyStateWeights UniformLerp(TyStateWeights lhs, TyStateWeights rhs, float t)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = TyUtility.Lerp(lhs._weights[i], rhs._weights[i], t);

			return p;
		}

		public static TyStateWeights NonUniformLerp(TyStateWeights lhs, TyStateWeights rhs, float[] tValues)
		{
			System.Diagnostics.Debug.Assert(tValues.Length >= (int)WeightType.Count);

			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = TyUtility.Lerp(lhs._weights[i], rhs._weights[i], tValues[i]);

			return p;
		}

		public static TyStateWeights operator *(TyStateWeights lhs, float rhs)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = lhs._weights[i] * rhs;

			return p;
		}

		public static TyStateWeights operator /(TyStateWeights lhs, float rhs)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = lhs._weights[i] / rhs;

			return p;
		}

		public static TyStateWeights operator *(TyStateWeights lhs, TyStateWeights rhs)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = lhs._weights[i] * rhs._weights[i];

			return p;
		}

		public static TyStateWeights operator /(TyStateWeights lhs, TyStateWeights rhs)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = lhs._weights[i] / rhs._weights[i];

			return p;
		}

		public static TyStateWeights operator +(TyStateWeights lhs, TyStateWeights rhs)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = lhs._weights[i] + rhs._weights[i];

			return p;
		}

		public static TyStateWeights operator -(TyStateWeights lhs, TyStateWeights rhs)
		{
			TyStateWeights p = new TyStateWeights();

			for (int i = 0; i < p._weights.Length; i++)
				p._weights[i] = lhs._weights[i] - rhs._weights[i];

			return p;
		}

		public static TyStateWeights GetDefault()
		{
			TyStateWeights p = new TyStateWeights(1.0f);
			p.SetWeight(TyStateWeights.WeightType.HealthFactor, 8.7f);
			return p;
		}

		public static TyStateWeights GetHeroBased(CardClass myClass, CardClass enemyClass)
		{
			if (myClass == CardClass.WARRIOR)
			{
				return new TyStateWeights(1.322651f, 4.013905f, 6.45684f, 6.764077f, 7.349644f);
			}

			else if (myClass == CardClass.SHAMAN)
			{
				return new TyStateWeights(6.003592f, 8.370952f, 3.456434f, 5.274337f, 2.222729f);
			}

			else if (myClass == CardClass.MAGE)
			{
				//1.154733f, 5.872955f, 7.000044f, 7.834682f, 10f,
				return new TyStateWeights(0.26369f, 5.311966f, 1.190827f, 6.026119f, 3.145358f);
			}

			//TODO:
			return GetDefault();
		}
	}
}
