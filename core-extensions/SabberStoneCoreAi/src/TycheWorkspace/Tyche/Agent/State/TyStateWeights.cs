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
			System.Diagnostics.Debug.Assert(defaultValues.Length == (int)WeightType.Count);

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
			p.SetWeight(WeightType.HealthFactor, 8.7f);
			return p;
		}

		public static TyStateWeights GetHeroBased(CardClass myClass, CardClass enemyClass)
		{
			if (myClass == CardClass.WARRIOR)
			{	
				return new TyStateWeights(3.283574f, 7.592678f, 6.853167f, 4.785677f, 3.926729f);
			}

			else if (myClass == CardClass.SHAMAN)
			{
				//17	2	18	0,58	0,42	0,7		0,527578	2,056115	9,752507	2,82615		3,83917		2,029905
				//16	2   18  0,54    0,36    0,68    0,5258836   2,514469    8,157766    8,79034		3,404604    1,919119
				return new TyStateWeights(2.056115f, 9.752507f, 2.82615f, 3.83917f, 2.029905f);
			}

			else if (myClass == CardClass.MAGE)
			{
				return new TyStateWeights(3.973221f, 9.552423f, 9.033211f, 2.2092f, 1.850587f);
			}

			else if(myClass == CardClass.DRUID)
			{
				//17	2	18	0,5		0,42	0,64	0,5441086	2,14698		10.0f		6,135773	6,071639	8,04523
				//2		0	20	0,5		0,4		0,7		0,5469287	1.995913	4.501529	1.888616	1.096681	3.516505
				//12	1	20	0,5		0,4		0,7		0,541828	0.9008623	2.841606	8.855236	5.318462	4.801469
				//7		0	20	0,56	0,38	0,68	0,5475698	2.761237	1.794317	2.943055	5.687263	6.699563
				return new TyStateWeights(1.995913f, 4.501529f, 1.888616f, 1.096681f, 3.516505f);
			}

			else if(myClass == CardClass.PRIEST)
			{
				//TODO: checked, but worse
				return GetDefault();
			}

			else if(myClass == CardClass.WARLOCK)
			{
				// 0	0   20  0,56    0,32    0,6		0,5269238   1,832379    1,650257    9,868885    5,680438    8,190629
				// 16	2	18	0,6		0,34	0,62	0,5419836	9,224876	4,635609	1,862006	5,790635	5,673455
				// 11	1	20	0,52	0,38	0,66	0,5551535	6.338876f	8.568761f	1.863452f	3.182807f	4.967152f
				return new TyStateWeights(6.338876f, 8.568761f, 1.863452f, 3.182807f, 4.967152f);
			}

			else if(myClass == CardClass.PALADIN)
			{
				//TODO: checked, but worse
				return GetDefault();
			}
			
			return GetDefault();
		}
	}
}
