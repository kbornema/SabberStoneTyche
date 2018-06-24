using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
	public class StateAnalyzerParams
	{	
		public enum FactorType { EmptyField, HealthFactor, DeckFactor, HandFactor, MinionFactor, Count}

		private float[] _factors;

		public float GetFactor(FactorType t) { return _factors[(int)t]; }
		public void SetFactor(FactorType t, float value) { _factors[(int)t] = value; }

		public StateAnalyzerParams()
		{
			_factors = new float[(int)FactorType.Count];
		}

		public StateAnalyzerParams(float defaultValues)
			: this()
		{
			for (int i = 0; i < _factors.Length; i++)
				_factors[i] = defaultValues;
		}

		public StateAnalyzerParams(params float[] defaultValues)
		: this()
		{
			for (int i = 0; i < _factors.Length; i++)
				_factors[i] = defaultValues[i];
		}

		public StateAnalyzerParams(System.Random random, float minValue, float maxValue)
			: this()
		{
			for (int i = 0; i < _factors.Length; i++)
				_factors[i] = random.RandFloat(minValue, maxValue);
		}

		public StateAnalyzerParams(StateAnalyzerParams other)
			: this()
		{
			for (int i = 0; i < _factors.Length; i++)
				_factors[i] = other._factors[i];
		}

		public void Clamp(float min, float max)
		{
			for (int i = 0; i < _factors.Length; i++)
				_factors[i] = Math.Clamp(_factors[i], min, max);
		}

		public override string ToString()
		{
			string s = "";

			for (int i = 0; i < _factors.Length; i++)
				s += _factors[i].ToString(CultureInfo.InvariantCulture) + ", ";

			return s;
		}

		public static StateAnalyzerParams UniformRandLerp(StateAnalyzerParams lhs, StateAnalyzerParams rhs, System.Random random, float tMin, float tMax)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
			{
				float t = random.RandFloat(tMin, tMax);
				p._factors[i] = Utility.Lerp(lhs._factors[i], rhs._factors[i], t);
			}

			return p;
		}

		public static StateAnalyzerParams UniformLerp(StateAnalyzerParams lhs, StateAnalyzerParams rhs, float t)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = Utility.Lerp(lhs._factors[i], rhs._factors[i], t);

			return p;
		}

		public static StateAnalyzerParams NonUniformLerp(StateAnalyzerParams lhs, StateAnalyzerParams rhs, float[] tValues)
		{
			System.Diagnostics.Debug.Assert(tValues.Length >= (int)FactorType.Count);

			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = Utility.Lerp(lhs._factors[i], rhs._factors[i], tValues[i]);

			return p;
		}

		public static StateAnalyzerParams operator *(StateAnalyzerParams lhs, float rhs)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = lhs._factors[i] * rhs;

			return p;
		}

		public static StateAnalyzerParams operator /(StateAnalyzerParams lhs, float rhs)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = lhs._factors[i] / rhs;

			return p;
		}

		public static StateAnalyzerParams operator *(StateAnalyzerParams lhs, StateAnalyzerParams rhs)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = lhs._factors[i] * rhs._factors[i];

			return p;
		}

		public static StateAnalyzerParams operator /(StateAnalyzerParams lhs, StateAnalyzerParams rhs)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = lhs._factors[i] / rhs._factors[i];

			return p;
		}

		public static StateAnalyzerParams operator +(StateAnalyzerParams lhs, StateAnalyzerParams rhs)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = lhs._factors[i] + rhs._factors[i];

			return p;
		}

		public static StateAnalyzerParams operator -(StateAnalyzerParams lhs, StateAnalyzerParams rhs)
		{
			StateAnalyzerParams p = new StateAnalyzerParams();

			for (int i = 0; i < p._factors.Length; i++)
				p._factors[i] = lhs._factors[i] - rhs._factors[i];

			return p;
		}
	}
}
