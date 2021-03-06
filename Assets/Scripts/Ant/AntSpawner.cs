using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public partial class AntSpawner: MonoBehaviour
{
	public static AntSpawner Instance { get; private set; }

	public Transform parent;
	public Transform checkpointsParent;
	public Transform[] checkpoints;
	public GameObject antPrefab;
	public TMP_InputField roundTxt;
	public Round[] rounds;
	public int round;

	private void Start()
	{
		Instance = this;
		LoadRounds();
		roundTxt.text = round.ToString();

		checkpoints = new Transform[checkpointsParent.childCount];
		for (int i = 0; i < checkpointsParent.childCount; i++)
			checkpoints[i] = checkpointsParent.GetChild(i);
	}

	public void EarthQuake() =>
		StartCoroutine(CoEarthQuake());

	public IEnumerator CoEarthQuake()
	{
		float start = Time.time;
		while (Time.time - start < 1f)
		{
			for (int i = 0; i < parent.childCount; i++)
				parent.GetChild(i).position += (Vector3)UnityEngine.Random.insideUnitCircle;

			yield return new WaitForFixedUpdate();
		}
	}

	public void SetRoundFromInput()
	{
		if (int.TryParse(roundTxt.text, out round))
			return;

		round = 1;
		roundTxt.text = "1";
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

	public Ant SpawnAnt(AntType type, AntProperty props = AntProperty.None)
	{
		var obj = Instantiate(antPrefab, parent);
		obj.transform.position = checkpoints[0].position;

		var ant = obj.GetComponent<Ant>();
		ant.checkpoints = checkpoints;
		ant.props = props;
		ant.type = type;
		return ant;
	}

	private IEnumerator SpawnGroup(Wave wave)
	{
		for (int i = 0; i < wave.count; i++)
		{
			SpawnAnt(wave.type, wave.props);
			yield return new WaitForSeconds(wave.interval);
		}
	}

	private IEnumerator PlayRoundInternal()
	{
		if (round > rounds.Length)
			yield break;

		int reward = rounds[round - 1].reward;
		foreach (var group in rounds[round - 1].waves)
			yield return SpawnGroup(group);

		while (!RoundOver())
			yield return new WaitForFixedUpdate();
		GameManager.Instance.money += reward;
		round++;

		roundTxt.text = round.ToString();
	}
}
