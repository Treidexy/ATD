using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Operator: int
{
	None,
	Assign,
	Combine,
	Scale,
}

public class OperationConverter<T>: JsonConverter<Operation<T>>
{
	public static char Sign(Operator op) =>
		op switch
		{
			Operator.Assign => '=',
			Operator.Combine => '+',
			Operator.Scale => '*',
			_ => ' ',
		};

	public static Operator Sign(char op) =>
		op switch
		{
			'=' => Operator.Assign,
			'+' => Operator.Combine,
			'*' => Operator.Scale,
			_ => Operator.None,
		};

	public override Operation<T> ReadJson(JsonReader reader, Type objectType, Operation<T> existingValue, Boolean hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartObject)
			return serializer.Deserialize<Operation<T>>(reader);

		// Do this cuz we default to `Combine`
		return new Operation<T> { value = (T)reader.Value, op = Operator.Combine };
	}

	public override void WriteJson(JsonWriter writer, Operation<T> value, JsonSerializer serializer) {}
}

[Serializable, JsonConverter(typeof(OperationConverter<>))]
public struct Operation<T>
{
	public T value;
	public Operator op;

	public T Resolve(ref T to) =>
		op switch
		{
			Operator.None => to,
			Operator.Assign => to = value,
			Operator.Combine => to = (dynamic)to + value,
			Operator.Scale => to = (dynamic)to * value,
			_ => to,

		};
	public static explicit operator T(Operation<T> op) =>
		op.value;
	public static explicit operator Operator(Operation<T> op) =>
		op.op;
}

[Serializable]
public struct Upgrade
{
	[NonSerialized]
	public Sprite sprite;
	public string image;
	public string name;
	public int price;

	public Operation<DartProperty> props;
	public Operation<float> effectLifetime;
	public Operation<float> reload;
	public Operation<float> kb;
	public Operation<int> dps;
	public Operation<int> damage;
	public Operation<int> pierce;
	public Operation<int> range;
}
