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
			p.SetWeight(WeightType.HealthFactor, 8.7f);
			return p;
		}

		public static TyStateWeights GetHeroBased(CardClass myClass, CardClass enemyClass)
		{
			if (myClass == CardClass.WARRIOR)
			{
				//1): 1.580433f, 8.778511f,	7.78354f, 7.201142f, 8.289609f
				//2): 1.495044f, 9.781904f, 6.516459f, 8.087907f, 7.545108f
				//3): 0.0f, 8.267946f, 10.0f, 3.935781f, 5.104651f
				//2,453197	6,553556	9,408744	1,562617	5,105491
				return new TyStateWeights(2.453197f,  6.553556f, 9.408744f, 1.562617f, 5.105491f);
			}

			else if (myClass == CardClass.SHAMAN)
			{
				//23	4	12	0,6		0,42	0,66	0,5483057	4,104358	8,032728	7,645365	8,195593	7,138855
				//20	3	12	0,56	0,36	0,64	0,5214015	1,612406	9,560477	10.0f		6,303699	4,849987
				//37	7   8   0,66    0,44    0,72    0,6052518   0,1244369   9,38691		0,9763278   1,769793    1,528298

				return new TyStateWeights(6.003592f, 8.370952f, 3.456434f, 5.274337f, 2.222729f);
			}

			else if (myClass == CardClass.MAGE)
			{
				//old: 0.26369f, 5.311966f, 1.190827f, 6.026119f, 3.145358f
				return new TyStateWeights(6.917449f, 8.021517f, 10.0f, 7.317904f, 7.429502f);
			}

			else if(myClass == CardClass.DRUID)
			{
				//3.740027f, 2.664373f, 5.931867f, 3.657737f, 9.264104f
				return new TyStateWeights(3.740027f, 2.664373f, 5.931867f, 3.657737f, 9.264104f);
			}

			else if(myClass == CardClass.PRIEST)
			{
				//1.551631f, 8.070081f, 0.527284f, 2.586267f, 2.014101f
				return new TyStateWeights(1.551631f, 8.070081f, 0.527284f, 2.586267f, 2.014101f);
			}

			else if(myClass == CardClass.WARLOCK)
			{
				//TODO:
				// 4.970002f, 5.345061f, 0.9560884f, 4.134902f, 5.900652f
				return new TyStateWeights(4.970002f, 5.345061f, 0.9560884f, 4.134902f, 5.900652f);
			}

			else if(myClass == CardClass.PALADIN)
			{
				// 2	0	20	0,44	0,3		0,6		0.407714	2.544867	1.187623	4.4971		8.914433	9.156882
				//29	5	12	0,54	0,38	0,54	0.4902952	1.797979	8.836073	4.400247	3.894981	9.119069
				//28	5	10	0,44	0,4		0,56	0.4446335	7.338588	10.0		6.420178	2.121965	2.288175
				//9		0   20  0,42    0,34    0,64    0.4465979   1.71998		7.632212    0.526979    1.348682    2.545016
				//13	1	16	0,46	0,3		0,64	0,5036716	7,871994	8,671459	2,132355	6,525496	4,081241
				return GetDefault();
			}

			//TODO:
			return GetDefault();
		}
	}
}
