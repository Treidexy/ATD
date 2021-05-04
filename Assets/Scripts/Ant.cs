using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum AntProperty: int
{
	None,

	Flame = 1 << 0,
}

public enum AntType: int
{
	None,

	Black,
	White,
	Blue,
	Green,
	Yellow,
	Brown,
}

public class Ant: MonoBehaviour
{
	public Transform[] checkpoints;
	public AntType type;
	public float speed;
	public AntProperty props;
	public ParticleSystem flameSystem;
	public AudioSource pop;

	[Header("Don't Touch")]
	public int nextCheckIndex;
	public Transform nextCheckpoint;

	private void Start()
	{
		nextCheckpoint = checkpoints[nextCheckIndex];
		UpdateType();

		if (props.HasFlag(AntProperty.Flame))
			flameSystem.Play();
		StartCoroutine(DPSLoop());
	}

	private void FixedUpdate()
	{
		Vector3 dir = nextCheckpoint.position - transform.position;
		transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90);

		if (dir.sqrMagnitude < speed * speed * Time.deltaTime * Time.deltaTime)
		{
			transform.position = nextCheckpoint.position;
			nextCheckIndex++;
			if (nextCheckIndex >= checkpoints.Length)
			{
				GameManager.Instance.Health -= (int)type;
				Destroy(gameObject);
				return;
			}
			nextCheckpoint = checkpoints[nextCheckIndex];
		}
		else
			transform.position += speed * dir.normalized * Time.deltaTime;
	}

	public void Split()
	{
		var ant = AntSpawner.Instance.SpawnAnt(type);
		ant.transform.position = transform.position + (Vector3)Random.insideUnitCircle;
		ant.nextCheckIndex = nextCheckIndex;
		ant.props = props;
	}

	public void Pop(Dart dart)
	{
		if (dart.props.HasFlag(DartProperty.Flame))
		{
			props |= AntProperty.Flame;
			flameSystem.Play();
		}

		for (int i = 0; i < dart.damage; i++)
		{
			dart.pierce--;
			Pop();
		}
	}

	public void Pop(int damage)
	{
		for (int i = 0; i < damage; i++)
			Pop();
	}

	public void Pop()
	{
		GameManager.Instance.Money++;
		pop.Play();

		switch (type)
		{
		case AntType.Black:
			Destroy(gameObject);
			return;
		case AntType.White:
			type = AntType.Black;
			break;
		case AntType.Blue:
			type = AntType.White;
			break;
		case AntType.Green:
			type = AntType.White;
			break;
		case AntType.Yellow:
			type = AntType.Blue;
			Split();
			type = AntType.Green;
			Split();
			break;
		case AntType.Brown:
			type = AntType.Green;
			Split();
			Split();
			Split();
			Split();
			type = AntType.Blue;
			Split();
			break;
		}

		UpdateType();
	}

	public void UpdateType()
	{
		Color matCol = Color.black;
		switch (type)
		{
		case AntType.Black:
			speed = 3;
			matCol = Color.gray;
			break;
		case AntType.White:
			speed = 3;
			matCol = Color.white;
			break;
		case AntType.Blue:
			speed = 4;
			matCol = Color.blue;
			break;
		case AntType.Green:
			speed = 4;
			matCol = Color.green;
			break;
		case AntType.Yellow:
			speed = 8;
			matCol = Color.yellow;
			break;
		case AntType.Brown:
			speed = 5;
			matCol = new Color(210, 105, 30);
			break;
		}

		GetComponent<Renderer>().material.color = matCol;
	}

	private IEnumerator DPSLoop()
	{
		while (true)
		{
			if (props.HasFlag(AntProperty.Flame))
				Pop();

			yield return new WaitForSeconds(1);
		}
	}

	private void OnDrawGizmos()
	{
		if (nextCheckpoint)
		{
			print(nextCheckpoint);
			Gizmos.color = Color.white;
			Gizmos.DrawLine(transform.position, nextCheckpoint.position);
		}
	}
}