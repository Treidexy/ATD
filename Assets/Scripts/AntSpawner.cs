using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AntSpawner: MonoBehaviour
{
	public static AntSpawner Instance { get; private set; }

	public AudioSource antPop;
	public Transform parent;
	public Transform[] checkpoints;
	public GameObject antPrefab;
	public TMP_InputField roundTxt;
	public int round;

	private void Start()
	{
		Instance = this;
		roundTxt.text = round.ToString();
	}

	public void SetRoundFromInput()
	{
		if (!int.TryParse(roundTxt.text, out round))
		{
			round = 1;
			roundTxt.text = "1";
		}
	}

	public void PlayRound(int roundNum)
	{
		round = roundNum;
		PlayRound();
	}

	public void PlayRound() =>
		StartCoroutine(PlayRoundInternal());

	public Boolean RoundOver() =>
		parent.childCount <= 0;

	public Ant SpawnAnt(AntType type)
	{
		var obj = Instantiate(antPrefab, parent);
		obj.transform.position = checkpoints[0].position;

		var ant = obj.GetComponent<Ant>();
		ant.checkpoints = checkpoints;
		ant.pop = antPop;
		ant.type = type;
		return ant;
	}

	public IEnumerator SpawnAnts(AntType type, int count, float interval)
	{
		for (int i = 0; i < count; i++)
		{
			yield return SpawnAnt(type);
			yield return new WaitForSeconds(interval);
		}
	}

	private IEnumerator PlayRoundInternal()
	{
		int moneyEarned = 100;

		switch (round)
		{
		case 1:
			for (int i = 0; i < 14; i++)
			{
				SpawnAnt(AntType.Black);
				yield return new WaitForSeconds(1.2f);
			}
			break;
		case 2:
			for (int i = 0; i < 12; i++)
			{
				SpawnAnt(AntType.Black);
				yield return new WaitForSeconds(1f);
			}
			break;
		case 3:
			for (int i = 0; i < 6; i++)
			{
				SpawnAnt(AntType.White);
				yield return new WaitForSeconds(1.6f);
			}
			break;
		case 4:
			for (int i = 0; i < 20; i++)
			{
				SpawnAnt(AntType.Black);
				yield return new WaitForSeconds(1f);
			}
			break;
		case 5:
			for (int i = 0; i < 10; i++)
			{
				SpawnAnt(AntType.Black);
				yield return new WaitForSeconds(1f);
			}
			for (int i = 0; i < 5; i++)
			{
				SpawnAnt(AntType.White);
				yield return new WaitForSeconds(1.6f);
			}
			break;
		case 6:
			for (int i = 0; i < 10; i++)
			{
				SpawnAnt(AntType.White);
				yield return new WaitForSeconds(1f);
			}
			break;
		case 7:
			for (int i = 0; i < 10; i++)
			{
				SpawnAnt(AntType.Blue);
				yield return new WaitForSeconds(1.6f);
			}
			break;
		case 8:
			for (int i = 0; i < 10; i++)
			{
				SpawnAnt(AntType.White);
				yield return new WaitForSeconds(1.6f);
			}
			for (int i = 0; i < 5; i++)
			{
				SpawnAnt(AntType.Green);
				yield return new WaitForSeconds(1.6f);
			}
			break;
		case 9:
			for (int i = 0; i < 10; i++)
			{
				SpawnAnt(AntType.Green);
				yield return new WaitForSeconds(1.6f);
			}
			break;
		case 10:
			SpawnAnt(AntType.Yellow);
			moneyEarned = 150;
			break;
		case 11:

			break;
		}

		while (!RoundOver())
			yield return new WaitForFixedUpdate();
		GameManager.Instance.Money += moneyEarned;
		round++;

		roundTxt.text = round.ToString();
	}
}
