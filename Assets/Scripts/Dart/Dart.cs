using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum DartProperty
{
	None,

	Camo = 1 << 0,
	Flame = 1 << 1,
	Wet = 1 << 2,
	Rinse = 1 << 3,
	Ricochet = 1 << 4,
}

public enum DartType
{
	Sharp,
	Explosive,
	HyperSonic,
}

public class Dart: MonoBehaviour
{
	public GameObject explosionPrefab;
	public DartProperty props;
	public DartType type;
	public int pierce;
	public int damage;
	public int blast;
	public int dps;
	public float kb;
	public float stick;
	public float explosion;
	public float speed;
	public float timeout;

	[Header("NO TOUCH")]
	public Vector3 startPos;
	public List<Transform> hit;
	public Vector3 dir;
	public Rigidbody2D rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		startPos = transform.position;
	}

	private void FixedUpdate()
	{
		if (props.HasFlag(DartProperty.Ricochet))
		{
			if (AntSpawner.Instance.parent.childCount <= 0)
			{
				Destroy(gameObject);
				return;
			}

			Transform ant = null;
			for (int i = 0; i < AntSpawner.Instance.parent.childCount; i++)
			{
				ant = AntSpawner.Instance.parent.GetChild(i);
				if (!hit.Contains(ant))
					break;
			}

			if (ant is null)
				Destroy(gameObject);

			dir = ant.position - transform.position;
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
		}

		rb.velocity = speed * dir * GameManager.FixedDeltaTime;
		if (!props.HasFlag(DartProperty.Ricochet) && Vector3.Distance(transform.position, startPos) >= timeout + 1)
			Destroy(gameObject, Time.fixedDeltaTime);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (pierce <= 0)
		{
			Destroy(gameObject);
			return;
		}

		if (other.gameObject.layer != LayerMask.NameToLayer("Ant"))
			return;

		if (explosion > 0)
		{
			var obj = Instantiate(explosionPrefab);
			obj.transform.position = transform.position;

			var explosion = obj.GetComponent<Explosion>();
			explosion.stick = stick;
			explosion.explosion = this.explosion;
			explosion.damage = damage;
			explosion.props = props;
			explosion.blast = blast;
			explosion.dps = dps;
		}

		var ant = other.GetComponent<Ant>();
		hit.Add(ant.transform);
		ant.Pop(this);

		if (pierce <= 0)
			Destroy(gameObject);
	}
}
